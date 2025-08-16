# MiniOps.Nucleus

[![NuGet](https://img.shields.io/nuget/vpre/MiniOps.Nucleus.Core?style=flat-square)](https://www.nuget.org/packages/MiniOps.Nucleus.Core) 
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE) 
[![Size](https://img.shields.io/badge/package_size-26KB-lightgreen?style=flat-square)]()

**MiniOps.Nucleus** is a **lightweight request metrics collector and dashboard** for .NET applications.  
At only **26 KB**, it’s designed to be **blazing fast, minimal, and effortless** to integrate into your services.

---

## ✨ Features

- 🔎 **Request Metrics Tracking**  
  Collects request duration, status codes, paths, and timestamps.

- 🧵 **Concurrent In-Memory Store**  
  Built with a thread-safe concurrent queue for **non-blocking, high-speed writes**.

- 💾 **Batch Inserts & Deletes**  
  Flushes requests to the database in batches, reducing write overhead. Deletes old rows automatically to reduce DB bloat.

- 📊 **Dashboard (Early Preview)**  
  - Real-time streaming chart of requests (via SignalR).  
  - Currently, **only the request graph is implemented**.  
  - Upcoming panels: status breakdown, latency distribution, and more.

- ⚡ **Ultra Lightweight**  
  With a **26 KB footprint**, MiniOps.Nucleus is one of the lightest metrics solutions you can add to your project.

---

## Getting Started

Quick start
Register Nucleus in DI and configure options:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Optional: Swagger, etc.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Nucleus Core
builder.Services.AddNucleus(op =>
{
    op.DatabaseType = NucleusDatabaseTypes.SQLServer; // SQLServer
    op.ConnectionString = "<your-connection-string>";
    op.SchemaName = "Nucleus";           // optional, default "Nucleus"
    op.LogTTLSeconds = 60;               // delete logs older than 60s
    op.BatchFlushIntervalSeconds = 1;    // flush in-memory logs every 1s
    op.SeedDatabase = true;              // auto-create schema/table if needed
});

var app = builder.Build();

// Optional: dev-time tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Your endpoints
app.MapGet("/ping", () => Results.Ok("pong"));

// Start Nucleus (seeding + background services)
await app.UseNucleus();

app.Run();
```
That’s it—requests flowing through the app are tracked and written to your database in batches. Old rows are purged automatically based on LogTTLSeconds.

--
## 🤝 Contributing
Contributions, issues, and feature requests are welcome!  
Please check the [issues](https://github.com/NoubarKay/MiniOps.Nucleus/issues) before creating new ones.
