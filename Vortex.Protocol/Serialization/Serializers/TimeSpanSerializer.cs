namespace Vortex.Protocol.Serialization.Serializers;

public class TimeSpanSerializer : IFieldSerializer
{
    public void Write(BinaryWriter writer, object? value)
    {
        writer.Write(((TimeSpan?)value)?.Ticks ?? 0);
    }

    public object Read(BinaryReader reader)
    {
        return new TimeSpan(reader.ReadInt64());
    }
}
