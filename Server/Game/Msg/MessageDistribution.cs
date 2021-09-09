using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace Server.Game
{
    public class MessageDistribution : Singleton<MessageDistribution>
    {
        private List<IMessageHandler> _messageHandlers;

        public override void Init()
        {
            _messageHandlers = new List<IMessageHandler>();
        }
        
        public void OnThreadMessage(short code, IMessage message)
        {
            SynchronizationQueue.Instance.Post(
                (o) => { OnMainThreadMessage(code, message); }, null
            );
        }

        public void AddMessageHandler(IMessageHandler messageHandler)
        {
            _messageHandlers.Add(messageHandler);
        }

        public void RemoveMessageHandler(IMessageHandler messageHandler)
        {
            _messageHandlers.Remove(messageHandler);
        }

        public void OnMainThreadMessage(short code, IMessage message)
        {
            Console.WriteLine($"OnMainThreadMessage : {code}");
            for (int i = 0; i < _messageHandlers.Count; i++)
            {
                _messageHandlers[i].OnMessageHandler(code, message);
            }
        }
    }
}