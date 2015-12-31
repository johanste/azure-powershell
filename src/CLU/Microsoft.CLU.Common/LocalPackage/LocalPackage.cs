using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.CLU.Common
{
    /// <summary>
    /// Represents details of a package exists in local file system under packages root directory.
    /// </summary>
    internal class LocalPackage
    {

        /// <summary>
        /// Protected constructor ensure class instances created only via
        /// LocalPackage::Load* static methods.
        /// </summary>
        public LocalPackage(PackageConfig config, DirectoryInfo packageDirInfo)
        {
            this.Config = config;
            FullName = config.Name + "." + packageDirInfo.Name;
            Name = config.Name;
            FullPath = packageDirInfo.FullName;
            _marker = new PackageMarker(packageDirInfo.FullName);
            _commandAssemblies = config.CommandAssemblies.ToArray();
        }


        /// <summary>
        /// The base name of the package. Base name is the package name without version tag.
        /// e.g. if the FullName is Contoso.FileUtils.7.0.1 then base name is "Contoso.FileUtils"
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Full package name that includes package version tag.
        /// e.g. Contoso.FileUtils.7.0.1
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Absolute path to the package in the local file system under packages root directory.
        /// </summary>
        public string FullPath { get; private set; }

        public PackageConfig Config { get; private set; }

        /// <summary>
        /// The version tag of the package.
        /// e.g. if the FullName of the package is Contoso.FileUtils.7.0.1 then version is "7.0.1"
        /// </summary>
        public string Version
        {
            get
            {
                return FullName.Remove(0, Name.Length + 1);
            }
        }

        /// <summary>
        /// Full path to lib directory for the current FX
        /// </summary>
        public string LibDirPath
        {
            get
            {
                return Path.Combine(FullPath, Common.Constants.LibFolder, CLUEnvironment.CLRName);
            }
        }
        /// <summary>
        /// Full path to content directory of the package.
        /// </summary>
        public string ContentDirPath
        {
            get
            {
                return Path.Combine(FullPath, Constants.ContentFolder);
            }
        }

        /// <summary>
        /// Load and return reference to the default package assembly. A package may contains multiple
        /// assemblies, for e.g. assemblies with command implementations, assembly with name same as
        /// the package and more. This method loads the one with name same as package (if exists).
        /// </summary>
        /// <returns></returns>
        public Assembly DefaultAssembly
        {
            get { return _commandAssemblies.FirstOrDefault(); }
        }

        public IEnumerable<Assembly> CommandAssemblies
        {
            get
            {
                return _commandAssemblies;
            }
        }

        /// <summary>
        /// Backing field used in LoadDefaultAssembly method.
        /// </summary>
        private Assembly _defaultPackageAssembly;


        /// <summary>
        /// Loads the name mapping rules defined in this package.
        /// </summary>
        /// <returns>The name mapping</returns>
        public ConfigurationDictionary LoadNameMapping()
        {
            return ConfigurationDictionary.Load(Path.Combine(FullPath, Constants.IndexFolder, Constants.NameMappingFileName));
        }


        #region Private fields

        private PackageMarker _marker;
        private Assembly[] _commandAssemblies;
        #endregion
    }
}