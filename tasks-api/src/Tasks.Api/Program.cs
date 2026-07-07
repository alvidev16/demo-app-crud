using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Tasks.Api.Auth;
using Tasks.Api.Middleware;
using Tasks.DAL;
using Tasks.Services;
using Tasks.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ----- Configuration -----
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tasks.db";

// ----- Layers -----
builder.Services.AddDataAccess(connectionString);              // DAL
builder.Services.AddScoped<ITaskService, TaskService>();       // Services
builder.Services.AddSingleton(TimeProvider.System);            // clock for the due-date rule
builder.Services.AddSingleton<JwtTokenService>();

// ----- Auth -----
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
    });
builder.Services.AddAuthorization();

// Serialize/accept enums as their names ("Todo"/"InProgress"/"Done"), not integers,
// so the API contract is human-readable and clients can send status names.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ----- Swagger / OpenAPI (with JWT bearer support) -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tasks API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT returned by POST /api/auth/token."
    });
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        // Pass the host document so the $ref resolves to #/components/securitySchemes/Bearer;
        // without it the requirement is dropped and Swagger UI never attaches the token.
        { new OpenApiSecuritySchemeReference("Bearer", doc, null), new List<string>() }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Apply migrations on startup (skipped under the in-memory test host).
if (app.Environment.EnvironmentName != "Testing")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
    db.Database.Migrate();
}

app.Run();

// Exposed for WebApplicationFactory<Program> in the integration tests.
public partial class Program { }
