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
RaceDay.API/
├── Controllers/
│ ├── AuthController.cs
│ ├── UsersController.cs
│ ├── EventsController.cs
│ ├── CategoriesController.cs
│ ├── EnrolmentsController.cs
│ └── ResultsController.cs
├── Data/
│ └── ApplicationDbContext.cs
├── DTOs/
├── Migrations/
├── Models/
│ ├── User.cs
│ ├── OrganiserProfile.cs
│ ├── ParticipantProfile.cs
│ ├── Event.cs
│ ├── Category.cs
│ ├── Enrolment.cs
│ ├── Result.cs
│ └── JwtSettings.cs
├── Services/
│ ├── IJwtService.cs / JwtService.cs
│ ├── IAuthService.cs / AuthService.cs
│ ├── IUserService.cs / UserService.cs
│ ├── IEventService.cs / EventService.cs
│ ├── ICategoryService.cs / CategoryService.cs
│ ├── IEnrolmentService.cs / EnrolmentService.cs
│ └── IResultService.cs / ResultService.cs
├── appsettings.json
└── Program.cs

RaceDay.Tests/
├── Helpers/
│ └── TestDbContextFactory.cs
├── AuthServiceTests.cs
└── RoleAccessTests.cs

text

---

## API Endpoints

### Authentication
| Method | Route | Description | Access |
|--------|-------|-------------|--------|
| POST | /api/Auth/register | Register a new user | Public |
| POST | /api/Auth/login | Login and receive JWT | Public |

### Users
| Method | Route | Description | Access |
|--------|-------|-------------|--------|
| GET | /api/Users/profile | Get own full profile | Authenticated |
| PUT | /api/Users/profile | Update own profile | Authenticated |
| GET | /api/Users/me | Get own basic details | Authenticated |

### Events
| Method | Route | Description | Access |
|--------|-------|-------------|--------|
| GET | /api/Events | List all events | Public |
| GET | /api/Events/{eventId} | Get event by ID | Public |
| POST | /api/Events | Create event | Organiser |
| PUT | /api/Events/{eventId} | Update event | Organiser |
| DELETE | /api/Events/{eventId} | Delete event | Organiser |
| GET | /api/Events/my-events | Get organiser's own events | Organiser |

### Categories
| Method | Route | Description | Access |
|--------|-------|-------------|--------|
| GET | /api/Categories/event/{eventId} | List categories for event | Public |
| GET | /api/Categories/{categoryId} | Get category by ID | Public |
| POST | /api/Categories | Create category | Organiser |
| PUT | /api/Categories/{categoryId} | Update category | Organiser |
| DELETE | /api/Categories/{categoryId} | Delete category | Organiser |

### Enrolments
| Method | Route | Description | Access |
|--------|-------|-------------|--------|
| GET | /api/Enrolments | Get own enrolments | Participant |
| GET | /api/Enrolments/event/{eventId} | Get enrolments for event | Organiser |
| GET | /api/Enrolments/{enrolmentId} | Get enrolment by ID | Authenticated |
| POST | /api/Enrolments | Enrol in event | Participant |
| PUT | /api/Enrolments/{enrolmentId} | Update enrolment status | Authenticated |
| DELETE | /api/Enrolments/{enrolmentId} | Cancel enrolment | Authenticated |

### Results
| Method | Route | Description | Access |
|--------|-------|-------------|--------|
| GET | /api/Results/event/{eventId} | Get event results | Public |
| GET | /api/Results/my-results | Get own results | Participant |
| GET | /api/Results/{resultId} | Get result by ID | Public |
| POST | /api/Results | Upload result | Organiser |
| PUT | /api/Results/{resultId} | Update result | Organiser |
| DELETE | /api/Results/{resultId} | Delete result | Organiser |

---

## Setup Instructions

### Prerequisites
- Visual Studio 2022
- .NET 8.0 SDK
- SQL Server



## Authentication

The API uses JWT Bearer tokens.

1. Call POST /api/Auth/register or POST /api/Auth/login.
2. Copy the token from the response.
3. In Swagger, click Authorize.
4. Enter: Bearer YOUR_TOKEN_HERE
5. Click Authorize.

Passwords are hashed with BCrypt and never stored in plain text.

---

## Role-Based Access Control

| Role | Can Do |
|------|--------|
| Anonymous | Browse events, view categories, view results |
| Participant | Enrol in events, view own enrolments and results, update profile |
| Organiser | Manage own events and categories, view their event enrolments, upload results |

Wrong-role access returns 403 Forbidden.

---

## Unit Tests

The test suite contains 9 tests:

AuthServiceTests (5 tests):
- Register as Organiser creates user + profile
- Login with valid credentials returns token
- Login with wrong password throws UnauthorizedAccessException
- Duplicate email registration throws exception
- Password is hashed (not stored as plain text)

RoleAccessTests (4 tests):
- Participant cannot create event
- Organiser can create event
- Organiser cannot enrol in event
- Participant can enrol in event (with bib number)






