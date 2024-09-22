# Core Backend

This is a .NET Core 8 Web API project that connects to a local SQL Server database.

## Prerequisites

- [.NET Core 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Management Studio (SSMS v18.0)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Developer or Express edition for local development)

## Setup

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/indpro-fullstack-be.git
   cd indpro-fullstack-be
   ```

2. Update the connection string:
   - Open `appsettings.json`
   - Update the `DefaultConnection` string with your local SQL Server details:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=.;Initial Catalog=YOUR_DB_NAME;Integrated Security=True;TrustServerCertificate=True;"
     }
     ```

3. Apply database migrations:
   ```
   Add-Migration InitialCreate
   ```
4. Create db and schema:
   ```
   Update-Database
   ```
## Running the Application

1. Navigate to the project directory:
   ```
   cd indpro-fullstack-be
   ```

2. Run the application:
   ```
    ctrl+F5
   ```

3. The API will be available at `https://localhost:5001` and `http://localhost:5000`

## API Documentation

Once the application is running, you can view the Swagger UI documentation at:

`https://localhost:5001/swagger`

## Contributing

No contributions allowed

## License

This project is private