using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public class ProcessableGroup : IProcessable
    {
        System.Collections.ArrayList _proc = new System.Collections.ArrayList();

        public ProcessableGroup()
        {
        }
        public void Add(IProcessable proc)
        {
            _proc.Add(proc);
        }
        public void RemoveAll()
        {
            _proc.Clear();
        }
        public void Process()
        {
            foreach (IProcessable proc in _proc)
            {
                proc.Process();
            }
        }
    }
}
