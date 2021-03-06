﻿using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private static Container setupIocContainer(SetupArgs args)
        {
            var container = new Container();

            container.Register<ILogger>(() => LogManager.GetLogger(nameof(LiteDbApplication)), Lifestyle.Singleton);
            container.Register(() => args, Lifestyle.Singleton);
            container.RegisterPackages(new[] { typeof(LiteDbApplication).Assembly });
            container.RegisterPackages(new[] { args.Assembly });
            
            container.Verify(VerificationOption.VerifyAndDiagnose);

            return container;
        }
        private static void run(SetupArgs args)
        {
            var log = setupLogging();

            log.Info("Loading Program");

            using (var application = args.Application = new Application())
            {
                log.Info("Loading Ioc Container");
                
                using (var container = args.IocContainer = setupIocContainer(args))
                {
                    log.Info("Program is Ready");

                    application.MainForm = container.GetInstance<MainForm>();
                    
                    args.BeforeRunning?.Invoke(args);

                    if (args.OpenFiles?.Length > 0)
                    {
                        var viewModel = args.IocContainer.GetInstance<MainFormModel>();

                        foreach (var connectionString in args.OpenFiles)
                        {
                            try
                            {
                                viewModel.Open(new LiteDatabase(connectionString), connectionString.IndexOf(';') == -1 ?
                                    connectionString : 
                                    Regex.Match(connectionString, @"Filename\s*=.*[;$]", RegexOptions.IgnoreCase).ToString().Substring(10).TrimEnd(';'));
                            }
                            catch (Exception error)
                            {
                                MessageBox.Show(error.Message,
                                    "Open File Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxType.Error);
                            }
                        }
                    }

                    application.Run();
                }
            }

        }

        /// <summary>
        /// Starts the application and blocks the thread until the program exits.
        /// </summary>
        public static void Run(SetupArgs args)
        {
            try
            {
                run(args);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message,
                    "Application Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxType.Error);
            }
        }
    }
    /// <summary>
    /// The arguments for setting up the UI for the platform.
    /// </summary>
    public class SetupArgs
    {
        /// <summary>
        /// The ETO application.
        /// </summary>
        public Application Application { get; internal set; }

        /// <summary>
        /// The assembly for a platform build.
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// The IOC container.
        /// </summary>
        public Container IocContainer { get; internal set; }

        /// <summary>
        /// This method is invoked before the application runs.
        /// </summary>
        public Action<SetupArgs> BeforeRunning { get; set; } 

        /// <summary>
        /// The main form for the app.
        /// </summary>
        public MainForm MainForm => Application?.MainForm as MainForm;

        /// <summary>
        /// Files to open on load.
        /// </summary>
        public string[] OpenFiles { get; set; }
    }
}
