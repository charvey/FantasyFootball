using System.Collections.Concurrent;
using System.Diagnostics;

namespace FantasyFootball.Terminal.Daily
{
    public class BlockingCollectionSlim<T>
    {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private readonly AutoResetEvent waitToTake = new AutoResetEvent(false);
        private readonly AutoResetEvent waitToAdd = new AutoResetEvent(false);
        private bool stillAdding;

        public int Capacity { get; }
        public int Count => queue.Count;
        public bool IsCompleted => !stillAdding && queue.Count == 0;

        public BlockingCollectionSlim(int capacity = int.MaxValue)
        {
            this.Capacity = capacity;
            this.stillAdding = true;
        }

        public void CompleteAdding() => stillAdding = false;

        public void Add(T item)
        {
            while (queue.Count >= Capacity)
                waitToAdd.WaitOne();
            queue.Enqueue(item);
            waitToTake.Set();
        }
        public T Take()
        {
            T item;
            while (!queue.TryDequeue(out item))
                waitToTake.WaitOne();
            waitToAdd.Set();
            return item;
        }
        public bool TryTake(out T item, TimeSpan patience)
        {
            if (queue.TryDequeue(out item))
            {
                waitToAdd.Set();
                return true;
            }
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed < patience)
            {
                if (queue.TryDequeue(out item))
                {
                    waitToAdd.Set();
                    return true;
                }
                var patienceLeft = (patience - stopwatch.Elapsed);
                if (patienceLeft <= TimeSpan.Zero)
                    break;
                else if (patienceLeft < MinWait)
                    // otherwise the while loop will degenerate into a busy loop,
                    // for the last millisecond before patience runs out
                    patienceLeft = MinWait;
                waitToTake.WaitOne(patienceLeft);
            }
            return false;
        }
        private static readonly TimeSpan MinWait = TimeSpan.FromMilliseconds(1);
    }
}
