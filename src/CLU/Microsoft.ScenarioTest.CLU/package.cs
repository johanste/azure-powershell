using System;
using System.Reflection;

namespace Microsoft.CLU.Test
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var baseDirectory = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            var config = new Microsoft.CLU.Common.PackageConfig(
                "Microsoft.ScenarioTests.CLU",
                new System.Reflection.Assembly[] { typeof(Program).GetTypeInfo().Assembly }
                );


            return Microsoft.CLU.CommandPackage.CommandPackage.Handle(config, baseDirectory.Parent.Parent, args);
        }
    }
}
