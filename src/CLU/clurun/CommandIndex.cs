using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace clurun
{
    public class CommandIndex
    {
        public CommandIndex(string indexString)
        {
            string[] split1 = indexString.Split(':');
            Args = split1[0].Replace(";", " ");

            Package = split1[1].Split('/')[0];
        }

        public string Package { get; private set; }

        public string Args { get; private set; }
    }
}
