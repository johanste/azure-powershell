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
        private string bashExe;
        private string testLocation;
        private string dropLocation;
        private string runtime;

        public BasicCLUTests()
        {
            //debug
            //Environment.SetEnvironmentVariable("PackagesRootPath", @"C:\johanste-azure-powershell\drop\clurun\win7-x64\pkgs");

            string pkgRoot = Environment.GetEnvironmentVariable("PackagesRootPath");
            Assert.NotEmpty(pkgRoot);

            bashExe = $@"{Environment.GetEnvironmentVariable("ProgramW6432")}\Git\bin\bash.exe";
            testLocation = $@"{pkgRoot.Replace("C:", "/C")}\..\..\..\..\src\CLU\Microsoft.ScenarioTests.CLU\Tests\BasicCLUTests.sh";
            dropLocation = $@"{pkgRoot}\..";
            runtime = "win7-x64";
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
    }
}
