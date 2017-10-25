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
        public ProcessableGroup(params IProcessable [] procs)
        {
            Add(procs);
        }
        public void Add(IProcessable proc)
        {
            _proc.Add(proc);
        }
        public void Add(params IProcessable[] procs)
        {
            foreach (var proc in procs)
            {
                _proc.Add(proc);
            }
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
