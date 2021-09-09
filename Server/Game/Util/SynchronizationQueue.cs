using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Server.Game
{
    public class SynchronizationQueue : SynchronizationContext
    {
        public static SynchronizationQueue Instance { get; private set; }
        private readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

        public SynchronizationQueue()
        {
            Instance = this;
        }
        
        private void Add(Action action){
            _queue.Enqueue(action);
        }
        
        public void Update(){
            while (true) {
                if (!_queue.TryDequeue(out var a)) {
                    return;
                }
                a();
            }
        }
        
        public override void Post(SendOrPostCallback callback, object state){
            this.Add(() => { callback(state); });
        }

        public void Debug()
        {
            Console.WriteLine("123");
        }
        
    }
}