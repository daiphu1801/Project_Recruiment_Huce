# Project_Recruiment_Huce

Job portal built with ASP.NET MVC 5, Entity Framework 6, and a Colorlib template. It supports custom authentication with OWIN cookies and a simple role model (Admin, Company, Recruiter, Candidate).

## Features
- Public site using shared layout (Home, About, Contact, Jobs Listing, Job Details).
- Authentication (login by username or email) with server-side validation.
- Role-based UI (Post a Job button only for Company/Recruiter; Candidate sees profile manage link).
- Manage pages:
  - Candidate profile edit with rich-text About (Quill).
  - Stub views for Company and Recruiter profile edit.
- Admin area scaffolded layout under `Areas/Admin`.

## Tech stack
- ASP.NET MVC 5.2.9, OWIN Cookie Auth
- Entity Framework 6.4.4 (Code-First-to-Existing DB style POCOs)
- SQL Server (LocalDB or full SQL Server)
- Bootstrap 4 theme from Colorlib, jQuery, Owl Carousel, etc.

## Project structure
- `Project_Recruiment_Huce/`
  - `Controllers/` Home, Account, Jobs, Candidates
  - `Views/` Razor views; shared layout `_Layout.cshtml`
  - `Views/Candidates/CandidatesManage.cshtml` profile form (Quill editor)
  - `Views/Companies/CompaniesManage.cshtml` company profile form (stub)
  - `Views/Recruiters/RecruitersManage.cshtml` recruiter profile form (stub)
  - `DbContext/RecruitmentDbContext.cs` EF context
  - `Models/` POCOs (Account, Candidate, Company, Recruiter, JobPost, ...)
  - `Repositories/` simple repositories for Account, JobPost
  - `Areas/Admin/` admin UI layout
  - `Database/WEBVER2.SQL` database schema and constraints

## Prerequisites
- Windows + Visual Studio 2022 (Community is fine)
- .NET Framework 4.7.2 targeting pack
- SQL Server LocalDB (installed with VS) or SQL Server Express/Standard

## Database setup
There are two simple options. The default Web.config is configured for LocalDB.

1) Use SQL Server LocalDB (recommended for local dev)
- Ensure LocalDB is installed: it ships with Visual Studio.
- Open SQL Server Object Explorer in VS (or use `sqlcmd`) and run the script:
  - File: `Project_Recruiment_Huce/Database/WEBVER2.SQL`
  - This creates database `JOBPORTAL_EN` and all tables.
- Connection string (already set):
  ```
  Server=(localdb)\MSSQLLocalDB;Database=JOBPORTAL_EN;Integrated Security=True;MultipleActiveResultSets=True;
  ```

2) Use full SQL Server / SQL Express
- Create the DB by executing `WEBVER2.SQL` on your instance.
- Update `Web.config` connectionStrings → `RecruitmentDb`, e.g.:
  - Windows auth:
    ```
    Server=.\SQLEXPRESS;Database=JOBPORTAL_EN;Integrated Security=True;MultipleActiveResultSets=True;
    ```
  - SQL login:
    ```
    Server=.\SQLEXPRESS;Database=JOBPORTAL_EN;User Id=sa;Password=your_password;MultipleActiveResultSets=True;Trusted_Connection=False;
    ```

## Running locally
1. Open `Project_Recruiment_Huce.sln` in Visual Studio.
2. Build solution (NuGet packages will restore automatically).
3. Ensure the DB `JOBPORTAL_EN` exists (see Database setup above).
4. Run with IIS Express (green play button) or press F5.

Default routes:
- `/` → `Home/Index`
- `/Account/Login` → login (username or email)
- `/Jobs/JobsListing`, `/Jobs/JobsDetails/{id}`
- Manage menu (top-right) routes by role:
  - Candidate → `/Candidates/CandidatesManage`
  - Company → `/Companies/CompaniesManage`
  - Recruiter → `/Recruiters/RecruitersManage`
  - Admin → separate Admin UI (`/Admin`)

## Notes
- Rich text `About` for Candidate allows HTML and is length-limited in model; server trims to 500 chars for safety.
- “Post a Job” appears only for Company/Recruiter.
- To change the DB server, just edit the `RecruitmentDb` connection string in `Web.config`.