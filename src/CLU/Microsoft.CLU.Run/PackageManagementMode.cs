using CLU.Packages;

using Microsoft.CLU.Common;
using Microsoft.CLU.Common.Properties;

using NuGet.Versioning;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using static Microsoft.CLU.CLUEnvironment;

namespace Microsoft.CLU.Run
{
    /// <summary>
    /// The "IRunMode" implementation for package management.
    /// </summary>
    internal class PackageManagementMode : IRunMode
    {
        /// <summary>
        /// Check if this IRunMode implementation for package management can handle the arguments.
        /// </summary>
        /// <param name="arguments">The argument to inspect to see implementation can handle it</param>
        /// <returns>True, if arguments can be handled, False otherwise</returns>
        public bool CanHandle(string[] arguments)
        {
            return _options.ContainsKey(arguments[0]);
        }

        /// <summary>
        /// Run a command that is identified by the arguments and supported by this
        /// IRunMode implementation for package managment.
        /// </summary>
        /// <param name="arguments">The arguments</param>
        public int Run(string[] arguments)
        {
            _packagesRootPath = CLUEnvironment.GetPackagesRootPath();
            try
            {
                _runtimeConfiguration = CLUEnvironment.RuntimeConfig;
            }
            catch (TargetInvocationException tie)
            {
                CLUEnvironment.Console.WriteErrorLine(tie.InnerException.Message);
                CLUEnvironment.Console.WriteDebugLine($"{tie.InnerException.GetType().FullName}\n{tie.InnerException.StackTrace}");
                return 4; //  Microsoft.CLU.CommandModelErrorCode.InternalFailure;
            }
            catch (Exception exc)
            {
                CLUEnvironment.Console.WriteErrorLine(exc.Message);
                CLUEnvironment.Console.WriteDebugLine($"{exc.GetType().FullName}\n{exc.StackTrace}");
                return 4; // Microsoft.CLU.CommandModelErrorCode.InternalFailure;
            }

            try
            {
                _repository = new PackageRepository(_runtimeConfiguration.RepositoryPath);
                _manager = new PackageManager(_repository, _packagesRootPath);
                _manager.PackageInstalled += PackageInstalled;
                _manager.PackageUninstalled += PackageUninstalled;

                try
                {
                    if (arguments.Length > 0)
                    {
                        int argsBase = 0;

                        string version = null;

                        switch (arguments[argsBase])
                        {
                            case "--version":
                            case "-v":
                                if (argsBase + 1 >= arguments.Length ||
                                    arguments[argsBase + 1].StartsWith("-", StringComparison.Ordinal))
                                {
                                    CLUEnvironment.Console.WriteLine(Strings.PackageManagementMode_Run_VersionIdMissing);
                                    return 3; // Microsoft.CLU.CommandModelErrorCode.MissingParameters;
                                }
                                version = arguments[argsBase + 1];
                                argsBase += 2;
                                break;
                        }

                        int selectedOption = _options[arguments[argsBase]];
                        bool packageNamesFound = arguments.Length > argsBase + 1;

                        switch (selectedOption)
                        {
                            case 0:
                                if (packageNamesFound)
                                {
                                    for (int i = argsBase + 1; i < arguments.Length; ++i)
                                    {
                                        Install(arguments[i], version);
                                    }
                                }
                                else
                                {
                                    _runtimeConfiguration.RuntimeVersion = version;
                                    Install(_runtimeConfiguration.RuntimePackage, _runtimeConfiguration.RuntimeVersion);
                                }
                                break;
                            case 1:
                                if (packageNamesFound)
                                {
                                    for (int i = argsBase + 1; i < arguments.Length; ++i)
                                    {
                                        Remove(arguments[i], version);
                                    }
                                }
                                else
                                {
                                    _runtimeConfiguration.RuntimeVersion = version;
                                    Remove(_runtimeConfiguration.RuntimePackage, _runtimeConfiguration.RuntimeVersion);
                                }
                                break;
                            case 2:
                                if (packageNamesFound)
                                {
                                    for (int i = argsBase + 1; i < arguments.Length; ++i)
                                    {
                                        Upgrade(arguments[i], version);
                                    }
                                }
                                else
                                {
                                    _runtimeConfiguration.RuntimeVersion = version;
                                    Upgrade(_runtimeConfiguration.RuntimePackage, _runtimeConfiguration.RuntimeVersion);
                                }
                                break;
                        }
                    }
                }
                finally
                {
                    _manager.PackageInstalled -= PackageInstalled;
                    _manager.PackageUninstalled -= PackageUninstalled;
                }
            }
            catch (TargetInvocationException tie)
            {
                CLUEnvironment.Console.WriteErrorLine(tie.InnerException.Message);
                CLUEnvironment.Console.WriteDebugLine($"{tie.InnerException.GetType().FullName}\n{tie.InnerException.StackTrace}");
                return 4; // Microsoft.CLU.CommandModelErrorCode.InternalFailure;
            }
            catch (Exception exc)
            {
                CLUEnvironment.Console.WriteErrorLine(exc.Message);
                CLUEnvironment.Console.WriteDebugLine($"{exc.GetType().FullName}\n{exc.StackTrace}");
                return 4; //  Microsoft.CLU.CommandModelErrorCode.InternalFailure;
            }

            return 0; // CommandModelErrorCode.Success;
        }

