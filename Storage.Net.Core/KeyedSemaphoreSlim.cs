using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Storage.Net.Core
{
    internal class KeyedSemaphoreSlim : IDisposable
    {
        private Queue<SemaphoreSlim> _pool = new Queue<SemaphoreSlim>();
        private Dictionary<string, SemaphoreSlim> _semaphoreMap = new Dictionary<string, SemaphoreSlim>();

        private object _lock = new object();

        public Task WaitAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (_lock)
            {
                if (!_semaphoreMap.TryGetValue(key, out SemaphoreSlim sem))
                {
                    if (_pool.Count > 0)
                        sem = _pool.Dequeue();
                    else
                        sem = new SemaphoreSlim(1, 1);
                    _semaphoreMap[key] = sem;
                }

                return sem.WaitAsync(cancellationToken);
            }
        }

        public void Release(string key)
        {
            lock (_lock)
            {
                var sem = _semaphoreMap[key];
                sem.Release();

                if (sem.CurrentCount == 1)
                {
                    _semaphoreMap.Remove(key);
                    _pool.Enqueue(sem);
                }
            }
        }

        public void Dispose()
        {
            foreach (var key in _semaphoreMap.Keys)
                Release(key);

            foreach (var sem in _pool)
                sem.Dispose();
        }
    }

}
