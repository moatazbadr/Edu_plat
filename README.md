# EduPlat Web API Module

> [![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download) [![GitHub release](https://img.shields.io/github/v/release/SALEH-SHERIF/Edu_plat)](https://github.com/SALEH-SHERIF/Edu_plat/releases)

## 📋 Release Notes

*Use this section as your GitHub Release **Description** when publishing the `.exe` package.*

**Version 2.3.4** — *EduPlat Web API* (`EduPlat.exe`)

**What's Inside:**

- ⚙️ **Standalone Executable**: `EduPlat.exe`—zero-dependency Windows binary.
- 🔒 **JWT Security**: Access tokens, blacklisting, and role-based access control.
- 🗂️ **Course & Materials**: Upload/download study resources (PDF, Word, PPT) up to 300 MB.
- 🎥 **Video Uploading**: Support for course-related videos.
- 📝 **Exam Engine**: CRUD for Exams, Questions (2–4 choices), scoring, and time limits.
- 📧 **Notifications**: SMTP emails + Firebase push for registration, material uploads, and exams.
- 🔄 **EF Core**: Auto migrations, seed data, SQL Server/Azure SQL support.
- 🔄 **Caching**: In-memory with optional Redis integration.
- 📚 **Swagger UI**: Explore and test all endpoints with Bearer token support.

**Before You Download:**

1. Install **.NET 8.0 Runtime**.
2. Prepare your `appsettings.json` (see `appsettings.json.sample`).
3. Ensure SQL Server/Azure SQL is accessible.

**Package Includes:**

- `EduPlat.exe`
- `appsettings.json.sample`
- `README.md` (this guide)

---

## 🚦 Live Demo

- **ngrok**: https://great-hot-impala.ngrok-free.app/swagger/index.html
- **RunASP.net**: https://eduplat123.runasp.net/swagger/index.html

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

## 🖥️ Section 2: Pre-built Executable

<details>
<summary>Download & Extract</summary>

1. Go to [Releases](https://github.com/SALEH-SHERIF/Edu_plat/releases)
2. Download `.zip`/`.exe` (Windows) or `.tar.gz` (macOS)
3. Extract:
   - Windows: right-click → Extract All...
   - macOS: `tar -xzf EduPlat_JWT_mac.tar.gz`
</details>

<details>
<summary>Configure & Launch</summary>

1. Place `appsettings.json` next to `EduPlat.exe`.
2. Run:
   - Windows: `./EduPlat.exe`
   - macOS/Linux: `chmod +x EduPlat && ./EduPlat`
</details>

---

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

