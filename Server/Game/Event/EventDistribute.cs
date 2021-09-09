using System;
using System.Collections.Generic;
using log4net;

namespace Server.Game
{
    public class EventDistribute : Singleton<EventDistribute>
    {
        public delegate void EventCallback(object sender, object data);
        private Dictionary<int, List<EventCallback>> _events;

        private static ILog _log;
        
        public override void Init()
        {
            _log = LogManager.GetLogger(this.GetType());
            _events = new Dictionary<int, List<EventCallback>>();
        }

        public void Register(int type, EventCallback callback)
        {
            if (_events.TryGetValue(type, out var callList))
            {
                callList.Add(callback);
            }
            else
            {
                _events[type] = new List<EventCallback> {callback};
            }
        }

        public void Cancel(int type, EventCallback callback)
        {
            if (_events.TryGetValue(type, out var callList))
            {
                callList.Remove(callback);
            }
        }

        public void Distribute(object sender, int type, object data)
        {
            if (TryGetEventList(type, out var list))
            {
                var count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        list[i].Invoke(sender, data);
                    }
                    catch (Exception e)
                    {
                        _log.Error(e);
                    }
                }
            }
        }

        private bool TryGetEventList(int type, out List<EventCallback> list)
        {
            if (_events.TryGetValue(type, out var callList))
            {
                list = callList;
            }
            else
            {
                list = new List<EventCallback>();
                _events[type] = list;
            }

            return true;
        }
        
    }
}