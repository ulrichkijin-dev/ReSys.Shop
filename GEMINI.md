# ReSys.Shop

This is a comprehensive e-commerce platform built with a modern, distributed architecture. It includes a customer-facing storefront, an administration panel, and a sophisticated backend with features like image-based product search.

## Project Overview

The solution is a .NET-based system with a Vue.js frontend for the admin panel and a Python-based service for image search. It is designed with a clean architecture, separating concerns into distinct projects.

### Key Technologies

*   **Backend:** .NET 9, ASP.NET Core, Carter, MediatR, Entity Framework Core, PostgreSQL, Redis, Quartz.NET
*   **Frontend (Admin):** Vue.js 3, Vite, Pinia, Vue Router, PrimeVue
*   **Image Search:** Python, FastAPI, PyTorch, Transformers, `pgvector`
*   **Authentication:** JWT, with support for Google, Facebook, and OpenID Connect
*   **Deployment:** Docker

### Architecture

The solution follows a clean architecture pattern:

*   **`ReSys.Shop.Core`:** Contains the core business logic, domain models, and interfaces.
*   **`ReSys.Shop.Infrastructure`:** Implements the interfaces defined in the Core project, handling concerns like data access, external services, and background jobs.
*   **`ReSys.Shop.Api`:** The main ASP.NET Core application that exposes the backend functionality through a RESTful API.
*   **`ReSys.Shop.Admin`:** A Vue.js single-page application for managing the e-commerce platform.
*   **`ReSys.Shop.ImageSearch`:** A Python-based microservice that provides image search functionality using a pre-trained vision transformer model.
*   **`ReSys.Shop.Tests`:** Contains unit and integration tests for the solution.

## Building and Running

### Backend (.NET)

To run the backend, you will need the .NET 9 SDK installed.

```bash
# Restore dependencies
dotnet restore

# Run the API
dotnet run --project src/ReSys.Shop.Api/ReSys.Shop.Api.csproj
```

The API will be available at `http://localhost:5000` (or as configured in `launchSettings.json`).

### Frontend (Admin)

To run the admin frontend, you will need Node.js and npm installed.

```bash
# Navigate to the admin project directory
cd src/ReSys.Shop.Admin

# Install dependencies
npm install

# Run the development server
npm run dev
```

The admin panel will be available at `http://localhost:5173` (or the next available port).

### Image Search (Python)

To run the image search service, you will need Python and Docker installed. The service is designed to be run in a Docker container.

```bash
# Navigate to the image search project directory
cd src/ReSys.Shop.ImageSearch

# Build and run the Docker container
docker-compose up --build
```

The image search service will be available at `http://localhost:8000`.

## Development Conventions

*   **Central Package Management:** .NET dependencies are managed centrally in the `Directory.Packages.props` file.
*   **Coding Style:** The project uses `.editorconfig` to enforce consistent coding styles. For the frontend, Prettier and ESLint are used.
*   **Testing:** The project uses xUnit for testing, with FluentAssertions for assertions and NSubstitute for mocking.
*   **API Documentation:** The API is documented using Swagger/OpenAPI, and can be accessed at the `/swagger` endpoint.
*   **Database Migrations:** Entity Framework Core is used for database migrations. To create a new migration, run the following command from the `src/ReSys.Shop.Infrastructure` directory:
    ```bash
    dotnet ef migrations add <MigrationName> --startup-project ../ReSys.Shop.Api/ReSys.Shop.Api.csproj
    ```
    To apply migrations, run:
    ```bash
    dotnet ef database update --startup-project ../ReSys.Shop.Api/ReSys.Shop.Api.csproj
    ```