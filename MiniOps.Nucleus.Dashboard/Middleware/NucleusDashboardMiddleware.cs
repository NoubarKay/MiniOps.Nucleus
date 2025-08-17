using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Nucleus.Core.Hubs;

namespace Nucleus.Dashboard.Middleware;

public static class NucleusDashboardMiddleware
{
    public static void AddNucleusDashboardService(this IServiceCollection services)
    {
        services.AddRouting();
        services.AddSignalR();
        services.AddCors(options =>
        {
            options.AddPolicy("Allow_Nucleus_Dashboard", policy =>
            {
                policy.SetIsOriginAllowed(x => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        services.AddControllers().AddApplicationPart(typeof(NucleusDashboardMiddleware).Assembly);
    }
    public static void UseNucleusDashboard(this IApplicationBuilder app, string basePath)
    {
        // Get the assembly and set up the embedded file provider (adjust the namespace as needed)
        var assembly = Assembly.GetExecutingAssembly();
        var embeddedFileProvider = new EmbeddedFileProvider(assembly, "Nucleus.Dashboard.wwwroot");
        var contents = embeddedFileProvider.GetDirectoryContents(string.Empty);

        foreach (var file in contents)
        {
            if (file.Exists)
            {
                Console.WriteLine(file.Name);
            }
        }
        // Ensure basePath starts with a "/" and does not end with one.
        if (string.IsNullOrEmpty(basePath))
            basePath = "/";
        if (!string.IsNullOrEmpty(basePath) && !basePath.StartsWith('/'))
            basePath = "/" + basePath;

        basePath = basePath.TrimEnd('/');

        // Map a branch for the basePath
        app.Map(basePath, dashboardApp =>
        {

            dashboardApp.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = embeddedFileProvider,
                DefaultFileNames = new[] { "index.html" }
            });

            // Serve static files from the embedded provider
            dashboardApp.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedFileProvider
            });

            // Redirect ticker asset requests that lack the basePath segment
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/dashboard-1") &&
                    !context.Request.Path.StartsWithSegments($"{basePath}/dashboard"))
                {
                    var correctedPath = $"{basePath}{context.Request.Path}";
                    context.Response.Redirect(correctedPath);
                    return;
                }

                await next();
            });

            // Set up routing and CORS for this branch
            dashboardApp.UseRouting();
            dashboardApp.UseCors("Allow_Nucleus_Dashboard");

            // Combine all endpoint registrations into one call
            dashboardApp.UseEndpoints(endpoints =>
            {
                // Map controller routes (e.g., Home/Index)
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: $"{basePath}{{controller=Home}}/{{action=Index}}/{{id?}}"
                );

                // Map the SignalR hub.
                // Inside the branch, map with a relative path.
                endpoints.MapHub<NucleusHub>("/nucleus-hub");
            });

            // SPA fallback middleware: if no route is matched, serve the modified index.html
            dashboardApp.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 404 &&
                    context.Request.PathBase.Value != null &&
                    context.Request.PathBase.Value.StartsWith(basePath))
                {
                    var file = embeddedFileProvider.GetFileInfo("index.html");
                    if (file.Exists)
                    {
                        using var stream = file.CreateReadStream();
                        using var reader = new StreamReader(stream);
                        var htmlContent = await reader.ReadToEndAsync();

                        // Inject the base tag and other replacements into the HTML
                        htmlContent = ReplaceBasePath(htmlContent, basePath);

                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync(htmlContent);
                    }
                }
            });
        });
    }
    
    private static string ReplaceBasePath(string htmlContent, string basePath)
    {
        var regex = new System.Text.RegularExpressions.Regex("(src|href|action)=\"/(?!/)");
        htmlContent = regex.Replace(htmlContent, $"$1=\"{basePath}/");
        return htmlContent.Replace("__base_path__", basePath);
    }
}