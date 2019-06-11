using System;
using System.Collections.Generic;

namespace AuthProject.WorkflowTest
{
    public class WorkflowException : Exception
    {
        public WorkflowException(string exception) : base(exception)
        {
        }

        public WorkflowException(IEnumerable<string> results) : base(string.Join("\n", results))
        {
        }
    }
}