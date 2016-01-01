using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Framework.Runtime.Common.CommandLine;

namespace Azure
{
    /// <summary>
    /// Type responsible for parsing and bootstrapping command execution.
    /// </summary>
    public class Program
    {
        public static int Main(string[] args)
        {
            Console.ReadLine();


            var tc = System.Environment.TickCount;
            string pkgRoot = @"C:\repos\azure-ps-bugfix\azure-powershell\drop\clurun\win7-x64\pkgs";
            
            //foreach (var cmd in FindCommands(pkgRoot, args))
            //{
            //    Console.WriteLine($"Match: {cmd}");
            //}


            //foreach (var cmd in CompleteCommands(pkgRoot, args))
            //{
            //    Console.WriteLine($"Completion: {cmd}");
            //}

            var hashSet = new HashSet<string>();
            foreach (var cmd in CompleteCommands(pkgRoot, args))
            {
                var splitCmd = cmd.Split(';');

                System.Diagnostics.Debug.Assert(splitCmd.Length >= args.Length);

                var nextWord = args.Length == 0 || String.Equals(args[args.Length - 1], splitCmd[args.Length - 1]) ? splitCmd[args.Length] : splitCmd[args.Length - 1];
                hashSet.Add(nextWord);
            }
            Console.WriteLine($"Next word: {String.Join(" ", hashSet)}");

            Console.WriteLine($"All this took {Environment.TickCount - tc}ms...");
            return 0;
        }

        public static IEnumerable<string> FindCommands(string pkgRoot, string[] args)
        {
            var semiColonSeparatedArgs = String.Join(";", args) + ";";

            Func<string, bool> matcher = (cmd) =>
            {
                return Program.MatchScore(cmd, semiColonSeparatedArgs) >= cmd.Length;
            };

            return FindMatches(pkgRoot, args, matcher);
        }

        public static IEnumerable<string> CompleteCommands(string pkgRoot, string[] args)
        {
            var semiColonSeparatedArgs = String.Join(";", args);

            Func<string, bool> matcher = (cmd) =>
            {
                return Program.MatchScore(cmd, semiColonSeparatedArgs) >= semiColonSeparatedArgs.Length;
            };

            return FindMatches(pkgRoot, args, matcher);
        }

        public static IEnumerable<string> FindMatches(string pkgRoot, string[] args, Func<string, bool> matchFunc)
        {
            foreach (var commandIndex in GetCommandIndexes(pkgRoot).SelectMany((s) => { return s; }))
            {
                var semiColonSeparatedCommand = commandIndex.Split(':')[0] + ";";
                if (matchFunc(semiColonSeparatedCommand))
                {
                    yield return semiColonSeparatedCommand;
                }
            }
        }


        public static IEnumerable<string[]> GetCommandIndexes(string pkgsRootPath)
        {
            foreach (var location in GetIndexLocations(pkgsRootPath))
            {
                yield return System.IO.File.ReadAllLines(location);
            }
        }

        public static IEnumerable<string> GetIndexLocations(string pkgsRootPath)
        {
            var rootInfo = new System.IO.DirectoryInfo(pkgsRootPath);

            foreach (var pkgPath in rootInfo.EnumerateDirectories())
            {
                foreach (var versionPath in pkgPath.EnumerateDirectories())
                {
                    var cmdletIndexPath = Path.Combine(versionPath.FullName, "_indexes", "_cmdlets.idx");
                    if (File.Exists(cmdletIndexPath))
                    {
                        yield return cmdletIndexPath;
                    }
                }
            }

        }

        public static int MatchScore(string semiColonSeparatedArgs, string semiColonSeparatedCommand)
        {
            int score = 0;
            for (int charPos = 0; charPos < Math.Min(semiColonSeparatedArgs.Length, semiColonSeparatedCommand.Length); ++charPos)
            {
                if (semiColonSeparatedArgs[charPos] != semiColonSeparatedCommand[charPos])
                {
                    break;
                }
                else
                {
                    score = charPos + 1;
                }
            }

            return score;
        }
    }
}
