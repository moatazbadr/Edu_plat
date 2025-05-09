![EduPlat Logo](https://placeholder-url-to-your-logo-image/EduPlat-logo.png)

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
