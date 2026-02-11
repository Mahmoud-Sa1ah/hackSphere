# PentestHub Setup Guide

## Quick Start

### 1. Database Setup

1. Install MySQL Server
2. Create a database:
   ```sql
   CREATE DATABASE PentestHub;
   ```
3. Update `backend-api/appsettings.json` with your connection string

### 2. Backend Setup

```bash
cd backend-api
dotnet restore
dotnet run
```

The API will start on `http://localhost:5000`

### 3. Frontend Setup

```bash
cd frontend-react
npm install
npm run dev
```

The frontend will start on `http://localhost:5173`

### 4. Launcher Setup

```bash
cd launcher-client
dotnet build
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Run the launcher:
```bash
./bin/Release/net9.0/win-x64/publish/PentestHub.Launcher.exe
```

## First User

1. Register a new account through the frontend
2. Default role is "Student" (RoleId: 2)
3. Admin role (RoleId: 1) can be assigned manually in the database

## AI Integration (Optional)

1. Install GPT4All or set up a compatible LLM API
2. Update `backend-api/appsettings.json`:
   ```json
   {
     "AI": {
       "ApiUrl": "http://localhost:4891/v1/chat/completions",
       "Model": "gpt4all-j"
     }
   }
   ```

## Tools Setup

1. Place your pentesting tool executables in `tools-containerized/`
2. Tools should be named exactly as they appear in the database (e.g., `nmap.exe`, `metasploit.bat`)
3. The launcher will automatically find and execute them

## Troubleshooting

### Backend won't start
- Check MySQL connection string
- Ensure port 5000 is available
- Check firewall settings

### Frontend can't connect to backend
- Verify backend is running on port 5000
- Check CORS settings in `Program.cs`
- Verify proxy settings in `vite.config.ts`

### Launcher won't connect
- Verify backend is running
- Check API URL in launcher command
- Ensure SignalR hub is accessible

### Tools not executing
- Verify tool exists in `tools-containerized/`
- Check file permissions
- Ensure tool is executable (Windows: .exe, .bat, .cmd)

