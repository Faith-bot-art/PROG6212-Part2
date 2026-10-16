# PROG6212 Part 2 - RaceDay API

## Description

RaceDay is a RESTful web API for managing road running, walking, and cycling events in South Africa. It is designed for the South African road events community and allows Event Organisers to create and manage events, categories, and results, while Participants can browse events, enrol, and track their performance history.

This API is built with ASP.NET Core Web API (.NET 8), uses Entity Framework Core with the Code-First approach, and connects to SQL Server. It implements JWT authentication with role-based access control, and includes 9 unit tests covering authentication and authorization.

---

## User Roles

### 1. Event Organiser
- Create, update, and delete events
- Add, update, and delete categories for events
- View enrolments for their own events
- Upload, update, and delete race results

### 2. Participant
- Browse and search upcoming events (public)
- Enrol in events by selecting a category
- View personal enrolment history
- View personal race results
- Update their own profile

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core Web API (.NET 8) |
| Database | SQL Server |
| ORM | Entity Framework Core (Code-First) |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net-Next |
| API Documentation | Swagger / OpenAPI |
| Testing | xUnit, EF Core InMemory |
| CI/CD | GitHub Actions |

---

## Project Structure
