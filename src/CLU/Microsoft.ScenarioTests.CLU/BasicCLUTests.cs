using Microsoft.Azure.Commands.Common.ScenarioTest;
using Microsoft.Azure.Commands.ScenarioTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Commands.Resources.Test
{
    public class BasicCLUTests
    {
        private const string bashExe = "bash.exe";
        private const string runtime = "win7-x64";
        private readonly string testLocation;
        private readonly string dropLocation;

        public BasicCLUTests()
        {
            //debug
            Environment.SetEnvironmentVariable("Path", 
                $@"{Environment.GetEnvironmentVariable("Path")};{Environment.GetEnvironmentVariable("ProgramW6432")}\Git\bin");
            Environment.SetEnvironmentVariable("PackagesRootPath", @"C:\johanste-azure-powershell\drop\clurun\win7-x64\pkgs");

            if (!IsInPath(bashExe))
            {
                throw new ArgumentException($"Couldn't find {bashExe} in PATH");
            }

            string pkgRoot = Environment.GetEnvironmentVariable("PackagesRootPath");

            if(string.IsNullOrEmpty(pkgRoot))
            {
                throw new ArgumentException("Environment variable PackagesRootPath not set, please set to the pkgs location");
            }

            testLocation = $@"{pkgRoot.Replace("C:", "/C")}\..\..\..\..\src\CLU\Microsoft.ScenarioTests.CLU\Tests\BasicCLUTests.sh";
            dropLocation = $@"{pkgRoot}\..";
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void BasicCLUCommandTest()
        {
            ExecuteProcess(bashExe, $"{testLocation} {dropLocation} {runtime}");
        }

        private static void ExecuteProcess(string fileName, string arguments)
        {
            Environment.SetEnvironmentVariable("Path", $"{Environment.GetEnvironmentVariable("Path")};{Path.GetDirectoryName(fileName)}");
            ProcessHelper helper = new ProcessHelper(Directory.GetDirectoryRoot(fileName), fileName, arguments.Split(' '));
            Assert.Equal(0, helper.StartAndWaitForExit());
        }

        private bool IsInPath(string fileName)
        {
            return (Environment.GetEnvironmentVariable("PATH")
                .Split(';')
                .Select(p => Path.Combine(p, fileName))
                .Any(p => File.Exists(p)));
        }
    }
}
