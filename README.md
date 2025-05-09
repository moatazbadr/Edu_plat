<p align="center">
  <img src="EduPlat.jpeg" alt="EduPlat Logo" width="200"/>
</p>

<h1 align="center">EduPlat</h1>

## 📚 Table of Contents

- [Introduction](#eduplat-web-api)
- [Key Features](#key-features)
- [📄 Full Documentation](#-full-documentation)
- [🚦 Live Demo](#-live-demo)
- [Getting Started / Installation](#getting-started--installation)
- [Pre-built Executable](#-pre-built-executable)
- [Troubleshooting](#-troubleshooting)


# EduPlat Web API

EduPlat is a powerful educational platform built with ASP.NET Core, designed to simplify managing courses, exams, and communication for students and faculty. This Web API is the core of EduPlat, delivering secure, scalable RESTful endpoints to support all platform features. It’s built with clean architecture, robust security, and performance in mind, making it a solid choice for any learning management system (LMS).

## Key Features

### Easy Deployment
- **EduPlat.exe**: A single, no-dependency Windows executable for quick setup and deployment.

### Secure Authentication
- Uses **JWT** for token-based authentication.
- Includes **role-based access control** and token blacklisting for secure access.

### Course & Resource Management
- Upload and download course materials (PDF, Word, PPT) with a **300 MB** file size limit.
- Organize resources efficiently within course modules.

### Video Support
- Attach educational videos to courses for richer learning experiences.

### Exam System
- Full **CRUD** support for creating and managing exams and questions (2–4 choices).
- Features scoring and customizable time limits.

### Notifications
- Sends **SMTP email** and **Firebase push notifications** for events like:
  - New user registration
  - Course material uploads
  - Exam schedules

### Database Integration
- Built with **Entity Framework Core**, supporting **auto migrations**.
- Compatible with **local SQL Server**.

### Developer-Friendly Docs
- Interactive **Swagger UI** for testing endpoints with Bearer token authentication.

## 📄 Full Documentation

For a complete guide to the platform’s features, usage, and API details, check out the official documentation:

🔗 [**EduPlat Documentation**](https://moatazs-organization-1.gitbook.io/edu-plat)

> Includes setup instructions, API reference, architecture overview, and more.

## 🚦 Live Demo

- **Hosted Demo**: [EduPlat Live Demo](https://eduplat123.runasp.net/swagger/index.html)

- ## Getting Started / Installation

### Prerequisites:
Before you begin, ensure that you have the following installed:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio](https://visualstudio.microsoft.com/) (with .NET Core workload)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (or another database if required for the project)

### Installation Steps:

1. **Clone the Repository:**
   Open your terminal or command prompt and run the following command to clone the repository:
   ```bash
   git clone https://github.com/SALEH-SHERIF/Edu_plat.git

2. **Open the Project in Visual Studio:**
      <span style="font-size: 20px;">●</span> After cloning the repository, open the project in Visual Studio by navigating to the project folder and opening the .sln file
 
3. **Configure the Database:**

      <span style="font-size: 20px;">●</span> If the project uses SQL Server or another database, make sure to set up your database accordingly.

      <span style="font-size: 20px;">●</span> Update the connection string in the **appsettings.json** file to match your database configuration

4. **Install Dependencies:**
      <span style="font-size: 20px;">●</span> In Visual Studio, the NuGet packages will be restored automatically when you open the project. However, if not, you can manually restore the packages by running:
   ```bash
   dotnet restore
5. **Run the Project:**

     <span style="font-size: 20px;">●</span> In Visual Studio, press F5 or click "Start Debugging" to run the project.
   
     <span style="font-size: 20px;">●</span> The Web API should now be running locally, typically accessible at
     ``` https://localhost:5001 or https://localhost:5000 ```
 
 6. **Test the API::**
  
     <span style="font-size: 20px;">●</span> You can use tools like Postman to send HTTP requests to your API endpoints.

     <span style="font-size: 20px;">●</span> The API will be available at https://localhost:5001/api (or the configured port).

**Feel free to reach out if you need further clarification on any step!**

This setup assumes that the project uses a SQL Server or similar database and includes the default setup for .NET 8. Let me know if there’s any specific configuration or part you want to adjust!

##  Pre-built Executable

1. **Download & Extract**
   - Go to [Releases](https://github.com/SALEH-SHERIF/Edu_plat/releases).
   - Download the `.zip`/`.exe` (Windows) or `.tar.gz` (macOS) file.
   - Extract:
     - *Windows*: right-click → Extract All...
     - *macOS*:  
       ```bash
       tar -xzf Edu_plat_mac.tar.gz
       ```

2. **Run the Application**
   - *Windows*: Open **Command Prompt** or **PowerShell**, navigate to the folder containing `Edu_plat.exe`, and run:
     ```bash
     .\Edu_plat.exe
     ```
   - *macOS/Linux*: Open **Terminal**, navigate to the folder containing `Edu_plat`, and run:
     ```bash
     chmod +x Edu_plat && ./Edu_plat
     ```

3. **Access the API**
   - After the API starts, it will listen on the default port. You should see a message like:
     ```
     Now listening on: http://0.0.0.0:7189
     ```

4. **Access Swagger UI**
   - To interact with the API using Swagger UI, navigate to:
     [http://localhost:7189/swagger/index.html](http://localhost:7189/swagger/index.html)


> **Notes:**
> - If you have trouble accessing the API, make sure port `7198` is not blocked by another program or firewall.
> - To access the API from another device on the same network, replace `localhost` with the IP address of your machine.  
>   For example: `https://192.168.1.10:7198/api/...`


##  Troubleshooting

Here are some common issues and how to resolve them:

### ❌ Database Connection Issues
- **Problem**: Unable to connect to the database.
- **Solution**:  
  - Ensure the correct server name and authentication method are used in `appsettings.json`.
  - Make sure the SQL Server service is running.
  - Check firewall settings on your machine.

### ❌ Entity Framework Migrations Not Working
- **Problem**: Errors when applying migrations.
- **Solution**:  
  - Delete the `Migrations/` folder.
  - Re-create the initial migration and apply it:
    ```bash
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

### ❌ Port Conflict
- **Problem**: The application port is already in use.
- **Solution**:  
  - Open `Properties/launchSettings.json` and change the `applicationUrl` to an available port (e.g., `https://localhost:7199`).

### ❌ SSL Warnings in Browser
- **Problem**: Browser shows warning for untrusted certificate.
- **Solution**:  
  - Trust the development SSL certificate using:
    ```bash
    dotnet dev-certs https --trust
    ```

### ❌ Upload Size Limit Exceeded
- **Problem**: File uploads fail with large files.
- **Solution**:  
  - Increase `MaxRequestBodySize` in `Program.cs`, or configure limits in your reverse proxy (e.g., Nginx/IIS).

### ❌ Missing Configuration Keys
- **Problem**: JWT/SMTP/Firebase settings are missing or incorrect.
- **Solution**:  
  - Double-check your `appsettings.json` and `secrets.json` for required keys like `Jwt:Key`, `Smtp:Host`, etc.

---

> 💬 For questions, issues, or feature requests, please open a ticket here: [GitHub Issues](https://github.com/SALEH-SHERIF/Edu_plat/issues)

*Enjoy building with EduPlat! 🚀*
