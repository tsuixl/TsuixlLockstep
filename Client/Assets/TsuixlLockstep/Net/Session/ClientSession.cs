using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Network.Http;
using Tsuixl.Net.Buffer;

namespace Tsuixl.Net.Session
{
    
    public class ClientSession: ISynchronizationContext, ISession
    {
        public string SessionID { get; protected set; }
        public EndPoint RemoteEndPoint => _socket?.RemoteEndPoint;
        public Socket Socket => _socket;
        public bool IsConnected => _socket != null && _socket.Connected;
        
        protected Socket _socket;
        private Queue<ISendBuffer> _sendBuffers;

        private IPackageFilter _packageFilter;
        private ISessionEvent _sessionEvent;

        private ClientSession()
        { }

        public ClientSession(IPackageFilter filter, ISessionEvent sessionEvent)
        {
            _packageFilter = filter;
            _sessionEvent = sessionEvent;
            _sendBuffers = new Queue<ISendBuffer>();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(string ip, int port)
        {
            try
            {
                _socket.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), OnConnectCallBack, _socket);
            }
            catch (Exception e)
            {
                OnConnected(false);
            }
        }

        private void OnConnectCallBack(IAsyncResult ar)
        {
            try
            {
                var socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                if (!socket.Connected)
                {
                    OnConnected(false);
                    return;
                }
                StartReceive();
                OnConnected(true);
            }
            catch (Exception e)
            {
                OnDisconnected(e.ToString());
            }
        }

        #region 发送

        public void Send(short code, IMessage message)
        {
            var buffer = ClassPool<NetTransferBuffer>.Get();
            var bodyLength = message.CalculateSize();
            buffer.Capacity = _packageFilter.HeadLength + bodyLength;
            buffer.Reset();
            buffer.WriteShort(code);
            buffer.WriteInt32(bodyLength);
            using (var outputStream = new CodedOutputStream(buffer.GetRefCache, _packageFilter.HeadLength, bodyLength))
            {
                message.WriteTo(outputStream);
            }
            buffer.IncreaseSize(bodyLength);
            Send(buffer);
        }
        
        public void Send(ISendBuffer buffer)
        {
            if (buffer == null)
                return;

            var count = 0;
            lock (_sendBuffers)
            {
                _sendBuffers.Enqueue(buffer);
                count = _sendBuffers.Count;
            }

            if (count != 1)
            {
                return;
            }
            
            _SendBuffer(buffer);
        }
        
        private void _SendBuffer(ISendBuffer buffer)
        {
            try
            {
                _socket.BeginSend(buffer.Data, buffer.Offset, buffer.RemainSize, 0, OnSendCallback, buffer);
            }
            catch (Exception e)
            {
                OnDisconnected(e.ToString());
            }
        }

        private void OnSendCallback(IAsyncResult ar)
        {
            var context = (ISendBuffer) ar.AsyncState;
            if (context == null)
            {
                OnDisconnected("OnSendCallback context == null");
                return;
            }

            var sendCount = EndSend(ar);
            if (sendCount == 0) return;

            context.Advance(sendCount);
            if (context.IsSendFinish)
            {
                context.OnRecycle();

                ISendBuffer nextBuffer = null;
                lock (_sendBuffers)
                {
                    _sendBuffers.Dequeue();
                    if (_sendBuffers.Count > 0)
                    {
                        nextBuffer = _sendBuffers.Peek();
                    }
                }

                if (nextBuffer != null)
                {
                    _SendBuffer(nextBuffer);
                }
            }
            else
            {
                _SendBuffer(context);
            }
        }
        
        private int EndSend(IAsyncResult ar)
        {
            var error = string.Empty;
            var sendLength = 0; 
            do
            {
                if (!IsConnected) break;
                
                try
                {
                    sendLength = _socket.EndSend(ar);
                }
                catch (Exception e)
                {
                    error = e.ToString();
                }
            } while (false);

            if (sendLength == 0)
            {
                OnDisconnected(error);
            }
           
            return sendLength;
        }

        #endregion

        #region 接收

        private void StartReceive()
        {
            if (!IsConnected)
            {
                return;
            }
            
            var byteBuffer = ClassPool<ByteBuffer>.Get();
            byteBuffer.Reset();
            byteBuffer.Capacity = 64;
            
            _socket.BeginReceive(byteBuffer.GetRefCache, 0, _packageFilter.HeadLength, SocketFlags.None,
                OnReceiveCallback, byteBuffer);
        }

