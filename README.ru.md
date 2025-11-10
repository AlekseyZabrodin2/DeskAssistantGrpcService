# 🧩 DeskAssistant gRPC Service

**DeskAssistantGrpcService** — это серверная часть проекта **DeskAssistant**, реализованная на .NET 9 и основанная на **gRPC**.  
Сервис отвечает за управление задачами, хранение данных в PostgreSQL и взаимодействие с другими компонентами системы.

---

## 🚀 Основные возможности

- 🧠 gRPC API для взаимодействия с клиентами DeskAssistant  
- 💾 CRUD-операции с задачами  
- 🐘 Подключение к PostgreSQL через Entity Framework Core  
- 🔔 Уведомления через TelegramNotificationService  
- ⚙️ Возможность работы как Windows-служба  

---

## 🏗️ Архитектура проекта

```

DeskAssistantGrpcService
│
├── DataBase/
│   └── TasksDbContext.cs          # Контекст базы данных EF Core
│
├── Services/
│   ├── TaskGrpcService.cs         # Реализация gRPC-сервиса
│   └── TaskServiceImpl.cs         # Логика бизнес-операций
│
├── Protos/
│   └── task.proto                 # gRPC контракты
│
├── Program.cs                     # Точка входа приложения
├── appsettings.json               # Конфигурация (строка подключения, порты)
└── nlog.config                    # Настройка логирования

````

---

## ⚙️ Установка и запуск

### 1. Сборка проекта

```bash
dotnet publish -c Release -r win-x64
````

После сборки исполняемый файл появится по пути:

```
D:\Develop\DeskAssistantGrpcService\DeskAssistantGrpcService\bin\Release\net9.0\win-x64\publish\
```

---

### 2. Настройка подключения к PostgreSQL

Перед первым запуском открой `appsettings.json` и укажи свои данные для подключения:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DeskAssistantDB;Username=postgres;Password=yourpassword"
  }
}
```

---

### 3. Установка как Windows-службы

#### 🔹 Удалить старую версию (если есть)

```bash
sc delete DeskAssistantGrpcService
```

#### 🔹 Создать новую службу

```bash
sc create DeskAssistantGrpcService binPath="D:\Develop\DeskAssistantGrpcService\DeskAssistantGrpcService\bin\Release\net9.0\win-x64\publish\DeskAssistantGrpcService.exe" DisplayName="Grpc Service for DeskAssistant" start=auto
```

#### 🔹 Запустить службу

```bash
sc start DeskAssistantGrpcService
```

#### 🔹 Остановить службу

```bash
sc stop DeskAssistantGrpcService
```

---

## 🧠 Отладка

* При запуске через **Visual Studio** используется порт `5000` (HTTP) и `5001` (HTTPS).
* При запуске как служба — тот порт, который указан в `appsettings.json` или `launchSettings.json`.
* Можно тестировать через любой gRPC-клиент (например, [BloomRPC](https://github.com/bloomrpc/bloomrpc) или `grpcurl`).

---

## 🪵 Логирование (NLog)

Проект использует **NLog** для ведения логов и записи ошибок в файлы и системный журнал Windows.

Пример `nlog.config`:

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

📂 Логи сохраняются в:

```
DeskAssistantGrpcService\logs\
```

---

## 🧩 Пример взаимодействия через gRPC

```protobuf
service TaskService {
  rpc GetAllTasks (EmptyRequest) returns (GetAllTasksResponse);
  rpc CreateTask (TaskItem) returns (CreateTaskResponse);
}
```

Клиент (на C#):

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

## 🧰 Используемые технологии

| Компонент      | Библиотека / Версия                   |
| -------------- | ------------------------------------- |
| 💡 .NET        | 9.0                                   |
| 🔗 gRPC        | Grpc.AspNetCore                       |
| 🐘 PostgreSQL  | Npgsql.EntityFrameworkCore.PostgreSQL |
| ⚙️ EF Core     | Microsoft.EntityFrameworkCore         |
| 🪵 Логирование | NLog                                  |
| 💬 Telegram    | TelegramNotificationService           |
| 💻 ОС          | Windows 10+ / Windows Server 2019+    |

---

## 🧩 Структура конфигурации

```json
{
  "Kestrel": {
    "Endpoints": {
      "Grpc": {
        "Url": "https://localhost:5001"
      }
    }
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

## 📜 Лицензия

MIT License © 2025 — **Aleksey Zabrodin (Zabrodin_DevTech)**
Проект можно использовать и модифицировать свободно, с сохранением упоминания автора.


> 💡 *DeskAssistantGrpcService — часть экосистемы DeskAssistant: персонального помощника для автоматизации и продуктивной работы.*