        /// <summary>
        /// Nuget.PackageManager.PackageUninstalled event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageUninstalled(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package.GetFullName().Split(' ');
            var packageName = package[0];

            CLUEnvironment.Console.WriteLine(string.Format(Strings.PackageManagementMode_PackageUninstalled_PackageRemoved, packageName));
        }

        /// <summary>
        /// Nuget.PackageManager.PackageInstalled event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageInstalled(object sender, PackageOperationEventArgs e)
        {
            PerformInstallActions(e.Package);
        }

        /// <summary>
        /// Install given version of a package.
        /// </summary>
        /// <param name="packageName">The package name</param>
        /// <param name="version">The package version</param>
        /// <param name="caption">The message to show during installation</param>
        private void Install(string packageName, string version, string caption = "Installing '{0}'")
        {
            var installedVersions = _manager.GetInstalledPackageVersions(packageName);
            if (installedVersions.Any())
            {

                if (installedVersions.Contains(version))
                {
                    CLUEnvironment.Console.WriteLine(string.Format(Strings.PackageManagementMode_Install_PackageVersionAlreadyInstalled, packageName));
                    return;
                }

                RemoveExistingPackage(packageName, version ?? installedVersions.First());
            }

            IPackage package = (version == null) ?
                _repository.FindPackage(packageName) :
                _repository.FindPackage(packageName, SemanticVersion.Parse(version));

            if (package == null)
            {
                CLUEnvironment.Console.WriteErrorLine(string.Format(Strings.PackageManagementMode_Install_PackageVersionNotAvailable, packageName));
                return;
            }

            CLUEnvironment.Console.WriteLine(caption, packageName);

            _manager.InstallPackage(package, false, false);

            BuildIndexes(this._packagesRootPath, version, packageName);
        }

        private void BuildIndexes(string packagesPath, string version, string packageName)
        {
            // When exe names don't match their package name, there is no way to find the exe to build indexes
            // (since executables don't have extenstion on non-Windows).
            // It is recommended that the package and executable name match.  If they do not, you must include a
            // BuildIndex.cmd and BuildIndex.sh file so the package can be installed successfully on each platform.
            const string buildPackageCommandName = "BuildIndex";

            // Only build indexes if the package matches the current runtime
            string currentRuntime = Platform.GetCurrentRuntime();
            if (!packageName.Contains(currentRuntime))
            {
                return;
            }

            string basePackageName = string.Join(".", packageName.Split('.').Where(s => s != currentRuntime));
            string executableDir = Path.Combine(
                packagesPath, 
                packageName,
                version == null ? Constants.DefaultRuntimeVersion : version,
                Constants.LibFolder,
                Constants.DNXCORE50);
            string exePath = Path.Combine(executableDir, basePackageName + Platform.ExecutableExtension);
            string scriptPath = Path.Combine(executableDir, buildPackageCommandName + Platform.ScriptExtension);
            string executablePath = File.Exists(exePath) ? exePath : scriptPath;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = executablePath;
            startInfo.Arguments = Constants.BuildIndexToken;

            Process process = Process.Start(startInfo);
            process.WaitForExit();
            if(process.ExitCode != 0)
            {
                CLUEnvironment.Console.WriteErrorLine($"BuildIndex failed with code {process.ExitCode}");
            }
        }

