using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Nucleus.Core.Interfaces;
using Nucleus.Core.Models;

namespace Nucleus.Core.Middleware;

public class NucleusTrackerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, INucleusLogStore logStore)
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

        await logStore.SaveLogAsync(log);
    }
}