﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Implementation;
using GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Implementation.Details;
using GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Metrics;
using GranDen.Orleans.NetCoreGenericHost.OrleansDashboard.Metrics.Details;
using Orleans;

namespace GranDen.Orleans.NetCoreGenericHost.OrleansDashboard
{
    public static class ServiceCollectionExtensions
    {
        public static ISiloHostBuilder UseDashboard(this ISiloHostBuilder builder,
            Action<DashboardOptions> configurator = null)
        {
            builder.ConfigureApplicationParts(parts => parts.AddDashboardParts());
            builder.ConfigureServices(services => services.AddDashboard(configurator));
            builder.AddStartupTask<Dashboard>();

            return builder;
        }

        public static ISiloBuilder UseDashboard(this ISiloBuilder builder,
            Action<DashboardOptions> configurator = null)
        {
            builder.ConfigureApplicationParts(parts => parts.AddDashboardParts());
            builder.ConfigureServices(services => services.AddDashboard(configurator));
            builder.AddStartupTask<Dashboard>();

            return builder;
        }

        public static IServiceCollection AddDashboard(this IServiceCollection services,
            Action<DashboardOptions> configurator = null)
        {
            services.Configure(configurator ?? (x => { }));
            services.Configure<TelemetryOptions>(options => options.AddConsumer<DashboardTelemetryConsumer>());

            services.AddSingleton<SiloStatusOracleSiloDetailsProvider>();
            services.AddSingleton<MembershipTableSiloDetailsProvider>();
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton<IGrainProfiler, GrainProfiler>();
            services.AddSingleton(c => (ILifecycleParticipant<ISiloLifecycle>)c.GetRequiredService<IGrainProfiler>());
            services.AddSingleton<IIncomingGrainCallFilter, GrainProfilerFilter>();

            services.AddSingleton<ISiloDetailsProvider>(c =>
            {
                var membershipTable = c.GetService<IMembershipTable>();

                if (membershipTable != null)
                {
                    return c.GetRequiredService<MembershipTableSiloDetailsProvider>();
                }

                return c.GetRequiredService<SiloStatusOracleSiloDetailsProvider>();
            });

            services.TryAddSingleton(GrainProfilerFilter.NoopOldGrainMethodFormatter);
            services.TryAddSingleton(GrainProfilerFilter.DefaultGrainMethodFormatter);

            return services;
        }

        public static IClientBuilder UseDashboard(this IClientBuilder builder)
        {
            builder.ConfigureApplicationParts(parts => parts.AddDashboardParts());

            return builder;
        }

        private static void AddDashboardParts(this IApplicationPartManager appParts)
        {
            appParts.AddFrameworkPart(typeof(Dashboard).Assembly).WithReferences();
        }

        public static IApplicationBuilder UseOrleansDashboard(this IApplicationBuilder app, DashboardOptions options = null)
        {
            if (string.IsNullOrEmpty(options?.BasePath) || options.BasePath == "/")
            {
                app.UseMiddleware<DashboardMiddleware>();
            }
            else
            {
                // Make sure there is a leading slash
                var basePath = options.BasePath.StartsWith("/") ? options.BasePath : $"/{options.BasePath}";

                app.Map(basePath, a => a.UseMiddleware<DashboardMiddleware>());
            }

            return app;
        }

        public static IServiceCollection AddServicesForSelfHostedDashboard(this IServiceCollection services, IClusterClient client = null,
            Action<DashboardOptions> configurator = null)
        {
            if (client != null)
            {
                services.AddSingleton(client);
                services.AddSingleton<IGrainFactory>(c => c.GetRequiredService<IClusterClient>());
            }

            services.Configure(configurator ?? (x => { }));
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);

            return services;
        }

        internal static IServiceCollection AddServicesForHostedDashboard(this IServiceCollection services, IGrainFactory grainFactory, DashboardOptions options)
        {
            services.AddSingleton(DashboardLogger.Instance);
            services.AddSingleton(Options.Create(options));
            services.AddSingleton<ILoggerProvider>(DashboardLogger.Instance);
            services.AddSingleton(grainFactory);

            return services;
        }
    }
}
