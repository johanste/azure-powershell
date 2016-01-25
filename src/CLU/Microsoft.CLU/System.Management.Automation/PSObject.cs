using Microsoft.CLU.Helpers;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace System.Management.Automation
{
    public class PSObject 
    {
        public const string AdaptedMemberSetName = "psadapted";
        public const string BaseObjectMemberSetName = "psbase";
        public const string ExtendedMemberSetName = "psextended";

        public PSObject()
        {
            Properties = new PropertyInfoCollection();
        }

        public object BaseObject { get; private set; }

        public PSMemberInfoCollection<PSPropertyInfo> Properties { get; private set; }

        internal class PropertyInfoCollection : PSMemberInfoCollection<PSPropertyInfo>
        {
            public override PSPropertyInfo this[string name]
            {
                get
                {
                    return _collection[name];
                }
            }

            public override void Add(PSPropertyInfo member)
            {
                _collection.Add(member.Name, member);
            }

            public override void Add(PSPropertyInfo member, bool preValidated)
            {
                Add(member);
            }

            public override IEnumerator<PSPropertyInfo> GetEnumerator()
            {
                return _collection.Values.GetEnumerator();
            }

            public override void Remove(string name)
            {
                if (_collection.ContainsKey(name))
                    _collection.Remove(name);
            }

            private Dictionary<string, PSPropertyInfo> _collection = new Dictionary<string, PSPropertyInfo>();
        }
    }
}
