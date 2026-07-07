using System.Text;
using demo_app_crud.Server.Auth;
using demo_app_crud.Server.Middleware;
using Demo.DAL;
using Demo.DAL.Data;
using Demo.Services;
using Demo.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ----- Configuration -----
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=demo.db";

// ----- Layers (DI) -----
builder.Services.AddDataAccess(connectionString);          // DAL: DbContext, repositories, password hasher
builder.Services.AddScoped<IProductService, ProductService>(); // Services (business logic)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// ----- Authentication / Authorization -----
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
        };
    });
builder.Services.AddAuthorization();

// ----- Web / API -----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo CRUD API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT returned by /api/auth/login."
    });
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc, null), new List<string>() }
    });
});

var app = builder.Build();

// ----- Pipeline -----
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseDefaultFiles();
app.MapStaticAssets();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("/index.html");

// ----- Seed demo data (skipped for the in-memory test host) -----
if (app.Environment.EnvironmentName != "Testing")
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(db, hasher);
}

app.Run();

// Exposed so the integration-test project can reference the entry point via WebApplicationFactory<Program>.
public partial class Program { }
