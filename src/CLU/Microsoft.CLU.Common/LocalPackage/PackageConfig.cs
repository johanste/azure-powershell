using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CLU.Common
{
    /// <summary>
    /// Type represents package configuration.
    /// </summary>
    public class PackageConfig
    {
        /// <summary>
        /// Private constructor to ensure class instances created only via
        /// PackageConfig::Load static method.
        /// </summary>
        public PackageConfig(string packageName, IEnumerable<Assembly> commandAssemblies, string nounPrefix = null, bool nounFirst = true) 
        {
            this.Name = packageName;
            this.CommandAssemblies = commandAssemblies;
            this.NounPrefix = nounPrefix;
            this.NounFirst = nounFirst;
        }

        /// <summary>
        /// The package name.
        /// </summary>
        public string Name
        {
            get; private set;
        }

        /// <summary>
        /// The package noun prefix.
        /// </summary>
        public string NounPrefix { get; private set; }
            

        /// <summary>
        /// The package verb/noun order.
        /// </summary>
        public bool NounFirst { get; private set;  }
        
        /// <summary>
        /// Backing field for CommandAssemblies property.
        /// </summary>
        private List<Assembly> _commandAssemblies;
        /// <summary>
        /// Collection of path to command assemblies in the package. Each path should
        /// be relative to the package root folder '$root\packages\package-name'.
        /// The assembly name (the last segment of the path) must have extension (.dll).
        /// e.g. lib/net452/Contoso.SystemUtils.dll
        /// </summary>
        public IEnumerable<Assembly> CommandAssemblies
        {
            get
            {
                return _commandAssemblies;
            }
            set
            {
                this._commandAssemblies = new List<Assembly>(value);
            }
        }
    }
}
