using System;
using System.IO;
using System.Net;
using Google.Protobuf;

namespace Server.Package
{
    public static class PackageUtil
    {
        public static byte[] GetMsgPackage(short code, IMessage message)
        {
            if (message == null)
            {
                return default;
            }

            var size = message.CalculateSize();
            byte[] resultBytes = new byte[6 + size];
            using var memoryStream = new MemoryStream(resultBytes);
            using var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(IPAddress.HostToNetworkOrder(code));
            binaryWriter.Write(IPAddress.HostToNetworkOrder(size));
            using var input = new CodedOutputStream(memoryStream, 6, false);
            message.WriteTo(input);
            input.Flush();

            return resultBytes;
        }
        
    }
}