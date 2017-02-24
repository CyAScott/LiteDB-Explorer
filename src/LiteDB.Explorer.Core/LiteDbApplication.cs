using System;
using System.Reflection;
using Eto.Forms;
using LiteDB.Explorer.Core.Forms;
using NLog;
using SimpleInjector;
using Container = SimpleInjector.Container;

namespace LiteDB.Explorer.Core
{
    /// <summary>
    /// The class used to start the application.
    /// </summary>
    public static class LiteDbApplication
    {
        private static ILogger setupLogging()
        {
            return LogManager.GetLogger(nameof(LiteDbApplication));
        }
        private static Container setupIocContainer(Assembly assembly)
        {
            var container = new Container();

            container.Register<ILogger>(() => LogManager.GetLogger(nameof(LiteDbApplication)), Lifestyle.Singleton);
            container.RegisterPackages(new[] { typeof(LiteDbApplication).Assembly });
            container.RegisterPackages(new[] { assembly });
            
            container.Verify(VerificationOption.VerifyAndDiagnose);

            return container;
        }
        private static void run(Assembly assembly)
        {
            var log = setupLogging();

            log.Info("Loading Program");

            using (var application = new Application())
            {
                log.Info("Loading Ioc Container");

                using (var container = setupIocContainer(assembly))
                {
                    log.Info("Program is Ready");

                    application.MainForm = container.GetInstance<MainForm>();

                    application.Run();
                }
            }

        }

        /// <summary>
        /// Starts the application and blocks the thread until the program exits.
        /// </summary>
        public static void Run(Assembly assembly)
        {
            try
            {
                run(assembly);
            }
            catch (Exception error)
            {
                MessageBox.Show("Failed to load Lite DB Explorer: " + Environment.NewLine + error.Message,
                    "Load Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxType.Error);
            }
        }
    }
}
