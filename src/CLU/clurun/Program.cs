using Microsoft.CLU.Common.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace clurun
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
            var debugClu = Environment.GetEnvironmentVariable("DebugCLU");
            if (!String.IsNullOrEmpty(debugClu))
            {
                Console.WriteLine("This is your chance to attach a debugger...");
                Console.ReadLine();
            }

            if (args.Contains("--install"))
            {
                return (int)Microsoft.CLU.Run.CLURun.Execute(args);
            }

            string argsString = string.Join(" ", args);
            string[] indexFiles = Directory.GetFiles(GetExeDirectory(), "*.idx", SearchOption.AllDirectories);

            var commands = indexFiles
                .SelectMany(f => File.ReadAllLines(f))
                .Select(f => new CommandIndex(f));

            var command = commands
                .OrderByDescending(c => 
                    CommandMatcher.GetMatchScore(c.Args, argsString))
                    .FirstOrDefault(c => argsString.Contains(c.Args));

            if (command == null)
            {
                ShowHelp(argsString, commands);
                return -1;
            }

            return Execute(command, args);
        }

        private static string GetExeDirectory()
        {
            var assemblyLocation = typeof(Program).GetTypeInfo().Assembly.Location;
            return Path.GetDirectoryName(assemblyLocation);
        }

        private static int Execute(CommandIndex command, string[] args)
        {
            string extension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
            string executablePath = Directory.GetFiles(GetExeDirectory(), 
                command.Package + extension, SearchOption.AllDirectories).FirstOrDefault();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = executablePath;
            startInfo.Arguments = string.Join(" ", args);

            Process process = Process.Start(startInfo);
            process.WaitForExit();
            return process.ExitCode;
        }

        private static void ShowHelp(string argsString, IEnumerable<CommandIndex> commands)
        {
            bool found = false;
            foreach (var c in commands.Where(c => c.Args.StartsWith(argsString)))
            {
                found = true;
                Console.WriteLine(c.Args);
            }

            if (!found)
            {
                Console.WriteLine("Couldn't find any command starting with " + argsString);
            }
        }
    }
}
