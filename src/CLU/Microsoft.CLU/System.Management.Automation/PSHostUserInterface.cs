﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;

namespace System.Management.Automation.Host
{
    public abstract class PSHostUserInterface
    {
        protected PSHostUserInterface() { }

        public abstract int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice);
        public abstract PSCredential PromptForCredential(string caption, string message, string userName, string targetName);
        public abstract string ReadLine();
        public abstract void Write(string value);
        public abstract void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value);
        public abstract void WriteDebugLine(string message);
        public abstract void WriteErrorLine(string value);
        public abstract void WriteLine(string value);
        public virtual void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value) { }
        public abstract void WriteProgress(long sourceId, ProgressRecord record);
        public abstract void WriteVerboseLine(string message);
        public abstract void WriteWarningLine(string message);
    }
}
