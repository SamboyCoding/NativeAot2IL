namespace NativeAot2IL.Metadata;

public class MetadataHeader : ReadableClass
{
    public const uint RequiredMagic = 0xDEADDFFD;
    
    public uint Magic;
    public MetadataHandle[] ScopeDefinitionHandles { get; private set; }
    
    public override void Read(ClassReadingBinaryReader reader)
    {
        Magic = reader.ReadUInt32();
        if (Magic != RequiredMagic)
        {
            throw new InvalidDataException($"Invalid MetadataHeader magic: 0x{Magic:X8}, expected 0x{RequiredMagic:X8}");
        }

        ScopeDefinitionHandles = reader.ReadMetadataHandleArrayHereNoLock(HandleType.ScopeDefinition);
    }
}