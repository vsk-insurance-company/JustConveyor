using System;
using System.Collections.Generic;
using System.IO;
using CurrenciesFetcher.Settings;
using JustConveyor;
using JustConveyor.Contracts.Dependencies;
using JustConveyor.Contracts.Utils;
using JustConveyor.Injection;
using Newtonsoft.Json.Linq;
using NLog;

namespace CurrenciesFetcher
{
    class Program
    {
        private static Finalizer mFinalizer;
        private static Logger mLogger;
        private static double mMaxValue;

        static void Main()
        {
            mLogger = LogManager.GetCurrentClassLogger();

            BootstrapIoCContainer();
            RunConveyor();
        }

        private static void RunConveyor()
        {
            var supplier = new JsonFileDiapasonSupplier("./ratesdiapason.json");
            Conveyor.Init(mLogger)
                .ScanForBlueprints()
                .WithSupplier("CurrenciesDiapasones", supplier)
                .Start();

            mFinalizer.GetWaitTask().Wait();
            mLogger.Info(
                $"Max currency rate for dates from {supplier.Diapason.From} to {supplier.Diapason.To}: {mMaxValue}");
        }

        private static void BootstrapIoCContainer()
        {
            var container = new IoCContainer();
            container.SetLogger(mLogger);
            Injection.RegisterInjectionProvider(container);

            mFinalizer = new Finalizer((package,ctx) =>
            {
                mMaxValue = ctx.FinalUnitContext.GetUnit<double>();
                return true;
            });
            container.RegisterSingle(mFinalizer);

            var servicesSettings = new ServicesSettingsManager(
                JObject.Parse(File.ReadAllText("./settings.json"))["services"]
                    .ToObject<Dictionary<string, ServiceSettings>>());
            container.RegisterSingle(servicesSettings);
        }
    }
}