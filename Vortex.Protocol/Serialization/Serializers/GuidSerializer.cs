using Vortex.Protocol.Serialization;

namespace Vortex.Protocol.Serialization.Serializers;

internal class GuidSerializer : FieldSerializer<Guid>
{
    protected override Guid ReadOverride(BinaryReader br) => new(br.ReadBytes(16));
    protected override void WriteOverride(BinaryWriter bw, Guid value) => bw.Write(value.ToByteArray());
}
