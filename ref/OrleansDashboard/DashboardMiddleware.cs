﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Orleans;
using OrleansDashboard.Implementation;
using OrleansDashboard.Model;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace OrleansDashboard
{
    public sealed class DashboardMiddleware
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        private const int REMINDER_PAGE_SIZE = 50;
        private readonly IOptions<DashboardOptions> options;
        private readonly DashboardLogger logger;
        private readonly RequestDelegate next;
        private readonly IDashboardClient client;

        public DashboardMiddleware(RequestDelegate next,
            IGrainFactory grainFactory,
            IOptions<DashboardOptions> options,
            DashboardLogger logger)
        {
            this.options = options;
            this.logger = logger;
            this.next = next;
            this.client = new DashboardClient(grainFactory);
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;

            if (request.Path == "/" || string.IsNullOrEmpty(request.Path))
            {
                await WriteIndexFile(context);

                return;
            }
            if (request.Path == "/favicon.ico")
            {
                await WriteFileAsync(context, "favicon.ico", "image/x-icon");

                return;
            }
            if (request.Path == "/index.min.js")
            {
                await WriteFileAsync(context, "index.min.js", "application/javascript");

                return;
            }

            if (request.Path == "/version")
            {
                await WriteJson(context, new { version = typeof (DashboardMiddleware).Assembly.GetName().Version.ToString() });

                return;
            }

            if (request.Path == "/DashboardCounters")
            {
                var result = await client.DashboardCounters();
                await WriteJson(context, result.Value);

                return;
            }

            if (request.Path == "/ClusterStats")
            {
                var result = await client.ClusterStats();

                await WriteJson(context, result.Value);

                return;
            }

            if (request.Path == "/Reminders")
            {
                try
                {
                    var result = await client.GetReminders(1, REMINDER_PAGE_SIZE);

                    await WriteJson(context, result.Value);
                }
                catch
                {
                    // if reminders are not configured, the call to the grain will fail
                    await WriteJson(context, new ReminderResponse { Reminders = new ReminderInfo[0], Count = 0 });
                }

                return;
            }

            if (request.Path.StartsWithSegments("/Reminders", out var pageString1) && int.TryParse(pageString1.ToValue(), out var page))
            {
                try
                {
                    var result = await client.GetReminders(page, REMINDER_PAGE_SIZE);

                    await WriteJson(context, result.Value);
                }
                catch
                {
                    // if reminders are not configured, the call to the grain will fail
                    await WriteJson(context, new ReminderResponse { Reminders = new ReminderInfo[0], Count = 0 });
                }

                return;
            }

            if (request.Path.StartsWithSegments("/HistoricalStats", out var remaining))
            {
                var result = await client.HistoricalStats(remaining.ToValue());

                await WriteJson(context, result.Value);

                return;
            }

            if (request.Path.StartsWithSegments("/SiloProperties", out var address1))
            {
                var result = await client.SiloProperties(address1.ToValue());

                await WriteJson(context, result.Value);

                return;
            }

            if (request.Path.StartsWithSegments("/SiloStats", out var address2))
            {
                var result = await client.SiloStats(address2.ToValue());

                await WriteJson(context, result.Value);

                return;
            }

            if (request.Path.StartsWithSegments("/SiloCounters", out var address3))
            {
                var result = await client.GetCounters(address3.ToValue());

                await WriteJson(context, result.Value);

                return;
            }

            if (request.Path.StartsWithSegments("/GrainStats", out var grainName1))
            {
                var result = await client.GrainStats(grainName1.ToValue());

                await WriteJson(context, result.Value);

                return;
            }

            if (request.Path == "/TopGrainMethods")
            {
                var result = await client.TopGrainMethods();

                await WriteJson(context, result.Value);

                return;
            }

            if (request.Path == "/Trace")
            {
                await TraceAsync(context);

                return;
            }

            await next(context);
        }

        private static async Task WriteJson(HttpContext context, object content)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/json";

            var json = JsonConvert.SerializeObject(content, Formatting.Indented, SerializerSettings);

            await context.Response.WriteAsync(json);
        }

        private static async Task WriteFileAsync(HttpContext context, string name, string contentType)
        {
            var assembly = typeof(DashboardMiddleware).GetTypeInfo().Assembly;

            context.Response.StatusCode = 200;
            context.Response.ContentType = contentType;

            var stream = OpenFile(name, assembly);

            using (stream)
            {
                await stream.CopyToAsync(context.Response.Body);
            }
        }

        private async Task WriteIndexFile(HttpContext context)
        {
            var assembly = typeof(DashboardMiddleware).GetTypeInfo().Assembly;

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";

            var stream = OpenFile("Index.html", assembly);

            using (stream)
            {
                var content = new StreamReader(stream).ReadToEnd();

                var basePath = string.IsNullOrWhiteSpace(this.options.Value.ScriptPath)
                    ? context.Request.PathBase.ToString()
                    : this.options.Value.ScriptPath;

                if (basePath != "/")
                {
                    basePath += "/";
                }

                content = content.Replace("{{BASE}}", basePath);
                content = content.Replace("{{HIDE_TRACE}}", options.Value.HideTrace.ToString().ToLowerInvariant());

                await context.Response.WriteAsync(content);
            }
        }

        private async Task TraceAsync(HttpContext context)
        {
            if (options.Value.HideTrace)
            {
                context.Response.StatusCode = 403;
                return;
            }

            var token = context.RequestAborted;

            using (var writer = new TraceWriter(logger, context))
            {
                await writer.WriteAsync(@"
   ____       _                        _____            _     _                         _
  / __ \     | |                      |  __ \          | |   | |                       | |
 | |  | |_ __| | ___  __ _ _ __  ___  | |  | | __ _ ___| |__ | |__   ___   __ _ _ __ __| |
 | |  | | '__| |/ _ \/ _` | '_ \/ __| | |  | |/ _` / __| '_ \| '_ \ / _ \ / _` | '__/ _` |
 | |__| | |  | |  __/ (_| | | | \__ \ | |__| | (_| \__ \ | | | |_) | (_) | (_| | | | (_| |
  \____/|_|  |_|\___|\__,_|_| |_|___/ |_____/ \__,_|___/_| |_|_.__/ \___/ \__,_|_|  \__,_|

You are connected to the Orleans Dashboard log streaming service
").ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromMinutes(60), token).ConfigureAwait(false);
                await writer.WriteAsync("Disconnecting after 60 minutes\r\n").ConfigureAwait(false);
            }
        }

        private static Stream OpenFile(string name, Assembly assembly)
        {
            var file = new FileInfo(name);

            return file.Exists
                ? file.OpenRead()
                : assembly.GetManifestResourceStream($"OrleansDashboard.{name}");
        }
    }
}