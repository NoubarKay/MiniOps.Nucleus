# MiniOps.Nucleus
![NuGet](https://img.shields.io/nuget/v0.0.1-alpha/MiniOps.Nucleus.Core?style=flat-square)  
![License](https://img.shields.io/github/license/noubarkay/MiniOps.Nucleus?style=flat-square)  
![Size](https://img.shields.io/badge/package_size-26KB-lightgreen?style=flat-square)

**MiniOps.Nucleus** is a lightweight request metrics collector and dashboard package for .NET applications.  
At only **26 KB**, it’s designed to be blazing fast, minimal, and easy to integrate into your services.
---

## ✨ Features

- 🔎 **Request Metrics Tracking**  
  Captures request duration, status code, path, and timestamps.

- 🧵 **Concurrent Store**  
  Uses an in-memory concurrent queue for fast non-blocking writes.

- 💾 **Efficient Inserts**  
  Requests are flushed in batches to reduce DB write overhead.

- 📊 **Dashboard (Early Preview)**  
  - Realtime streaming chart of requests (via SignalR).  
  - Currently **only the graph is implemented**.  
  - More panels (status breakdown, latency distribution, etc.) coming soon.

- ⚡ **Ultra Lightweight**  
  With a **26 KB footprint**, MiniOps.Nucleus is one of the lightest metrics packages you can add to your project.

---
