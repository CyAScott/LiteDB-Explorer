using System;
using LiteDB.Explorer.Core;

namespace LiteDB.Explorer.Windows
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            LiteDbApplication.Run(typeof(Program).Assembly);
        }
    }
}
