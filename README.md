#  RaceDay - Event Management System

## Overview

RaceDay is a full-stack web-based event management platform designed specifically for the South African road running, walking, and cycling community. The system bridges the gap between event organisers and participants, replacing outdated paper-based registration and spreadsheet management with a modern, cloud-aware solution.

The platform enables Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter competitions, track their personal performance history, and prepare for race day using live weather and route information.

## Core Features

###  For Organisers
- Create, edit, and delete events
- Manage event categories and entry fees
- Capture participant results (times and positions)
- View all event enrolments
- Monitor event capacity

###  For Participants
- Create accounts and manage profiles
- Browse upcoming events
- Enter events by selecting categories
- View personal enrolment history
- Track race results and performance
- Access event route and weather information

##  Technologies Used

| Technology | Purpose |
|-----------|---------|
| ASP.NET Core Web API 8.0 | RESTful API framework |
| Entity Framework Core 8.0 | ORM with Code-First approach |
| SQL Server (LocalDB) | Relational database |
| BCrypt / SHA256 | Password hashing |
| Server-side Sessions | Authentication state |
| xUnit + EF Core InMemory | Unit testing (16 tests) |
| Swagger / OpenAPI | API documentation & testing |
| GitHub Actions | CI/CD pipeline |

---

##  Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or later
- [GitHub Desktop](https://desktop.github.com/) (optional)


#### SQL Scripts Results
<img width="1917" height="970" alt="image" src="https://github.com/user-attachments/assets/aa9b5b2d-1148-4109-ba4b-8b6c53bdd2dd" />

### GitHub Workflows
<img width="1141" height="407" alt="image" src="https://github.com/user-attachments/assets/a38b3d57-ae13-46fe-97e5-4650a99b766b" />

### Docker build
<img width="1908" height="813" alt="image" src="https://github.com/user-attachments/assets/5c967669-1516-4675-889b-c23abd309e06" />


## Part 3 — MVC Web Application, Blob Storage & Docker

### Architecture
The application is a 3-tier architecture:
1. **RaceDay.Mvc**: Front-end Razor app (sessions, role-based UI).
2. **RaceDay.Api**: RESTful API (business logic).
3. **SQL Server / Local Storage**: Data persistence and image storage.

### How to Run Locally
1. Start the API: `dotnet run --project RaceDay/RaceDay.Api`
2. Start the MVC: `dotnet run --project RaceDay/RaceDay.Mvc`
3. Open `http://localhost:5096` in your browser.

### Docker Setup
To run the MVC application inside Docker:

1. Ensure **Docker Desktop** is running.
2. **Important:** The API must be running on your host machine.
3. Build the image (run from the solution root):
   ```bash
   docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc:part3 .


### Unlisted Video Link
https://www.youtube.com/watch?v=oX9mg6LIq3c

### AI Assistance Disclosure
In accordance with the assessment rules on AI use, I disclose that I used Claude (Anthropic) as a supplementary tool during development for:

Code review and debugging guidance

Explaining Entity Framework Core concepts and best practices

Suggesting error resolution strategies for compile and runtime issues


