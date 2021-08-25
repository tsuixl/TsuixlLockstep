using System;
using System.Collections.Concurrent;
using System.Threading;
using Network.Http;
using UnityEngine;

namespace Tsuixl.Net
{
    public class NetSynchronizationContext : MonoBehaviour
    {
        public class SynchronizationContextData
        {
            public ISynchronizationContext Owner;
            public object Data;

            public void Reset()
            {
                Owner = null;
                Data = null;
            }
        }

        private static bool _quitFlag;
        private static NetSynchronizationContext _instance;
        public static NetSynchronizationContext Instance
        {
            get
            {
                if (_quitFlag)
                {
                    return null;
                }
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NetSynchronizationContext>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("__NetSynchronizationContext__").AddComponent<NetSynchronizationContext>();
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }
                return _instance;
            }
        }

        private ConcurrentQueue<SynchronizationContextData> m_Jobs;
        private int m_MainThreadId;

        public void Init (){}
        
        private void Awake()
        {
            _instance = this;
            m_Jobs = new ConcurrentQueue<SynchronizationContextData>();
            m_MainThreadId = Thread.CurrentThread.ManagedThreadId;
        }
        
        public static void Dispatch(ISynchronizationContext owner, object data)
        {
            if (Instance != null)
            {
                if (owner != null)
                {
                    var contextData = ClassPool<SynchronizationContextData>.Get();
                    contextData.Data = data;
                    contextData.Owner = owner;
                    _instance.m_Jobs.Enqueue(contextData);
                }
            }
        }

        private void Update()
        {
            while (true)
            {
                if (m_Jobs != null && m_Jobs.TryDequeue(out var contextData))
                {
                    contextData.Owner.OnSynchronizationContext(contextData.Data);
                    contextData.Reset();
                    ClassPool<SynchronizationContextData>.Recycle(contextData);
                }
                else
                {
                    break;
                }
            }
        }

        private void OnApplicationQuit()
        {
            _quitFlag = true;
            _instance = null;
        }
    }
}