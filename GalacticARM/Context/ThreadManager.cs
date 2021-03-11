using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalacticARM.Context
{
    public static class ThreadManager
    {
        public static Dictionary<ulong, CpuThread> Threads  { get; internal set; }
        public static HashSet<ulong> ThreadIDs              { get; internal set; }

        static ThreadManager()
        {
            Threads = new Dictionary<ulong, CpuThread>();
            ThreadIDs = new HashSet<ulong>();
        }

        public static CpuThread CreateThread()
        {
            ulong ID = 0;

            while (true)
            {
                if (!ThreadIDs.Contains(ID))
                {
                    ThreadIDs.Add(ID);
                    Threads.Add(ID,new CpuThread(ID));

                    return Threads[ID];
                }

                ID++;
            }    
        }
    }
}
