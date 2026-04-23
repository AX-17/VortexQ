namespace Vortex.Protocol.Serialization.Serializers;

internal class StringSerializer : FieldSerializer<string>
{
    protected override string ReadOverride(BinaryReader br) => br.ReadString();
    protected override void WriteOverride(BinaryWriter bw, string value) => bw.Write(value);
}
