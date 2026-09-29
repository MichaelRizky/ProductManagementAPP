# Product Management API

A robust RESTful API built with **ASP.NET Core (.NET 10)** for managing products, featuring secure JWT authentication, efficient in-memory caching, and structured logging.

## 🚀 Features

- **Product Management:** Complete CRUD operations (Create, Read, Update, Delete) for products.
- **Search Capabilities:** Search products by name and filter by price range.
- **Secure Authentication:** User registration and login using JWT (JSON Web Tokens) and BCrypt for secure password hashing.
- **Caching:** In-Memory caching implemented for faster data retrieval (absolute and sliding expirations).
- **Logging:** Structured logging configured with NLog to trace application events, warnings, and errors.
- **Database:** SQLite database integrated using Entity Framework Core (EF Core).
- **Architecture:** Clean separation of concerns utilizing the Repository Pattern.
- **API Documentation:** OpenAPI/Swagger support built-in for easy API testing and exploration.

## 🛠️ Technology Stack

- **Framework:** .NET 10 / ASP.NET Core
- **Database:** SQLite
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer
- **Security:** BCrypt.Net-Next
- **Logging:** NLog
- **Environment Management:** DotNetEnv

## 📂 Project Structure

```
ProductManagementAPP/
├── Controllers/       # API Controllers (AuthController, ProductController)
├── Data/              # EF Core DbContext
├── Migrations/        # EF Core Migrations
├── Model/             # Entity models (Product, User, UserAuthDto)
├── Repositories/      # Data access logic (Repository Pattern)
├── Views/             # MVC Views (if UI is needed)
├── appsettings.json   # Application configuration
├── nlog.config        # NLog configuration rules
└── Program.cs         # Application entry point and service configuration
```

## 🔐 API Endpoints

### Authentication (`/api/Auth`)

- `POST /api/Auth/register` - Register a new user
- `POST /api/Auth/login` - Authenticate a user and receive a JWT token

### Products (`/api/Product`) - _Requires JWT Token_

- `GET /api/Product` - Retrieve all products (Cached)
- `GET /api/Product/{id}` - Retrieve a specific product by ID
- `GET /api/Product/search` - Search products by name, minPrice, and maxPrice
- `POST /api/Product` - Create a new product
- `PUT /api/Product/{id}` - Update an existing product
- `DELETE /api/Product/{id}` - Delete a product

## ⚙️ Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- SQLite

### Installation & Setup

1. **Clone the repository** (if applicable).
2. **Navigate to the project directory:**
   ```bash
   cd ProductManagementAPP
   ```
3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```
4. **Apply Entity Framework Migrations (Database Setup):**
   ```bash
   dotnet ef database update
   ```
   > **Note:** If you encounter an error stating that the `dotnet ef` command is not found (especially on a new PC), you need to install the Entity Framework Core CLI tools first by running:
   > ```bash
   > dotnet tool install --global dotnet-ef
   > ```

5. **Run the application:**
   ```bash
   dotnet run
   ```
6. **Access Swagger UI:**
   Open your browser and navigate to `https://localhost:<port>/swagger` (or the respective URL provided in the console) to interact with the API.

## 🔑 Authentication Usage

To access the `Product` endpoints, you need to provide a JWT token:

1. Register a new account via `/api/Auth/register`.
2. Login via `/api/Auth/login` to receive your `Token`.
3. In Swagger (or Postman), use the **Authorize** button/header and provide the token in the format: `Bearer <your_token>`.

## 📝 Logging Configuration

Logs are handled by **NLog** and are configured via `nlog.config`. By default, logs will be generated in the `logs/` directory inside the project root, keeping track of info, warnings, and errors automatically.
