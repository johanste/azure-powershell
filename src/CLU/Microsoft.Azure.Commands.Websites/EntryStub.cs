﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Reflection;

namespace Microsoft.Azure.Commands.Websites
{
    public class EntryStub
    {
        public static int Main(string[] args)
        {
            var baseDirectory = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            var config = new Microsoft.CLU.Common.PackageConfig(
                "Microsoft.Azure.Commands.Websites",
                new System.Reflection.Assembly[] { typeof(EntryStub).GetTypeInfo().Assembly },
                "AzureRm");

            return Microsoft.CLU.CommandPackage.CommandPackage.Handle(config, baseDirectory.Parent.Parent, args);
        }
    }
}
