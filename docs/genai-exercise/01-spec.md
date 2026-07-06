# 01 · Specification — Task Management API

> **Phase:** `specify` · Describes **what** the system must do and **why** — no
> technology or implementation choices. This document is the source of truth;
> the plan and tasks trace back to the requirement IDs defined here.

## 1. Problem statement

Users need a simple way to keep track of their personal work items ("tasks").
Each user manages their own list and must not be able to see or change anyone
else's tasks.

## 2. Actors

| Actor | Description |
|---|---|
| **User** | An authenticated person who owns and manages their own tasks. Assumed to already exist in the system. |

## 3. User stories

- **US-1** — As a user, I want to **create** a task (title, description, status, due date) so I can record work I need to do.
- **US-2** — As a user, I want to **list and view** my tasks so I can see what's pending.
- **US-3** — As a user, I want to **update** a task's details so I can keep it accurate.
- **US-4** — As a user, I want to **change a task's status** so I can reflect progress.
- **US-5** — As a user, I want to **delete** a task I no longer need.
- **US-6** — As a user, I want my tasks to be **private to me**, so no one else can read or modify them.

## 4. Functional requirements

| ID | Requirement |
|---|---|
| **FR-1** | A task has: a unique identifier, a **title**, an optional **description**, a **status**, a **due date**, and an **owner** (the user it belongs to). |
| **FR-2** | Title is required (3–120 characters). Description is optional (≤ 1000 characters). |
| **FR-3** | Status is one of: **Todo**, **In Progress**, **Done**. A new task starts as **Todo**. |
| **FR-4** | Status changes follow a state machine: **Todo → In Progress**, **In Progress → Done**, **In Progress → Todo** (reopen). Any other change is rejected. |
| **FR-5** | A task's **due date cannot be in the past** at creation time. |
| **FR-6** | Users can **create, read, update, and delete** tasks (full CRUD). |
| **FR-7** | A user may only read, update, delete, or change the status of **their own** tasks. |
| **FR-8** | Every task operation requires the caller to be **authenticated**. |

## 5. Acceptance criteria (behavioral)

- **AC-1** (FR-2, FR-5) — *Given* a title of 3–120 chars and a due date today or later, *when* the user creates a task, *then* it is created with status **Todo**.
- **AC-2** (FR-2) — *Given* an empty or too-long title, *when* the user creates a task, *then* the request is rejected with a field-level validation error and nothing is stored.
- **AC-3** (FR-5) — *Given* a due date in the past, *when* the user creates a task, *then* the request is rejected with a validation error.
- **AC-4** (FR-4) — *Given* a task in **Todo**, *when* the user tries to move it directly to **Done**, *then* the change is rejected as an invalid transition.
- **AC-5** (FR-4) — *Given* a task in **In Progress**, *when* the user moves it to **Done**, *then* the status becomes **Done**.
- **AC-6** (FR-7) — *Given* a task owned by another user, *when* the user requests it, *then* the system responds as if it **does not exist** (no data is revealed).
- **AC-7** (FR-8) — *Given* an unauthenticated caller, *when* any task endpoint is called, *then* access is denied.

## 6. Non-functional requirements

| ID | Requirement |
|---|---|
| **NFR-1** | Business rules (validation, status transitions, ownership) must be **verifiable in isolation**, independent of storage or transport. |
| **NFR-2** | Errors are returned in a **consistent, machine-readable** shape, and validation errors identify the offending field(s). |
| **NFR-3** | Requesting a resource the caller does not own must **not reveal its existence** (treated as "not found"). |
| **NFR-4** | **No secrets** (keys, connection strings) are hard-coded in source. |
| **NFR-5** | The behavior described by the acceptance criteria must be covered by **automated tests**, written **test-first**. |

## 7. Out of scope

Task sharing/collaboration, comments, attachments, reminders/notifications, tags,
sub-tasks, and user account management. These may be future specs.

## 8. Assumptions

- A **User** model and an **authentication mechanism** already exist (reused from the host application).
- One task has exactly one owner; ownership does not transfer.

## 9. Open questions

- Should completed (**Done**) tasks be reopenable directly to **In Progress**? *Current decision: no — only In Progress → Todo reopens; Done is terminal.* (Revisit if users request it.)
