namespace NativeAot2IL.Metadata;

public class MetadataHandle
{
    private int _value;
    
    public HandleType Type => (HandleType)(_value >> 25);
    
    public int Offset => _value & 0x01FFFFFF;
    
    public bool IsNull => Offset == 0;
    
    public void ReadWithKnownType(ClassReadingBinaryReader reader, HandleType expectedType)
    {
        _value = (int) reader.ReadCompressedUIntAtRawAddrNoLock(-1, out _);
        _value |= ((int)expectedType << 25);
    }
    
    public void ReadWithUnknownType(ClassReadingBinaryReader reader)
    {
        var value = reader.ReadCompressedUIntAtRawAddrNoLock(-1, out _);
        
        //NB THIS CHANGED AT SOME POINT. .NET 10 uses 7 bits for type, 25 for offset
        //But earlier versions used 8 bits for type, 24 for offset
        var type = (HandleType)(byte) (value & 0x7F);
        var offset = (int)(value >> 7);
        
        _value = (int)(offset | ((uint)type << 25));
    }

    public override string ToString()
    {
        return $"MetadataHandle: {Type} (0x{Offset:x7})";
    }
    
    public T? Resolve<T>(ClassReadingBinaryReader reader, bool doLock = true) where T : ReadableClass, new()
    {
        if(IsNull)
            return null;
        
        if(doLock)
            return reader.ReadReadable<T>(Offset);
        else
            return reader.ReadReadableNoLock<T>(Offset);
    }
    
    public string? ResolveString(ClassReadingBinaryReader reader, bool doLock = true)
    {
        if(IsNull)
            return null;
        
        if (doLock)
            return reader.ReadLengthPrefixedStringAtRawAddress(Offset);
        else
            return reader.ReadLengthPrefixedStringAtRawAddressNoLock(Offset);
    }
}