using Tsuixl.Net.Buffer;

namespace Tsuixl.Net
{
    public interface IPackageFilter
    {
        int HeadLength { get; }

        int GetBodyLength(ByteBuffer buffer);

        PacketData DecodePackage(ByteBuffer buffer);
    }
}