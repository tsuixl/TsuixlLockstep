using System.Buffers;
using SuperSocket.ProtoBase;

namespace Server.Package
{
    public class DefaultPackage : IKeyedPackageInfo<string>
    {
        public short Code;
        public int Length;
        public byte[] Buffer;
        public string Key => "LoginCommand";
    }
}