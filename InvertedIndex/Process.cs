using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex
{
    internal class Process<TResultKey, TResultValue>
    {
        private Thread thread;

        private ConcurrentDictionary<TResultKey, TResultValue> results;

        private Action<ConcurrentDictionary<TResultKey, TResultValue>> action;

        private delegate void PrFunc(ref ConcurrentDictionary<TResultKey, TResultValue> results);
        private PrFunc func;

        internal Process(Action<ConcurrentDictionary<TResultKey, TResultValue>> action, ConcurrentDictionary<TResultKey, TResultValue> results) 
        {
            this.action = action;
            this.results = results;
            //this.func = func;
            thread = new Thread(ProcessFunc);
        }

        public void Start() 
        {
            thread.Start();
        }

        public void WaitForResult() 
        {
            thread.Join();
        }

        private void ProcessFunc() 
        {
            action(results);
        }
    }
}
