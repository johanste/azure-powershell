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
        private string pkgRoot = Environment.GetEnvironmentVariable("PackagesRootPath");
        private string packageExe;

        public BasicCLUTests()
        {
            CleanUp();

            ExecuteProcess("powershell", $@" -ExecutionPolicy Bypass -Command 
                ""& {pkgRoot}\..\..\..\..\tools\CLU\BuildDrop.ps1 -commandPackagesToBuild Microsoft.ScenarioTests.CLU -excludeCluRun""");

            ExecuteProcess($@"{pkgRoot}\..\clurun.exe", "--install Microsoft.ScenarioTests.CLU.win7-x64");

            packageExe = $@"{pkgRoot}\Microsoft.ScenarioTests.CLU.win7-x64\0.0.1\lib\dnxcore50\Microsoft.ScenarioTests.CLU.exe";
            ExecuteProcess(packageExe, "--buildIndex");
        }

        ~BasicCLUTests()
        {
            CleanUp();
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void DirectExeNamedCommand()
        {
            ExecuteProcess(packageExe, "returncode show");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void DirectExeDefaultNamedCommandWithArgs()
        {
            ExecuteProcess(packageExe, "progress show --Steps 5");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void DispatcherNamedCommand()
        {
            ExecuteProcess($@"{pkgRoot}\..\az.cmd", "returncode show");
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void DispatcherDefaultNamedCommandWithArgs()
        {
            ExecuteProcess($@"{pkgRoot}\..\az.cmd", "progress show --Steps 5");
        }

        private static void ExecuteProcess(string fileName, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = fileName,
                Arguments = arguments
            };
            Process process = Process.Start(startInfo);
            process.WaitForExit();
            Assert.Equal(0, process.ExitCode);
        }

        private void CleanUp()
        {
            string dir = $@"{pkgRoot}\Microsoft.ScenarioTests.CLU.win7-x64";
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
