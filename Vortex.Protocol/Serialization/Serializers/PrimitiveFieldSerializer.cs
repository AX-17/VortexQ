namespace Vortex.Protocol.Serialization.Serializers;

internal class PrimitiveFieldSerializer<T> : FieldSerializer<T>
{
    protected override T ReadOverride(BinaryReader br)
    {
        var type = typeof(T);
        
        if (type.IsEnum)
        {
            type = Enum.GetUnderlyingType(type);
        }
        
        var method = typeof(BinaryReader).GetMethod($"Read{type.Name}")!;
        var result = method.Invoke(br, null)!;
        if (typeof(T).IsEnum)
        {
            return (T)Enum.ToObject(typeof(T), result);
        }
        
        return (T)result;
    }

    protected override void WriteOverride(BinaryWriter bw, T value)
    {
        var type = typeof(T);
        if (type.IsEnum)
        {
            type = Enum.GetUnderlyingType(type);
        }
        
        var method = typeof(BinaryWriter).GetMethod("Write", [type])!;
        method.Invoke(bw, [value]);
    }
}
