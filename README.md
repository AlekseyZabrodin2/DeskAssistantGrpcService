```markdown
# 🧩 DeskAssistant gRPC Service

**DeskAssistantGrpcService** is the backend part of the **DeskAssistant** project, implemented on .NET 9 and based on **gRPC**.  
The service is responsible for task management, data storage in PostgreSQL, and interaction with other system components.

---

## 🚀 Key Features

- 🧠 gRPC API for interaction with DeskAssistant clients  
- 💾 CRUD operations with tasks  
- 🐘 PostgreSQL connection via Entity Framework Core  
- 🔔 Notifications via TelegramNotificationService  
- ⚙️ Capability to run as a Windows Service  

---

## 🏗️ Project Architecture

```
DeskAssistantGrpcService
│
├── DataBase/
│   └── TasksDbContext.cs          # EF Core database context
│
├── Services/
│   ├── TaskGrpcService.cs         # gRPC service implementation
│   └── TaskServiceImpl.cs         # Business logic operations
│
├── Protos/
│   └── task.proto                 # gRPC contracts
│
├── Program.cs                     # Application entry point
├── appsettings.json               # Configuration (connection string, ports)
└── nlog.config                    # Logging configuration
```

---

## ⚙️ Installation and Setup

### 1. Project Build

```bash
dotnet publish -c Release -r win-x64
```

After building, the executable will appear at:
```
bin\Release\net9.0\win-x64\publish\
```

---

### 2. PostgreSQL Connection Configuration

Before first launch, open `appsettings.json` and specify your connection details:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DeskAssistantDB;Username=postgres;Password=yourpassword"
  }
}
```

---

### 3. Database Migrations

Before first launch, you need to apply migrations to create the database schema.

#### 🔹 Option A: Via CLI (recommended for development)

Execute in the project root:

```bash
dotnet ef database update
```

#### 🔹 Option B: Automatic on startup

Ensure that `Program.cs` contains code that automatically applies migrations on application startup (e.g., `context.Database.Migrate()`).

---

### 4. Installation as Windows Service

#### 🔹 Remove old version (if exists)

```bash
sc delete DeskAssistantGrpcService
```

#### 🔹 Create new service

```bash
sc create DeskAssistantGrpcService binPath="D:\Develop\DeskAssistantGrpcService\DeskAssistantGrpcService\bin\Release\net9.0\win-x64\publish\DeskAssistantGrpcService.exe" DisplayName="Grpc Service for DeskAssistant" start=auto
```

#### 🔹 Start service

```bash
sc start DeskAssistantGrpcService
```

#### 🔹 Stop service

```bash
sc stop DeskAssistantGrpcService
```

---

## 🧠 Debugging

* When running through **Visual Studio**, port `5000` (HTTP) and `5001` (HTTPS) are used.
* When running as a service - the port specified in `appsettings.json` is used.
* You can test using any gRPC client (e.g., [BloomRPC](https://github.com/bloomrpc/bloomrpc) or `grpcurl`).

---

## 🪵 Logging (NLog)

The project uses **NLog** for logging and writing errors to files and Windows Event Log.

Example `nlog.config`:

```xml
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true"
      internalLogLevel="Off">

  <targets>
    <target xsi:type="File" name="logfile" fileName="logs/${shortdate}.log"
            layout="${longdate}|${level}|${logger}|${message}|${exception}" />
    <target xsi:type="EventLog" name="eventlog" layout="${message}" source="DeskAssistantGrpcService" />
  </targets>

  <rules>
    <logger name="*" minlevel="Info" writeTo="logfile,eventlog" />
  </rules>
</nlog>
```

📂 Logs are saved to:
```
DeskAssistantGrpcService\logs\
```

---

## 🧩 gRPC Interaction Example

```protobuf
service TaskService {
  rpc GetAllTasks (EmptyRequest) returns (GetAllTasksResponse);
  rpc CreateTask (TaskItem) returns (CreateTaskResponse);
}
```

Client (in C#):

```csharp
var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new TaskService.TaskServiceClient(channel);

var response = await client.GetAllTasksAsync(new EmptyRequest());
foreach (var task in response.Tasks)
{
    Console.WriteLine($"{task.Name} ({task.DueDate})");
}
```

---

## 🧰 Technologies Used

| Component      | Library / Version                |
| -------------- | -------------------------------- |
| 💡 .NET        | 9.0                              |
| 🔗 gRPC        | Grpc.AspNetCore                  |
| 🐘 PostgreSQL  | Npgsql.EntityFrameworkCore.PostgreSQL |
| ⚙️ EF Core     | Microsoft.EntityFrameworkCore    |
| 🪵 Logging     | NLog                             |
| 💬 Telegram    | TelegramNotificationService      |
| 💻 OS          | Windows 10+ / Windows Server 2019+ |

---

## 🧩 Configuration Structure

```json
{
  "Kestrel": {
    "Endpoints": {
      "Grpc": {
        "Url": "https://localhost:5001"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DeskAssistantDB;Username=postgres;Password=yourpassword"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

---

## 📜 License

MIT License © 2025 — **Aleksey Zabrodin (Zabrodin_DevTech)**  
The project can be freely used and modified with attribution to the author.

> 💡 *DeskAssistantGrpcService is part of the DeskAssistant ecosystem: a personal assistant for automation and productive work.*"# DeskAssistantGrpcService" 
