using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Tsuixl.Net
{


    public enum SocketState : byte
    {
        Connecting,
        Connected,
        Closing,
        Closed
    }
    
    public class TcpSocket
    {
        private Socket _socket;
        private SocketState _state = SocketState.Closed;


        public TcpSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        
        public async Task ConnectServer(string ip, int port)
        {
            _state = SocketState.Connecting;
            var succees = await Connect(ip, port);
            if (succees)
            {
                _state = SocketState.Connected;
            }
        }
        
        private async Task<bool> Connect(string ip, int port)
        {
            try
            {
                await _socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return false;
            }
            return true;
        }


        public void Close()
        {
            _state = SocketState.Closing;
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
                _socket = null;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
    }
}

