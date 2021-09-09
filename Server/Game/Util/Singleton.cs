namespace Server.Game
{
    public abstract class Singleton <T> where T : class, new () 
    {
        private static T _instance = null;
        private static readonly object _padlock = new object();

        protected Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }

        public abstract void Init();
    }
}