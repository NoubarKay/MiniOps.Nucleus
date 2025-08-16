# MiniOps.Nucleus

[![NuGet](https://img.shields.io/nuget/vpre/MiniOps.Nucleus.Core?style=flat-square)](https://www.nuget.org/packages/MiniOps.Nucleus.Core)
[![Downloads](https://img.shields.io/nuget/dt/MiniOps.Nucleus.Core?style=flat-square)](https://www.nuget.org/packages/MiniOps.Nucleus.Core)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE)
[![Size](https://img.shields.io/badge/package_size-26KB-lightgreen?style=flat-square)]()
[![Release](https://img.shields.io/github/v/release/NoubarKay/MiniOps.Nucleus?style=flat-square)](https://github.com/NoubarKay/MiniOps.Nucleus/releases)
[![Last Commit](https://img.shields.io/github/last-commit/NoubarKay/MiniOps.Nucleus?style=flat-square)](https://github.com/NoubarKay/MiniOps.Nucleus/commits)
[![Build Status](https://img.shields.io/github/actions/workflow/status/NoubarKay/MiniOps.Nucleus/build.yml?style=flat-square)](https://github.com/NoubarKay/MiniOps.Nucleus/actions)


**MiniOps.Nucleus** is a **high-performance, ultra-lightweight request metrics collector and real-time dashboard** for .NET applications. With a footprint of just **26 KB**, it provides a minimal yet powerful solution for monitoring your application's request traffic without adding overhead.

It captures key request metrics — including duration, status codes, paths, and timestamps — and stores them in a **thread-safe, in-memory queue** for **non-blocking, high-speed writes**. Logs are **flushed in batches** to your database to reduce write operations and automatically **cleaned up** based on configurable retention settings.

MiniOps.Nucleus also includes an **early-preview SignalR-powered dashboard** for real-time visualization of request patterns, making it easy to monitor performance, spot spikes, and identify bottlenecks. Its lightweight design and simple integration make it ideal for **APIs, and high-throughput web applications**.


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

//...

// Start Nucleus (seeding + background services)
await app.UseNucleus();

app.Run();
```
That’s it—requests flowing through the app are tracked and written to your database in batches. Old rows are purged automatically based on LogTTLSeconds.

---

## Configuration Options
| Property                    | Type                   | Default     | Description                                                                                                         |
| --------------------------- | ---------------------- | ----------- | ------------------------------------------------------------------------------------------------------------------- |
| `DatabaseType`              | `NucleusDatabaseTypes` | —           | The type of database MiniOps will use. Supported: SqlServer (More to come)                                          |
| `ConnectionString`          | `string`               | `""`        | The connection string for the selected database                                                                     |
| `LogTTLSeconds`             | `int`                  | `1`         | Time-to-live for request logs, in seconds. Expired logs are deleted automatically by the background cleanup service |
| `BatchFlushIntervalSeconds` | `float`                | `1`         | How often, in seconds, the background flush service writes accumulated logs to the database                         |
| `SchemaName`                | `string`               | `"Nucleus"` | Optional custom schema/table name for request logs                                                                  |
| `SeedDatabase`              | `bool`                 | `false`     | Whether to seed the database automatically on startup                                                               |
---

## ❓ FAQ

**Is it thread-safe?**  
Yes, MiniOps.Nucleus uses a ConcurrentQueue for non-blocking, thread-safe writes.

**Can it work without a database?**  
Currently, a database is required for batch inserts.

**Which databases are supported?**  
SQLServer is fully supported; PostgreSQL, MySQL, SQLite support coming soon.

**Does it impact app performance?**  
It is designed to be lightweight and minimal, with <1ms overhead per request.

---


## 🤝 Contributing
Contributions, issues, and feature requests are welcome!  
Please check the [issues](https://github.com/NoubarKay/MiniOps.Nucleus/issues) before creating new ones.

---

## 📄 License
MiniOps.Nucleus is licensed under the [MIT License](LICENSE).
