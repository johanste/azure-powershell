using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.CLU.Common;

namespace Microsoft.CLU.CommandPackage
{
    public class Configuration 
    {
        string NounPrefix { get; set;  }
        bool NounFirst { get; set; }

    }

    /// <summary>
    /// Type responsible for parsing and bootstrapping command execution.
    /// </summary>
    public class CommandPackage
    {
        public static int Handle(PackageConfig config, DirectoryInfo baseDirectory, string[] args)
        {
            var debugClu = Environment.GetEnvironmentVariable("DebugCLU");
            if (!String.IsNullOrEmpty(debugClu))
            {
                Console.WriteLine("This is your chance to attach a debugger...");
                Console.ReadLine();
            }

            List<string> freeForAllArguments;
            List<string> escapedArguments;

            SplitArguments(args, out freeForAllArguments, out escapedArguments);

            Func<PackageConfig, DirectoryInfo, string[], int> command;
            if (freeForAllArguments.Contains("--buildIndex"))
            {
                command = (cfg, dir, remainingArgs) =>
                {
                    BuildIndex(cfg, dir);
                    return 0;
                };
            }
            else
            {
                command = (cfg, dir, remainingArgs) =>
                {
                    return ExecuteCommand(cfg, dir, remainingArgs);
                };
            }

            return command(config, baseDirectory, freeForAllArguments.Union(escapedArguments).ToArray());
        }

        public static int ExecuteCommand(PackageConfig config, DirectoryInfo baseDirectory, string[] args)
        {
            CLUEnvironment.Console = new ConsoleInputOutput(args);

            Stopwatch sw = Stopwatch.StartNew();
            var commandModel = new Microsoft.CLU.CommandModel.CmdletCommandModel();
            var package = new Common.CmdletLocalPackage(config, baseDirectory);

            int result = (int)commandModel.Run(config.NounPrefix, new Microsoft.CLU.CommandLineParser.UnixCommandLineParser(), package, args);
            CLUEnvironment.Console.WriteVerboseLine($"The command executed in {sw.ElapsedMilliseconds} ms");

            return result;
        }

        public static void BuildIndex(PackageConfig config, DirectoryInfo baseDirectory)
        {
            var package = new Common.CmdletLocalPackage(config, baseDirectory);

            IndexBuilder.CreateIndexes(package);
        }


        /// <summary>
        /// To avoid clashes between argumens that I can handle and arguments that a cmdlet may want to handle - 
        /// for example --buildIndex (however unlikely), the caller can pass in a double dash (--) and any argumens
        /// following the double-dash is off-limits for us to handle here...
        /// 
        /// The freeForAllArguments will contain all parameters up to (but not including) the double dash and the ecaped
        /// arguments contains all arguments following the double dash (which may be an empty list if there is no double dash)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void SplitArguments(string[] args, out List<string> freeForAllArguments, out List<string> escapedArguments)
        {
            var completeArgList = args.ToList();

            var doubleDashPosition = completeArgList.FindIndex((s) => s == "--");
            if (doubleDashPosition != -1)
            {
                freeForAllArguments = completeArgList.Take(doubleDashPosition).ToList();
                escapedArguments = completeArgList.Skip(doubleDashPosition + 2).ToList();
            }
            else
            {
                freeForAllArguments = completeArgList;
                escapedArguments = new List<string>();
            }
        }
    }
}
