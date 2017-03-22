using System;
using LiteDB.Explorer.Core;

namespace LiteDB.Explorer.Windows
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            LiteDbApplication.Run(new SetupArgs
            {
                Assembly = typeof(Program).Assembly,
                OpenFiles = args
            });
        }
    }
}
