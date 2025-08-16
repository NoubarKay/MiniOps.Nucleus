using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Nucleus.Core.Models;
using Nucleus.Core.Stores;

namespace Nucleus.Core.Middleware;

public class NucleusTrackerMiddleware(RequestDelegate next, IRequestStore logStore)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        int statusCode = 200;

        try
        {
            await next(context);
            statusCode = context.Response.StatusCode;
        }
        catch
        {
            statusCode = 500;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var metric = new NucleusLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                DurationMs = stopwatch.ElapsedMilliseconds,
                StatusCode = statusCode,
                Path = context.Request?.Path.Value ?? "/"
            };

            logStore.Add(metric);
        }
    }
}