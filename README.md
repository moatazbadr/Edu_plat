# EduPlat Web API Module

> [![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download) [![GitHub release](https://img.shields.io/github/v/release/SALEH-SHERIF/Edu_plat)](https://github.com/SALEH-SHERIF/Edu_plat/releases)

## 📋 Release Notes

*Use this section as your GitHub Release **Description** when publishing the `.exe` package.*

**Version 2.3.4** — *EduPlat Web API* (`Edu_plat.exe`)

**What's Inside:**

- ⚙️ **Standalone Executable**: `Edu_plat.exe`—zero-dependency Windows binary.
- 🔒 **JWT Security**: Access tokens, blacklisting, and role-based access control.
- 🗂️ **Course & Materials**: Upload/download study resources (PDF, Word, PPT) up to 300 MB.
- 🎥 **Video Uploading**: Support for course-related videos.
- 📝 **Exam Engine**: CRUD for Exams, Questions (2–4 choices), scoring, and time limits.
- 📧 **Notifications**: SMTP emails + Firebase push for registration, material uploads, and exams.
- 🔄 **EF Core**: Auto migrations, seed data, SQL Server/Azure SQL support.
- 📚 **Swagger UI**: Explore and test all endpoints with Bearer token support and so on .

**Before You Download:**

1. Install **.NET 8.0 Runtime**.
2. Prepare your `appsettings.json` (see `appsettings.json.sample`).
3. Ensure SQL Server/Azure SQL is accessible.

**Package Includes:**

- `Edu_plat.exe`
- `appsettings.json.sample`
- `README.md` (this guide)

---

## 🚦 Live Demo

- **Ngrok Link**  
  [https://great-hot-impala.ngrok-free.app](https://great-hot-impala.ngrok-free.app)  
  Temporary link hosted from my local machine using Ngrok. It will only work while my local server is running.

- **MonsterHosting Link (via RunASP.net)**  
  [https://eduplat123.runasp.net](https://eduplat123.runasp.net)  
  Publicly hosted version on MonsterHosting. It's stable and always accessible online.


---

## 🎯 Prerequisites

- **Source Code**: `git clone https://github.com/SALEH-SHERIF/Edu_plat.git`
- **.NET 8.0 SDK** (Windows/macOS)
- **Visual Studio 2022** or **VS Code**
- **SQL Server 2019+** (LocalDB/Express) or **Azure SQL**
- **Git**
- **SMTP credentials** (optional)
- **Firebase Server Key** (optional)

> Verify SDKs: `dotnet --list-sdks`

<details>
<summary>📦 Install Commands</summary>

| Requirement      | Windows                                 | macOS                              |
|------------------|-----------------------------------------|------------------------------------|
| .NET SDK         | `winget install Microsoft.DotNet.SDK.8` | `brew install --cask dotnet-sdk`   |
| Git              | `winget install Git.Git`                | `brew install git`                 |
</details>

---

## 🚀 Section 1: Build from Source

1. **Clone & Navigate**
   ```bash
   git clone https://github.com/SALEH-SHERIF/Edu_plat.git
   cd Edu_plat/JWT
   ```
2. **Configure** `appsettings.json`
   ```bash
   copy appsettings.json.sample appsettings.json  # Windows
   cp appsettings.json.sample appsettings.json    # macOS/Linux
   code appsettings.json
   ```
   Update:
   ```json
   "ConnectionStrings": { "DefaultConnection": "Server=YOUR_SERVER;Database=EduPlat;Trusted_Connection=True;" },
   "JWT": { "Key": "YOUR_SECRET_KEY", "Issuer": "EduPlatIssuer", "Audience": "EduPlatAudience", "DurationInMinutes": 60 },
   "MailSettings": { /* your SMTP config */ },
   "FirebaseSettings": { "Enabled": true, "ServerKey": "YOUR_FIREBASE_SERVER_KEY" }
   ```
3. **Migrate & Run**
   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.0
   dotnet ef database update
   dotnet run
   ```
4. **Access Endpoints**
   - HTTPS API: `https://localhost:5001`
   - HTTP API: `http://localhost:5000`
   - Swagger UI: `https://localhost:5001/swagger/index.html`

> *Tip:* On first HTTPS run trust the dev certificate: `dotnet dev-certs https --trust`.

---

## 🖥️ Section 2: Pre-built Executable

1. **Download & Extract**
   - Go to [Releases](https://github.com/SALEH-SHERIF/Edu_plat/releases).
   - Download the `.zip`/`.exe` (Windows) or `.tar.gz` (macOS) file.
   - Extract:
     - **Windows**: right-click → Extract All...
     - **macOS**: `tar -xzf Edu_plat_mac.tar.gz`

2. **Place `appsettings.json`**
   - Place `appsettings.json` next to `Edu_plat.exe`.

3. **Run the Application**
   - **Windows**: Open **Command Prompt** or **PowerShell**, navigate to the folder containing `Edu_plat.exe`, and run:
     ```bash
     .\Edu_plat.exe
     ```
   - **macOS/Linux**: Open **Terminal**, navigate to the folder containing `Edu_plat`, and run:
     ```bash
     chmod +x Edu_plat && ./Edu_plat
     ```

4. **Access the API**
   - After the API starts, it will listen on the default port. You should see a message like:
     ```
     Now listening on: http://0.0.0.0:7189
     ```
   - Open your browser and go to `http://localhost:7189` to access the API.

5. **Access Swagger UI**
   - To interact with the API using Swagger UI, navigate to:
     ```
     http://localhost:7189/swagger/index.html
     ```

---

> **Notes:**
> - If you have trouble accessing the API, make sure port `7189` is not blocked by another program or firewall.
> - To access the API from another device on the same network, replace `localhost` with the IP address of your machine, for example:
>   ```
>   http://192.168.1.100:7189
>   ```


## 🛠️ Configuration Tips

- 🔧 **Database**: open firewall ports; confirm credentials.
- 🔑 **JWT**: rotate your secret key periodically.
- 📧 **SMTP**: test via `telnet smtp.server.com 587`.
- 🔄 **Redis**: enable in `appsettings.json` if using caching.

---

## 🐞 Troubleshooting

- ❌ **DB Connection**: verify server name, auth method.
- ❌ **Migrations**: delete `Migrations/`, then:
  ```bash
  dotnet ef migrations add InitialCreate && dotnet ef database update
  ```
- ❌ **CORS**:
  ```csharp
  builder.Services.AddCors(opts => opts.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
  app.UseCors("AllowAll");
  ```
- ❌ **Port Conflict**: change `applicationUrl` in `Properties/launchSettings.json`.
- ❌ **SSL Warnings**: trust the dev cert.
- ❌ **Upload Limits**: adjust `MaxRequestBodySize` in `Program.cs` or proxy.
- ❌ **Missing Keys**: check `appsettings.json` for JWT, SMTP, Firebase.

---

> For issues or feature requests, open a new issue: [GitHub Issues](https://github.com/SALEH-SHERIF/Edu_plat/issues)

**Enjoy building with EduPlat! 🎉**

