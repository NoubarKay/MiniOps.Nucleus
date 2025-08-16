# MiniOps.Nucleus

[![NuGet](https://img.shields.io/nuget/vpre/MiniOps.Nucleus.Core?style=flat-square)](https://www.nuget.org/packages/MiniOps.Nucleus.Core) 
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE) 
[![Size](https://img.shields.io/badge/package_size-26KB-lightgreen?style=flat-square)]()
[![Changelog](https://img.shields.io/github/0.0.1-alpha/release/NoubarKay/MiniOps.Nucleus?style=flat-square)]()


**MiniOps.Nucleus** is a **lightweight request metrics collector and dashboard** for .NET applications.  
At only **26 KB**, it’s designed to be **blazing fast, minimal, and effortless** to integrate into your services.

---

## ✨ Features

- 🔎 **Request Metrics Tracking**  
  Collects request duration, status codes, paths, and timestamps.

- 🧵 **Concurrent In-Memory Store**  
  Built with a thread-safe concurrent queue for **non-blocking, high-speed writes**.

- 💾 **Batch Inserts**  
  Flushes requests to the database in batches, reducing write overhead.

- 📊 **Dashboard (Early Preview)**  
  - Real-time streaming chart of requests (via SignalR).  
  - Currently, **only the request graph is implemented**.  
  - Upcoming panels: status breakdown, latency distribution, and more.

- ⚡ **Ultra Lightweight**  
  With a **26 KB footprint**, MiniOps.Nucleus is one of the lightest metrics solutions you can add to your project.

---
