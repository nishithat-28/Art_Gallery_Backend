# 🖼️ Online Art Gallery - ASP.NET Core Web API Backend

## This document provides a summary of the available API endpoints and key files for the **Online Art Gallery** backend project.

### 📚 [API Endpoints Overview - Online Art Gallery Backend](#authentication)

### 🚀 [How to Run This Project](#how-to-run-this-project)

---

### Authentication

| Method | Endpoint             | Description               |
| :----- | :------------------- | :------------------------ |
| POST   | `/api/Auth/register` | Register a new user       |
| POST   | `/api/Auth/login`    | Login and get a JWT token |
| GET    | `/api/Auth/{id}`     | Get user details          |

---

### 🎨 ArtWork

| Method | Endpoint                             | Description                     |
| :----- | :----------------------------------- | :------------------------------ |
| GET    | `/api/ArtWork`                       | Get all artworks                |
| GET    | `/api/ArtWork/{id}`                  | Get artwork by ID               |
| GET    | `/api/ArtWork/category/{categoryId}` | Get artworks by category        |
| POST   | `/api/ArtWork/batch`                 | Get artworks by IDs (in body)   |
| POST   | `/api/ArtWork`                       | Create new artwork (Admin only) |
| PUT    | `/api/ArtWork/{id}`                  | Update artwork (Admin only)     |
| DELETE | `/api/ArtWork/{id}`                  | Delete artwork (Admin only)     |

---

### 🗂️ Category

| Method | Endpoint             | Description                      |
| :----- | :------------------- | :------------------------------- |
| GET    | `/api/Category`      | Get all categories               |
| GET    | `/api/Category/{id}` | Get category by ID               |
| POST   | `/api/Category`      | Create new category (Admin only) |
| PUT    | `/api/Category/{id}` | Update category (Admin only)     |
| DELETE | `/api/Category/{id}` | Delete category (Admin only)     |

---

### 🛒 Order

| Method | Endpoint                  | Description                                               |
| :----- | :------------------------ | :-------------------------------------------------------- |
| GET    | `/api/Order`              | Get all orders (Admin sees all, users see only their own) |
| GET    | `/api/Order/{id}`         | Get order by ID                                           |
| POST   | `/api/Order`              | Create new order                                          |
| GET    | `/api/Order/invoice/{id}` | Get invoice for an order                                  |

---

## 🗃️ Key Files and Their Functions

### 📦 Models/

- **User.cs** - Represents user accounts
- **Category.cs** - Represents art categories
- **ArtWork.cs** - Represents art pieces
- **Order.cs** - Represents customer orders
- **OrderItem.cs** - Represents items in an order

### 📂 Data/

- **ArtGalleryContext.cs** - Entity Framework `DbContext` for database operations
- **DataSeeder.cs** - Seeds initial data into the database

### 📨 DTOs/ (Data Transfer Objects)

- **UserDtos.cs** - DTOs for user-related operations
- **ArtWorkDtos.cs** - DTOs for artwork-related operations
- **OrderDtos.cs** - DTOs for order-related operations

### 🧩 Controllers/

- **AuthController.cs** - Handles user authentication and registration
- **ArtWorkController.cs** - Manages artworks
- **CategoryController.cs** - Manages categories
- **OrderController.cs** - Manages orders and invoices

### 🛠️ Helpers/

- **AuthHelper.cs** - Helper methods for authentication and JWT generation

### 🛠️ Program.cs

- Sets up dependency injection
- Configures JWT authentication
- Sets up database connection
- Configures CORS for Angular frontend

---

## ✅ Features Implemented

- Token-based authentication using JWT
- Role-based security (Admin vs Customer roles)
- RESTful API endpoints for artwork, user, and order management
- SQL Server database integration using Entity Framework Core
- Proper relationships between entities
- Invoice generation functionality

## How to Run This Project

### 1. Clone the Repository

`git clone OnlineArtGallery.API`

`cd OnlineArtGallery.API`

### 2. Configure the Database

Make sure SQL Server is installed and running.

Edit the connection string in `appsettings.json`:

```bash
{
    "ConnectionStrings": {
        "GalleryConnection": "Server=localhost;Database=OnlineArtGalleryDb;Trusted_Connection=True;TrustServerCertificate=True;"
    }
}
```

`NOTE : - Change localhost to your server name (example:- SystemName\\SQLEXPRESS)`

### 3. Install Dependencies

`dotnet restore`

### 4. Apply Migrations and Seed Data

If no migration exists:

`dotnet ef migrations add InitialCreate`

Then update the database:

`dotnet ef database update`

✅ The database and schema will be created.

### 5. Run the API

`dotnet run`

Then open your browser and go to:

`localhost:5052/swagger/index.html`

Swagger will list all available endpoints.

### ⚙️ Requirements

.NET SDK

SQL Server

Any IDE like VS Code
