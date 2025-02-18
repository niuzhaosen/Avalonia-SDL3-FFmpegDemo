using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
    internal static class ThreadQueue
    {
        internal static Queue<Action>  actions = new Queue<Action>();
        internal static Thread  thread;
        internal static AutoResetEvent signal = new AutoResetEvent(false);
        internal static void Start()
        {
            thread = new Thread(ProcessTasks) { IsBackground=true};
            thread.Start();
        }
        internal static void AddTask(Action action)
        {
            lock (actions)
            {
                actions.Enqueue(action);
            }
            signal.Set();
        }
        static void  ProcessTasks()
        {
            while (true)
            {
                Action action = null;
                lock (actions)
                {
                    if (actions.Count >0)
                    {
                        action = actions.Dequeue();
                    }
                }
                if(action!=null)
                {
                    action();
                }
                else
                {
                    signal.WaitOne();
                }
              
            }
        } }
}
