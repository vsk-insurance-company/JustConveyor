using System;
using JustConveyor.Contracts.Settings;
using Microsoft.Owin.Hosting;

namespace JustConveyor.Web
{
    public static class MetricsWebService
    {
        private static IDisposable mMetricsService;

        public static void StartService(ServiceSettings settings)
        {
            mMetricsService = WebApp.Start<MetricsServiceStartup>(url: settings.BaseAddress);
        }

        public static void StopServive()
        {
            mMetricsService.Dispose();
        }
    }
}