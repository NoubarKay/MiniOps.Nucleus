using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Nucleus.Core.Models;
using Nucleus.Core.Stores;

namespace Nucleus.Core.Middleware;

public class NucleusTrackerMiddleware(RequestDelegate next, IRequestStore logStore)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await next(context);

        sw.Stop();

        var log = new NucleusLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            DurationMs = sw.ElapsedMilliseconds,
            StatusCode = context.Response.StatusCode,
            Path = context.Request.Path
        };

        logStore.Add(log);
    }
}