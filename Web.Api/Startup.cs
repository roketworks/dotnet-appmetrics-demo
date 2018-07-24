using System;
using App.Metrics;
using App.Metrics.Extensions.Configuration;
using App.Metrics.Extensions.DependencyInjection;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Api
{
    public class Startup : IStartup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            Metrics = AppMetrics.CreateDefaultBuilder()
                .Configuration.ReadFrom(Configuration)
                .OutputMetrics.AsPrometheusPlainText()
                .OutputMetrics.AsPrometheusProtobuf()
                .Report.ToConsole(TimeSpan.FromSeconds(2))
                .Build();
        }

        public IMetricsRoot Metrics { get; }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddMetrics();
           
            services.AddMetrics(Metrics);
            services.AddMetricsReportingHostedService();
            services.AddMetricsEndpoints(Configuration);
            services.AddMetricsTrackingMiddleware(Configuration); 
            
            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMetricsAllMiddleware();
            app.UseMetricsEndpoint(Metrics.OutputMetricsFormatters.GetType<MetricsPrometheusProtobufOutputFormatter>());
            app.UseMetricsTextEndpoint(Metrics.OutputMetricsFormatters.GetType<MetricsPrometheusTextOutputFormatter>());

            app.UseMvc();
        }
    }
}