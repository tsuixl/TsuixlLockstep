using Google.Protobuf;
using Network.Http;
using Tsuixl.Net;
using Tsuixl.Net.Buffer;
using Tsuixl.Net.Session;

namespace Tsuixl.Net
{
    public class PacketData
    {
        public short Code { get; internal set; }
        public int HeadLength { get; internal set; }
        public ByteBuffer Buffer { get; internal set; }

        public PacketData()
        { }

        public void Release()
        {
            Code = 0;
            HeadLength = 0;
            if (Buffer != null)
            {
                Buffer.Reset();
                ClassPool<ByteBuffer>.Recycle(Buffer);
                Buffer = null;
            }
            ClassPool<PacketData>.Recycle(this);
        }
        
        public static T ToProtobufMessage<T>(ref PacketData packetData, MessageParser<T> parser, bool autoReleasePacket = true) where T : IMessage<T>
        {
            var result = parser.ParseFrom(packetData.Buffer.GetRefCache, packetData.HeadLength,
                packetData.Buffer.Count - packetData.HeadLength);
            if (autoReleasePacket)
            {
                packetData.Release();
                packetData = null;
            }
            return result;
        }
    }
}