using Google.Protobuf;

namespace Server.Game
{
    public interface IMessageHandler
    {
        void OnMessageHandler(short code, IMessage message);
    }
}