        /// <summary>
        /// Upgrade an existing package to new version.
        /// </summary>
        /// <param name="packageName">The package name</param>
        /// <param name="version">The package version</param>
        private void Upgrade(string packageName, string version)
        {
            if (!_manager.GetInstalledPackageVersions(packageName).Any())
            {
                CLUEnvironment.Console.WriteWarningLine(string.Format(Strings.PackageManagementMode_Upgrade_NothingToUpdate, packageName));
                return;
            }
            IPackage package = (version == null) ?
                _repository.FindPackage(packageName) :
                _repository.FindPackage(packageName, SemanticVersion.Parse(version));

            if (package == null)
            {
                CLUEnvironment.Console.WriteWarningLine(string.Format(Strings.PackageManagmentMode_Upgrade_PackageOrVersionNotAvailable, packageName));
                return;
            }

            var currentVersion = SemanticVersion.Parse(version);
            if (package.Version != currentVersion)
            {
                RemoveExistingPackage(packageName, version);
                Install(packageName, version, string.Format(Strings.PackageManagmentMode_Upgrade_UpdatingToVersion, package.Version));
            }
            else
            {
                CLUEnvironment.Console.WriteWarningLine(string.Format(Strings.PackageManagmentMode_Upgrade_AlreadyUpToDate, packageName));
            }
        }

        /// <summary>
        /// Remove specific version of a package.
        /// </summary>
        /// <param name="packageName">The package name</param>
        /// <param name="version">The package version</param>
        private void Remove(string packageName, string version)
        {
            if (!_manager.GetInstalledPackageVersions(packageName).Any())
            {
                CLUEnvironment.Console.WriteWarningLine(string.Format(Strings.PackageManagmentMode_Remove_NothingToRemove, packageName));
                return;
            }
            
            RemoveExistingPackage(packageName, version);
        }

        /// <summary>
        /// Removes an installed package identified by the given LocalPackage reference.
        /// </summary>
        /// <param name="existingPackage">Reference to locally installed package</param>
        private void RemoveExistingPackage(string name, string version)
        {
            var currentVersion = NuGet.Versioning.SemanticVersion.Parse(version);
            _manager.UninstallPackage(name, currentVersion, true, true);
        }

        /// <summary>
        /// Perform post installation actions for a given LocalPackage reference.
        /// </summary>
        /// <param name="localPackage">Reference to locally installed package</param>
        private void PerformInstallActions(Package package)
        {
            // TODO: Generate indexes. Or not. 
            // IndexBuilder.CreateIndexes(localPackage);
        }

        private static void GenerateScript(string cfgPath)
        {
            var scriptPath = GetScriptPath(cfgPath);
            var scriptName = Path.GetFileNameWithoutExtension(scriptPath);
            var ScriptBaseName = "az";

            if (File.Exists(scriptPath))
                return;

            if (CLUEnvironment.Platform.IsUnixOrMacOSX)
            {
                File.WriteAllLines(scriptPath, new string[]
                {
                    "#!/bin/bash",
                    "SCRIPTPATH=$(dirname \"$0\")",
                    $"$SCRIPTPATH/clurun -s {ScriptBaseName} -r $SCRIPTPATH/{Path.GetFileName(cfgPath)} \"$@\""
                });
                System.Diagnostics.Process.Start("chmod", $"777 {scriptPath}");
            }
            else
            {
                File.WriteAllLines(scriptPath, new string[]
                {
                    "@echo off",
                    $@"%~dp0\clurun.exe -s {ScriptBaseName} -r %~dp0\{Path.GetFileName(cfgPath)} %*"
                });
            }
        }

        private static string GetScriptPath(string configPath)
        {
            var scriptFile = Path.GetFileNameWithoutExtension(configPath) + CLUEnvironment.Platform.ScriptFileExtension;
            return Path.Combine(Path.GetDirectoryName(configPath), scriptFile);
        }

        /// <summary>
        /// Extracts the module names from the given line.
        /// </summary>
        /// <param name="line">The line</param>
        /// <returns>Set of module names</returns>
        private static HashSet<string> GetModules(string line)
        {
            var items = line.Split(':');
            if (items.Length < 2)
                return new HashSet<string>();

            return items[1].Trim().Split(',').Select(l => l.Trim()).ToSet();
        }

#region Private fields.

        /// <summary>
        /// The options those identify package management mode.
        /// </summary>
        private IDictionary<string, int> _options = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "-v", -1 },
            { "--version", -1 },
            { "-i", 0 },
            { "--install", 0 },
            { "-d", 1 },
            { "--remove", 1 },
            { "-u", 2 },
            { "--upgrade", 2 }
        };

        /// <summary>
        /// The Nuget package repository.
        /// </summary>
        private IPackageRepository _repository;

        /// <summary>
        /// The Nuget package manager.
        /// </summary>
        private PackageManager _manager;

        /// <summary>
        /// The runtime configuration.
        /// </summary>
        private RuntimeConfig _runtimeConfiguration = null;

        /// <summary>
        /// The packages root path.
        /// </summary>
        private string _packagesRootPath = null;

        #endregion



    }
}
