# EduPlat Module

> [![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download) [![GitHub release](https://img.shields.io/github/v/release/SALEH-SHERIF/Edu_plat)](https://github.com/SALEH-SHERIF/Edu_plat/releases)

**EduPlat** is an educational platform that manages users (Students, Doctors, and Admins), Courses, Study Materials (PDF, Word, PowerPoint), and Online Exams (MCQ Questions). This JWT Module handles:

- 🔒 **Authentication & Authorization** using ASP.NET Core Identity
- 🪪 **JWT Token Issuance** and Revocation (Token Blacklisting)
- 🛡️ **Role Management**: `SuperAdmin`, `Admin`, `Doctor`, `Student`
- 📧 **Email & Notifications** via SMTP and Firebase

---

## 🎯 What You’ll Need First

Before diving in, gather these essentials:

- **Source Code Access**: Clone or download from GitHub
- **.NET 8.0 SDK** (Windows/macOS)
- **IDE**: Visual Studio 2022 / VS Code
- **Database**: SQL Server 2019+ (Express/LocalDB) or Azure SQL
- **Git** installed locally
- **SMTP Credentials** for email (optional)
- **Firebase Server Key** (optional)

> Tip: Use `dotnet --list-sdks` to verify your .NET SDK versions.

---

<details>
  <summary>📦 Prerequisites & Install Commands</summary>

| Requirement      | Windows                              | macOS                                   |
|------------------|--------------------------------------|-----------------------------------------|
| .NET SDK         | `winget install Microsoft.DotNet.SDK.8` | `brew install --cask dotnet-sdk`        |
| IDE / Editor     | Visual Studio 2022 / VS Code         | VS Code                                 |
| Database         | SQL Server 2019+ (LocalDB/Express)   | Azure SQL / Remote SQL Server           |
| Git              | `winget install Git.Git`             | `brew install git`                      |

</details>

---

## 🚀 Section 1: Build from Source

### 1. Clone the Repository
```bash
git clone https://github.com/SALEH-SHERIF/Edu_plat.git && cd Edu_plat/JWT
```

### 2. Configure `appsettings.json`
Click the icon or run:
```bash
code appsettings.json
``` 
and update values:

<details>
  <summary>Connection & JWT Settings</summary>

```json
"ConnectionStrings": { "DefaultConnection": "Server=YOUR_SERVER;Database=EduPlat;Trusted_Connection=True;" },
"JWT": { "Key": "YOUR_SECRET_KEY", "Issuer": "EduPlatIssuer", "Audience": "EduPlatAudience", "DurationInMinutes": 60 }
```
</details>

<details>
  <summary>Mail & Firebase Settings</summary>

```json
"MailSettings": { ... },
"FirebaseSettings": { "Enabled": true, "ServerKey": "YOUR_FIREBASE_SERVER_KEY" }
```
</details>

### 3. Apply Migrations & Run
```bash
# Install EF tool (one-time)
dotnet tool install --global dotnet-ef --version 8.0.0
# Update database
dotnet ef database update
# Start the API
dotnet run
```

**Endpoints**:
- 🌐 HTTPS: `https://localhost:5001`
- 🌐 HTTP : `http://localhost:5000`
- 📚 Swagger: `https://localhost:5001/swagger`

---

## 🖥️ Section 2: Pre-built Executable

<details>
  <summary>Download & Extract</summary>

1. **Releases Page**: [GitHub Releases](https://github.com/SALEH-SHERIF/Edu_plat/releases)
2. **Select Asset**: `.zip`/`.exe` (Windows) or `.tar.gz` (macOS)
3. **Extract**:
   - Windows: Right-click → Extract All...
   - macOS: `tar -xzf EduPlat_JWT_mac.tar.gz`
</details>

<details>
  <summary>Setup & Launch</summary>

1. **Add config**: place `appsettings.json` beside the executable (use same structure as source build).
2. **Run**:
   - Windows: `.\EduPlat.JWT.exe`
   - macOS: `chmod +x EduPlat.JWT && ./EduPlat.JWT`
</details>

---

## 🛠️ Configuration Tips

- 🔧 **Database**: Ensure connectivity & firewall (for Azure SQL)
- 🔑 **JWT**: Keep `Key` secret & rotate periodically
- 📧 **Email**: Test SMTP with `telnet smtp.server.com 587`
- 🔄 **Redis**: Configure `Redis:Enabled` in `appsettings.json` if using caching

---

## 🐞 Troubleshooting

> Use these quick fixes for common setup issues.

- ❌ **DB Connection Failed**: double-check server name, credentials, and network.
- ❌ **EF Migrations Error**: delete `Migrations/` then:
  ```bash
dotnet ef migrations add InitialCreate && dotnet ef database update
```
- ❌ **JWT Auth Issues**: match `Issuer` & `Audience`; sync system clocks.
- ❌ **CORS**: enable during dev in `Program.cs`:
  ```csharp
  builder.Services.AddCors(...);
  app.UseCors("AllowAll");
  ```
- ❌ **SMTP**: firewall rules & correct port; verify credentials.

---

> 🤔 Questions? Raise an issue on [GitHub Issues](https://github.com/SALEH-SHERIF/Edu_plat/issues). Enjoy! 🎉

