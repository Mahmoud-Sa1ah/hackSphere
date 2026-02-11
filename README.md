# PentestHub - Cybersecurity Pentesting Platform HackSphere

A comprehensive full-stack pentesting platform built with angular, ASP.NET Core, and a portable launcher client.

## 🏗️ Architecture

- **Frontend**: React 18 + TypeScript + Vite + TailwindCSS
- **Backend**: ASP.NET Core 9 Web API
- **Database**: MySQL
- **Real-time**: SignalR
- **AI Integration**:LLM API (Gemini)
- **Launcher**: C# .NET 9 Console Application

## 📁 Project Structure

```
/
├── frontend-angular/          # angular frontend application
├── backend-api/             # ASP.NET Core Web API
├── launcher-client/         # Portable C# launcher executable
├── tools-containerized/     # Directory for pentesting tools
├── docs/                    # Documentation
└── diagrams/                # Architecture diagrams
```

## 🚀 Getting Started

### Prerequisites

- Node.js 18+ and npm
- .NET 9 SDK
- MySQL Server
- GPT4All or compatible LLM API (optional, for AI features)

### Backend Setup

1. Navigate to `backend-api`:
   ```bash
   cd backend-api
   ```

2. Update `appsettings.json` with your MySQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=PentestHub;User=username;Password=yourpassword;Port=3306;"
     }
   }
   ```

3. Restore packages and run:
   ```bash
   dotnet restore
   dotnet run
   ```

   The API will be available at `http://localhost:53812`

### Frontend Setup

1. Navigate to `frontend-angular`:
   ```bash
   cd frontend-angular
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start development server:
   ```bash
   npm start
   ```

   The frontend will be available at `http://localhost:4200`

### Launcher Setup

1. Navigate to `launcher-client`:
   ```bash
   cd launcher-client
   ```

2. Build the launcher:
   ```bash
   dotnet build
   ```

3. Publish as portable executable:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

4. Run the launcher:
   ```bash
   ./bin/Release/net9.0/win-x64/publish/PentestHub.Launcher.exe
   ```

   Or with custom API URL and tools path:
   ```bash
   ./PentestHub.Launcher.exe http://localhost:5000 ../tools-containerized
   ```

## 🔧 Configuration

### AI Integration

Update `backend-api/appsettings.json`:
```json
  "AI": {
    "ApiUrl": "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent",
    "ApiKey": "AIzaSyDQovhJuBvKOM4wNT1FuheKJ2pQKldqNzI",
    "Model": "gemini-flash-latest"
  }
```

### Tools Directory

Place your pentesting tool executables in the `tools-containerized` directory. The launcher will look for tools by name (e.g., `nmap.exe`, `nmap.bat`, etc.).

## 📚 Features

### Frontend
- ✅ Authentication (Login/Register)
- ✅ Dashboard with real-time updates
- ✅ Tools management (60+ tools)
- ✅ Labs and challenges
- ✅ AI chat assistant
- ✅ Report generation and download
- ✅ Dark mode support
- ✅ Real-time notifications via SignalR
- ✅ Responsive design

### Backend
- ✅ JWT Authentication
- ✅ Role-based authorization
- ✅ SignalR hubs for real-time communication
- ✅ AI integration for scan analysis
- ✅ PDF report generation
- ✅ MySQL database with Entity Framework Core
- ✅ RESTful API endpoints

### Launcher
- ✅ Portable executable (no installation required)
- ✅ SignalR client for real-time commands
- ✅ Tool execution with output streaming
- ✅ Auto-reconnect on connection loss

## 🗄️ Database Schema

The platform uses the following main entities:
- Users & Roles
- Tools (60+ predefined tools)
- Scan History
- Reports
- Labs & Lab Results
- Notifications
- AI Conversations

## 🔐 Security

- Passwords are hashed using BCrypt
- JWT tokens for authentication
- Role-based access control
- SQL injection protection via EF Core
- CORS configuration for frontend

## 📝 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/change-password` - Change password

### Tools
- `GET /api/tools` - Get all tools
- `GET /api/tools/{id}` - Get tool by ID
- `POST /api/tools/scan` - Start a scan
- `GET /api/tools/scans` - Get user scans

### Labs
- `GET /api/labs` - Get all labs
- `POST /api/labs/{id}/submit` - Submit lab solution
- `GET /api/labs/results` - Get user lab results

### Reports
- `POST /api/reports/generate/{scanId}` - Generate report
- `GET /api/reports` - Get user reports
- `GET /api/reports/{id}/download` - Download report PDF

### AI
- `POST /api/ai/analyze` - Analyze scan output
- `POST /api/ai/chat` - Chat with AI assistant

### Notifications
- `GET /api/notifications` - Get notifications
- `POST /api/notifications/{id}/read` - Mark as read

### Dashboard
- `GET /api/dashboard` - Get dashboard data

## 🚢 Deployment

### Frontend
Deploy to Netlify, Vercel, or any static hosting:
```bash
cd frontend-react
npm run build
# Deploy the 'dist' folder
```

### Backend
Deploy to Azure, Render, or DigitalOcean:
```bash
cd backend-api
dotnet publish -c Release
# Deploy the published files
```

### Launcher
The launcher is a portable executable that can be distributed as a ZIP file containing:
- `PentestHub.Launcher.exe`
- `tools-containerized/` directory with tools

## 📄 License

This project is for educational and authorized security testing purposes only.

## ⚠️ Disclaimer

This platform is intended for authorized penetration testing and security research only. Unauthorized use against systems you do not own or have explicit permission to test is illegal and unethical.

