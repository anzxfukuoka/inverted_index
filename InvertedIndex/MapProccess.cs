﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvertedIndex
{
    internal class MapProccess<TKey, TValue>
    {
        private Thread thread;

        public delegate Dictionary<TKey, TValue> MapFunc (params object[] input);
        private object[] mapFuncInput;

        private MapFunc mapFunc;

        private ConcurrentDictionary<TKey, List<TValue>> results;

        public MapProccess(MapFunc mapFunc, ref ConcurrentDictionary<TKey, List<TValue>> results, params object[] mapFuncInput)
        {
            this.mapFunc = mapFunc;

            this.thread = new Thread(Map);
            this.mapFuncInput = mapFuncInput;

            this.results = results;
            
            this.thread.Start();
        }

        public void WaitForResult() 
        {
            thread.Join();
        }

        public void Map() 
        {
            Dictionary<TKey, TValue> returnValues = mapFunc(mapFuncInput);

            foreach (KeyValuePair<TKey, TValue> pair in returnValues) 
            {
                var key = pair.Key;
                var value = pair.Value; 

                if (results.TryGetValue(key, out var a))
                {
                    results[key].Add(value);
                }
                else 
                {
                    if (!results.TryAdd(key, new List<TValue>() { pair.Value })) 
                    {
                        results[key].Add(value);
                    }
                }
            }
        }
    }
}
