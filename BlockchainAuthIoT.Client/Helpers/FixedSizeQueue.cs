using System.Collections.Concurrent;

namespace BlockchainAuthIoT.Client.Helpers
{
    public class FixedSizedQueue<T>
    {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private readonly object lockObject = new();

        public int Limit { get; set; }

        public FixedSizedQueue(int limit)
        {
            Limit = limit;
        }
        
        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);
            lock (lockObject)
            {
                while (queue.Count > Limit && queue.TryDequeue(out _)) ;
            }
        }

        public T[] ToArray() => queue.ToArray();
    }
}
