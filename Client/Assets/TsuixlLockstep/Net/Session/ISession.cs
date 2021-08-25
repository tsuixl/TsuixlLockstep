using System.Net;
using System.Net.Sockets;
using Tsuixl.Net.Buffer;

namespace Tsuixl.Net.Session
{
    public interface ISession
    {
        string SessionID { get; }

        EndPoint RemoteEndPoint { get; }

        Socket Socket { get; }

        bool IsConnected { get; }

        void Send(ISendBuffer buffer);
    }
}