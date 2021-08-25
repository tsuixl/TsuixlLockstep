using System;
using Network.Http;
using Tsuixl.Net.Buffer;

namespace Tsuixl.Net
{
    public class FixedHeadPacketFilter : IPackageFilter
    {
        private readonly Type _codeType;
        private readonly int _fixedHeadLength;
        
        public int HeadLength => _fixedHeadLength;
        
        public FixedHeadPacketFilter(int headLength)
        {
            _fixedHeadLength = headLength;
        }

        public int GetBodyLength(ByteBuffer buffer)
        {
            return buffer.PeekInt32(2);
        }

        public PacketData DecodePackage(ByteBuffer buffer)
        {
            var packageData = ClassPool<PacketData>.Get();
            packageData.Code = buffer.PeekShort();
            packageData.HeadLength = buffer.PeekInt32(2);
            packageData.Buffer = buffer;
            return packageData;
        }
    }
}