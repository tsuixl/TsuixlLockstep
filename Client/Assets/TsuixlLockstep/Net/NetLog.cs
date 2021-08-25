using System;
using UnityEngine;

namespace Tsuixl.Net
{
    public static class NetLog
    {
        public static string TAG = "[NET]-";


        public static void Log(object o)
        {
            Debug.Log($"{TAG}{o}");
        }

        public static void Log(string format, params object[] args)
        {
            format = $"{TAG}{format}";
            // Console.WriteLine(string.Format(format, args));
            Debug.LogFormat(format, args);
        }
        
        
        public static void LogWarning(object o)
        {
            Debug.LogWarning($"{TAG}{o}");
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            format = $"{TAG}{format}";
            Debug.LogWarningFormat(format, args);
        }
        
        
        public static void LogError(object o)
        {
            
            Debug.LogError($"{TAG}{o}");
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            format = $"{TAG}{format}";
            Debug.LogErrorFormat(format, args);
        }
    }
}