        private void OnReceiveCallback(IAsyncResult ar)
        {
            var length = EndReceive(ar);
            if (length <= 0)
            {
                return;
            }
            var buffer = (ByteBuffer) ar.AsyncState;
            buffer.IncreaseSize(length);
            if (buffer.Count < _packageFilter.HeadLength)
            {
                // 继续接收包头
                _socket.BeginReceive(buffer.GetRefCache, buffer.Count, _packageFilter.HeadLength - buffer.Count,
                    SocketFlags.None, OnReceiveCallback, buffer);
            }
            else
            {
                // 准备接收消息体
                var bodyLength = _packageFilter.GetBodyLength(buffer);
                buffer.SetUserData(bodyLength);
                buffer.Capacity = _packageFilter.HeadLength + bodyLength;
                _socket.BeginReceive(buffer.GetRefCache, _packageFilter.HeadLength, bodyLength,
                    SocketFlags.None,
                    OnReceiveBodyCallback, buffer);
            }
        }


        private void OnReceiveBodyCallback(IAsyncResult ar)
        {
            var length = EndReceive(ar);
            if (length <= 0)
            {
                return;
            }
            var buffer = (ByteBuffer) ar.AsyncState;
            buffer.IncreaseSize(length);
            var bodyLength = buffer.GetUserData();
            if (buffer.Count < bodyLength)
            {
                // 继续接收
                _socket.BeginReceive(buffer.GetRefCache, buffer.Count, bodyLength - buffer.Count,
                    SocketFlags.None, OnReceiveBodyCallback, buffer);
            }
            else
            {
                // 接收完成
                OnCompleteReceive(buffer);
            }
        }


        private int EndReceive(IAsyncResult ar)
        {
            var length = 0;
            var error = string.Empty;
            do
            {
                if (!IsConnected) 
                    break;
                try
                {
                    length = _socket.EndReceive(ar);
                }
                catch (Exception e)
                {
                    length = 0;
                    error = e.ToString();
                    break;
                }
                
            } while (false);

            if (length <= 0)
            {
                OnDisconnected(error);
            }
            return length;
        }

        private void OnCompleteReceive(ByteBuffer buffer)
        {
            NetSynchronizationContext.Dispatch(this, _packageFilter.DecodePackage(buffer));
            StartReceive();
        }

        #endregion


        private void OnConnected(bool succeed, string error = "")
        {
            var data = ClassPool<SynchronizationSessionData>.Get();
            data.Type = succeed ? NetMessageType.ON_CONNECT_SUCCEED : NetMessageType.ON_CONNECT_FAILD;
            data.StringContent = error;
            NetSynchronizationContext.Dispatch(this, data);
            NetLog.LogError(error);
        }
        
        private void OnDisconnected(string error)
        {
            var data = ClassPool<SynchronizationSessionData>.Get();
            data.StringContent = error;
            data.Type = NetMessageType.ON_EXCEPTION;
            NetSynchronizationContext.Dispatch(this, data);
            Close();
        }
        
        public void Close()
        {
            lock (_sendBuffers)
            {
                _sendBuffers.Clear();
            }
            
            if (_socket != null)
            {
                var socket = _socket;
                _socket = null;
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
            }
        }


        public void OnSynchronizationContext(object context)
        {
            if (context is SynchronizationSessionData data)
            {
                switch (data.Type)
                {
                    case NetMessageType.ON_CONNECT_SUCCEED:
                        _sessionEvent?.OnConnected(true);
                        break;
                    case NetMessageType.ON_CONNECT_FAILD:
                        _sessionEvent?.OnConnected(false);
                        break;
                    case NetMessageType.ON_EXCEPTION:
                        _sessionEvent?.OnException(data.StringContent);
                        break;
                    case NetMessageType.ON_MESSAGE:
                        _sessionEvent?.OnMessage(this, data.Packet);
                        break;
                    case NetMessageType.ON_CLOSE:
                        _sessionEvent?.OnClose();
                        break;
                }
                ClassPool<SynchronizationSessionData>.Recycle(data);
            }
        }
    }
}