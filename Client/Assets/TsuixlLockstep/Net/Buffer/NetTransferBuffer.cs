using Network.Http;
using Tsuixl.Net.Buffer;

namespace Tsuixl.Net
{
    public class NetTransferBuffer : ByteBuffer, ISendBuffer
    {
        int ISendBuffer.Offset => Index;
        int ISendBuffer.RemainSize => Count;
        byte[] ISendBuffer.Data => GetRefCache;
        bool ISendBuffer.IsSendFinish => Count == 0;

        void ISendBuffer.OnRecycle()
        {
            Reset();
            ClassPool<NetTransferBuffer>.Recycle(this);
        }
    }
}