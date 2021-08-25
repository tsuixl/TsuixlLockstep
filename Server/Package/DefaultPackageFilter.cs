using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace Server.Package
{
    public class DefaultPackageFilter : FixedHeaderPipelineFilter<DefaultPackage>
    {
        
        public DefaultPackageFilter() : base(6)
        {
            
        }

        protected override int GetBodyLengthFromHeader(ref ReadOnlySequence<byte> buffer)
        {
            var reader = new SequenceReader<byte>(buffer);
            reader.Advance(2);
            reader.TryReadBigEndian(out int len);
            return len;
        }

        protected override DefaultPackage DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            var package = new DefaultPackage();
            var reader = new SequenceReader<byte>(buffer);
            
            // code
            reader.TryReadBigEndian(out short opShort);
            package.Code = opShort;
            
            // length
            reader.TryReadBigEndian(out int length);
            package.Length = length;
            
            // body
            package.Buffer = reader.Sequence.Slice(reader.Consumed).ToArray();
            
            return package;
        }
    }
}