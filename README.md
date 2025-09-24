# Team Local Docker & Azure SQL Setup

## Quickstart for Team Members

1. **Clone the repository:**
	```
	git clone <repo-url>
	cd <repo-folder>
	```

2. **Ensure your `.env` file exists in the project root and contains the Azure SQL connection string:**
	```
	AZURE_SQL_CONNECTION_STRING=Server=tcp:eskolar.database.windows.net,1433;Initial Catalog=eskolardb;Persist Security Info=False;User ID=eskolaradmin;Password=...;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
	```
	> **Never commit real passwords to public repos!**

3. **Run the app using Docker Compose:**
	```
	docker-compose up --build
	```

4. **Access the app:**
	- Open your browser to [http://localhost](http://localhost)

5. **All team members share the same Azure SQL database.**
	- Any changes to data are visible to everyone.

6. **If you update the `.env` or code, push/pull changes as needed.**

---
**Troubleshooting:**
- If you see connection errors, check your `.env` file and Azure SQL firewall settings.
- Make sure you run `docker-compose up` from the same directory as your `.env` file.
- If schema errors occur, run:
  ```
  dotnet ef database update
  ```
# eSkolar - Scholarship Application and Management System

A comprehensive scholarship management platform built with **Blazor Server** and **ASP.NET Core**. This system aims to connect students, benefactors, and educational institutions to streamline the scholarship application and management process.

## 🚀 Features

### For Students
- **Profile Management** - Complete student profiles with academic information
- **Scholarship Discovery** - Browse and search available scholarships
- **Application Tracking** - Submit and track scholarship applications
- **Dashboard** - Personalized dashboard with application status

### For Benefactors
- **Scholarship Creation** - Create and manage scholarship programs
- **Application Review** - Review and evaluate student applications
- **Organization Profile** - Manage benefactor organization information
- **Analytics** - Track scholarship program performance

### For Institutions
- **Student Management** - Oversee student profiles and applications
- **Partnership Management** - Collaborate with benefactors
- **Institutional Scholarships** - Manage internal scholarship programs
- **Reporting** - Generate comprehensive reports

## 🛠️ Technology Stack

- **Frontend**: Blazor Server, Bootstrap, BlazorBootstrap
- **Backend**: ASP.NET Core 8.0, Entity Framework Core
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity with Role-based Authorization
- **Architecture**: Clean Architecture principles

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full version)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/[your-username]/c2_eskolar.git
cd c2_eskolar
 

## 🐳 Docker Workflow

### 1. Build and Run with Docker Compose
```powershell
docker compose build
docker compose up
```

This will start both the SQL Server and the Blazor Server app containers. The app will be available at [http://localhost:80](http://localhost:80).

### 2. Environment Variables
- Connection strings and environment variables are set in `docker-compose.yml`.
- For HTTPS in development, see the commented instructions in `docker-compose.yml` and [Microsoft's official guide](https://learn.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-9.0).

### 3. Common Issues
- **Decimal precision warnings:** These do not prevent the app from running, but you may want to configure precision in your EF Core models.
- **Data Protection keys warning:** Keys are stored in the container and reset on restart. For production, use persistent storage.
- **Identity DI errors:** Ensure all Identity services use `ApplicationUser` consistently.

### 4. Stopping Containers
```powershell
docker compose down
```