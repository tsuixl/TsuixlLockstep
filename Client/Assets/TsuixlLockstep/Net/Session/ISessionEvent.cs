namespace Tsuixl.Net.Session
{
    public interface ISessionEvent
    {
        void OnConnected(bool succeed);

        void OnDisconnected(string error);

        void OnClose();

        void OnException(string error);

        void OnMessage(ISession session, PacketData data);
    }
}