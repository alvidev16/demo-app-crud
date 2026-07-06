# 04 · Implementation Prompt

> **Phase:** `implement` · In Spec-Driven Development the code is generated **from the
> approved artifacts**, not from an ad-hoc description. This prompt hands the AI the
> spec, the plan, and the task list, and asks it to implement **one task at a time**,
> test-first, staying traceable to the requirement IDs.

## Constitution (guardrails — apply to every step)

````text
- The specification (01-spec.md) is the source of truth. If code and spec disagree,
  the spec wins; if the spec is ambiguous, STOP and ask — do not guess.
- Follow the plan (02-plan.md): Layered architecture with the Clean dependency rule.
  Business rules never depend on EF Core or ASP.NET.
- TDD: for each task, write the failing test(s) FIRST, then the minimal code to pass.
- Naming: `TaskItem` (not `Task`), `TaskState` (not `TaskStatus`) — avoid BCL collisions.
- Errors as RFC-7807 ProblemDetails; validation errors carry a field→message map.
- A resource the caller doesn't own is reported as 404, never 403 (no existence leak).
- No hard-coded secrets. Do NOT invent NuGet/API surfaces — if unsure, stop and say so.
- Keep each change minimal, compiling, and green before moving to the next task.
````

## Implementation prompt

````text
You are a senior .NET engineer implementing a Task Management API using
Spec-Driven Development. Read these three artifacts and treat them as binding:

  - 01-spec.md   → requirements & acceptance criteria (WHAT/WHY)
  - 02-plan.md   → architecture & technical decisions (HOW)
  - 03-tasks.md  → the ordered task list with traceability

Work through 03-tasks.md IN ORDER, one task at a time. For each task:
  1. State the task ID and the requirement IDs it satisfies.
  2. Write the failing test(s) named after the acceptance criteria (e.g. the
     Todo→Done rejection covers AC-4).
  3. Implement the minimal code to make them pass.
  4. Report: which tests are green, and confirm the task's DoD is met.
  5. Stop and wait for my "next" before starting the following task.

Apply the Constitution above at every step. When you finish T-14, produce the
traceability check: every FR/NFR maps to at least one passing test.
````

## Follow-up prompts

````text
- "T-03 looks done, but AC-3 (past due date) isn't asserted. Add the test, then continue."
- "Refactor T-08 for the duplication you'd flag in review — behavior must stay green."
- "Update 01-spec.md: Done is now reopenable to In Progress. Re-derive the affected
   tasks and tests before changing code."   ← spec changes first, code follows
````

## Why this is Spec-Driven (not just a good prompt)

| Property | This deliverable |
|---|---|
| Spec is a **durable, versioned artifact** | `01-spec.md`, separate from the prompt |
| **Separation** of what/why from how | spec vs. plan |
| **Traceability** | every task and test cites FR/NFR/AC IDs |
| **Spec-first change control** | the last follow-up edits the spec *before* the code |
| Code is generated **from** the spec | the implementation prompt consumes the artifacts |
