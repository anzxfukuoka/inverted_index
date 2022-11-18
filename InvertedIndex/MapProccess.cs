using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex
{
    internal class MapProccess<T>
    {
        private T returnValue;

        private Thread thread;

        public delegate T MapFunc (params object[] input);
        private object[] mapFuncInput;

        private MapFunc mapFunc;

        public delegate T a(params object[] a);

        public MapProccess(MapFunc mapFunc, params object[] mapFuncInput)
        {
            this.mapFunc = mapFunc;

            this.thread = new Thread(Map);
            this.mapFuncInput = mapFuncInput;
            
            
            this.thread.Start();
        }

        public T WaitForResult() 
        {
            thread.Join();
            return returnValue;
        }

        public void Map() 
        {
            returnValue = mapFunc(mapFuncInput);
        }
    }
}
