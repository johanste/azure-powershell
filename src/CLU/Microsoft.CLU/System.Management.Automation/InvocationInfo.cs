using System.Collections.Generic;

namespace System.Management.Automation
{
    public class InvocationInfo
    {
        internal InvocationInfo()
        {
            BoundParameters = new Dictionary<string, object>();
        }

        public Dictionary<string, object> BoundParameters { get; internal set; }
        public string InvocationName { get; internal set; }
        public CommandInfo MyCommand { get; internal set; }
    }
}
