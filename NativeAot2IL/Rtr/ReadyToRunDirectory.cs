namespace NativeAot2IL.Rtr;

public class ReadyToRunDirectory : ReadableClass
{
    public uint Magic;
    public ushort MajorVersion;
    public ushort MinorVersion;
    public uint Flags;
    public ushort NumberOfSections;
    public byte EntrySize;
    public byte EntryType;
    public RtrSection[]? Sections;
    
    public override void Read(ClassReadingBinaryReader reader)
    {
        Magic = reader.ReadUInt32();
        MajorVersion = reader.ReadUInt16();
        MinorVersion = reader.ReadUInt16();
        Flags = reader.ReadUInt32();
        NumberOfSections = reader.ReadUInt16();
        EntrySize = reader.ReadByte();
        EntryType = reader.ReadByte();

        Sections = new RtrSection[NumberOfSections];
        reader.FillReadableArrayHereNoLock(Sections);
    }
}