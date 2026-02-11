# PentestHub Architecture

## System Overview

PentestHub is a full-stack cybersecurity pentesting platform with three main components:

1. **React Frontend** - User interface and real-time updates
2. **ASP.NET Core Backend** - REST API, SignalR hubs, and business logic
3. **C# Launcher Client** - Portable tool execution engine

## Component Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    React Frontend (Vite)                     │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ Dashboard│  │  Tools   │  │   Labs   │  │    AI    │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         SignalR Client (Real-time Updates)          │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────┬─────────────────────────────────────┘
                       │ HTTP/REST + WebSocket
                       │
┌──────────────────────▼─────────────────────────────────────┐
│              ASP.NET Core Web API (Backend)                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐   │
│  │ Controllers  │  │ SignalR Hubs │  │   Services   │   │
│  └──────────────┘  └──────────────┘  └──────────────┘   │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐   │
│  │   JWT Auth   │  │  AI Service  │  │ PDF Reports │   │
│  └──────────────┘  └──────────────┘  └──────────────┘   │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐ │
│  │         Entity Framework Core (MySQL)                  │ │
│  └──────────────────────────────────────────────────────┘ │
└──────────────────────┬─────────────────────────────────────┘
                       │ SignalR
                       │
┌──────────────────────▼─────────────────────────────────────┐
│              C# Launcher Client (Portable EXE)              │
│  ┌──────────────────────────────────────────────────────┐ │
│  │         SignalR Client (Command Receiver)            │ │
│  └──────────────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────────────┐ │
│  │         Tool Executor (Process Management)            │ │
│  └──────────────────────────────────────────────────────┘ │
└──────────────────────┬─────────────────────────────────────┘
                       │ Execute
                       │
┌──────────────────────▼─────────────────────────────────────┐
│              Tools Containerized Directory                   │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │  nmap    │  │metasploit│  │   zap    │  │   ...    │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Data Flow

### Scan Execution Flow

1. User selects tool and target in Frontend
2. Frontend sends POST `/api/tools/scan` to Backend
3. Backend creates ScanHistory record
4. Backend sends SignalR message to Launcher: "RunTool"
5. Launcher receives command and executes tool binary
6. Launcher streams output back via SignalR: "SendOutput"
7. Backend updates ScanHistory with output
8. When complete, Launcher sends "ScanComplete"
9. Backend triggers AI analysis
10. Backend sends notification to Frontend via SignalR
11. Frontend updates UI with results

### Real-time Updates

- **SignalR Launcher Hub**: Connects launcher to backend, handles tool execution commands
- **SignalR Notification Hub**: Pushes notifications to frontend users
- **Frontend SignalR Client**: Receives real-time updates and updates UI

## Security Architecture

- **JWT Authentication**: All API endpoints require valid JWT token
- **Role-Based Access**: Admin, Student, Professional roles
- **Password Hashing**: BCrypt with secure salt
- **CORS**: Configured for frontend origin only
- **SQL Injection Protection**: Entity Framework Core parameterized queries

## Database Schema

- **Users**: Authentication and user information
- **Roles**: Role-based access control
- **Tools**: 60+ predefined pentesting tools
- **ScanHistory**: Scan execution records with AI analysis
- **Reports**: Generated PDF reports
- **Labs**: Security challenges
- **LabResults**: User lab submissions with AI feedback
- **Notifications**: Real-time user notifications
- **AIConversations**: Chat history with AI assistant

## Technology Stack

### Frontend
- React 18
- TypeScript
- Vite
- TailwindCSS
- React Router
- Zustand (State Management)
- Axios (HTTP Client)
- SignalR Client
- React Hot Toast

### Backend
- ASP.NET Core 9
- Entity Framework Core
- MySQL (Pomelo Provider)
- SignalR
- JWT Bearer Authentication
- QuestPDF
- BCrypt.Net

### Launcher
- .NET 9 Console Application
- SignalR Client
- Process Management

## Deployment Architecture

### Development
- Frontend: `localhost:5173` (Vite dev server)
- Backend: `localhost:5000` (Kestrel)
- Launcher: Local executable

### Production
- Frontend: Static hosting (Netlify/Vercel)
- Backend: Cloud hosting (Azure/Render/DigitalOcean)
- Launcher: Portable EXE distributed to users

## Scalability Considerations

- **Stateless Backend**: Can scale horizontally
- **SignalR**: Supports multiple instances with Redis backplane (future)
- **Database**: MySQL can be replicated for read scaling
- **Launcher**: Multiple instances can connect simultaneously
- **AI Service**: Can be load balanced if needed

