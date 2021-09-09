using System.Threading;
using Google.Protobuf;

namespace Server.Game
{
    public class GameWorld : IMessageHandler
    {
        public GameWorld()
        {
            MessageDistribution.Instance.AddMessageHandler(this);
        }
        
        public void Update()
        {
        }

        public void OnDestroy()
        {
        }

        public void OnMessageHandler(short code, IMessage message)
        {
            
        }
    }
    
}