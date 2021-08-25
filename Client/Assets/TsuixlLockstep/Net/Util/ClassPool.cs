using System;
using System.Collections.Generic;
using System.Threading;

namespace Network.Http
{
    /// <summary>
    /// 实例对象缓存
    /// 多线程测试用例 ParallelTest.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ClassPool<T> where T : new()
    {
        private static readonly Pool<T> m_Pool = new Pool<T>();

        #region 下列方法支持在多线程环境下调用

        public static T Get()
        {
            return m_Pool.Get();
        }

        /// <summary>
        /// 为了更好的性能，不会验证该对象在池中是否重复，需要使用方自行处理。
        /// </summary>
        /// <param name="element"></param>
        public static void Recycle(T element)
        {
            m_Pool.Recycle(element);
        }

        public static void Clear()
        {
            m_Pool.Clear();
        }
        
        #endregion
        
        #region 下列的方法并不保证在多线程下数据安全，仅仅是作为Editor调试接口。
        
        public static int Count()
        {
            return m_Pool.count;
        }
        
        public static int InactiveCount()
        {
            return m_Pool.inactiveCount;
        }

        public static int ActiveCount()
        {
            return m_Pool.activeCount;
        }
        
        #endregion
    }

    class Pool<T> where T : new()
    {
        
#if UNITY_EDITOR
        private SpinLock m_SpinLock = new SpinLock(true);
#else
        private SpinLock m_SpinLock = new SpinLock ();
#endif

        private readonly Stack<T> m_Stack = new Stack<T>();

        public int count { get; private set; }
        public int activeCount => count - inactiveCount;
        public int inactiveCount => m_Stack.Count;

        public Pool()
        {

        }

        public T Get()
        {
            var gotLock = false;
            try
            {
                m_SpinLock.Enter(ref gotLock);
                T element;
                if (m_Stack.Count == 0)
                {
                    element = new T();
                    count++;
                }
                else
                {
                    element = m_Stack.Pop();
                }
                return element;
            }
            finally
            {
                if (gotLock)
                {
                    m_SpinLock.Exit();
                }         
            }
            
        }

        public void Recycle(T element)
        {
            if (element == null)
            {
                return;
            }
            
            var gotLock = false;
            try
            {
                m_SpinLock.Enter(ref gotLock);
            
                if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                {
#if UNITY_EDITOR
                    throw new Exception("重复添加");
#else
                    return;
#endif
                }
                m_Stack.Push(element);
            }
            finally
            {
                if (gotLock)
                {
                    m_SpinLock.Exit();
                }
            }
        }

        public void Clear()
        {
            var gotLock = false;
            try
            {
                m_SpinLock.Enter(ref gotLock);
                m_Stack.Clear();
                count = 0;
            }
            finally
            {
                if (gotLock)
                {
                    m_SpinLock.Exit();
                }
            }
        }
    }
}