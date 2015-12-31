using System;
using System.Collections.Generic;

using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Azure
{
    /// <summary>
    /// Type responsible for parsing and bootstrapping command execution.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Microsoft.CLU.Run (clurun.exe) main entry point.
        /// </summary>
        /// <param name="args">The commandline arguments</param>
        public static int Main(string[] args)
        {
            Console.ReadLine();
            var app = new CommandLineApplication();

            var install = app.Command("install", (cmd) => { }, false, false);
            var update = app.Command("update", (cmd) => { }, false);
            var remove = app.Command("remove", (cmd) => { }, false);

            update.OnExecute(() =>
            {
                Console.WriteLine("Update executing...");
                return 47;
            });

            install.OnExecute(() =>
            {
                Console.WriteLine("Install executing...");
                return Install(args);
            });

            remove.OnExecute(() =>
            {
                Console.WriteLine("Remove executing...");
                return 1765;
            });



            app.OnExecute(() =>
            {
                Console.WriteLine("Dispatch executing...");
                return 0;
            });

            try
            {
                int i = app.Execute(args);
                Console.WriteLine(i);
                return i;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int Install(IEnumerable<string> args)
        {
            foreach (var pkg in args)
            {
                Console.WriteLine($"Should have installed package {pkg}");
            }

            return 0;
        }
    }
}
