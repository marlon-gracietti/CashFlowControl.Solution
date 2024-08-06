# CashFlowControl.Solution

## Introduction
CashFlowControl.Solution is a comprehensive financial management application designed to streamline cash flow tracking, transaction management, and daily financial summaries. This solution employs clean architecture principles, ensuring maintainability, scalability, and testability.

## Project Structure
The project is structured following the Clean Architecture approach, which separates concerns and facilitates the addition of new features and services.

### Key Components:
- **Core**: Contains the business logic and domain entities.
- **Application**: Includes the services and application logic.
- **Infrastructure**: Handles data access, external services, and other infrastructure concerns.
- **Presentation**: Manages the user interface and API endpoints.

## Technologies Used
- **ASP.NET Core**: For building web APIs.
- **Entity Framework Core**: For database management and ORM.
- **xUnit**: For unit testing.
- **Moq**: For mocking dependencies in tests.
- **Docker**: For containerization.
- **Swagger**: For API documentation.

## Installation and Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop)

### Steps to Run the Application
1. **Clone the Repository**
   ```bash
   git clone https://github.com/mgracietti/CashFlowControl.Solution.git   
   cd CashFlowControl.Solution/scripts   
   ./run.sh
   ```

2. **Nativatge to API Documentation**
Swagger: Navigate to /swagger in your browser to access the API documentation and explore the available endpoints.
TransactionsService: http://localhost:8080/swagger/index.html
DailySummaryService: http://localhost:8081/swagger/index.html

### Usage Examples
1. **Adding a Transaction**
```http
POST /api/transactions
Content-Type: application/json

{
  "amount": 100,
  "date": "2024-08-05T00:00:00Z",
  "isCredit": true,
  "description": "Payment received"
}
```


2. **Getting Daily Summary**
```http
GET /api/dailysummary?date=2024-08-05
```
