namespace Vortex.Protocol.Serialization.Serializers;

internal class DateTimeSerializer : FieldSerializer<DateTime>
{
    protected override DateTime ReadOverride(BinaryReader br)
    {
        var ticks = br.ReadInt64();
        return new DateTime(ticks, DateTimeKind.Utc);
    }

    protected override void WriteOverride(BinaryWriter bw, DateTime value)
    {
        bw.Write(value.ToUniversalTime().Ticks);
    }
}
