namespace NativeAot2IL.Rtr;

public class RtrSection : ReadableClass
{
    public const int SizeInBytes = 24; //0x18
    
    public RtrSectionType SectionType;
    public RtrSectionFlags Flags;
    public ulong Start;
    public ulong End;

    public override void Read(ClassReadingBinaryReader reader)
    {
        SectionType = (RtrSectionType)reader.ReadUInt32();
        Flags = (RtrSectionFlags)reader.ReadUInt32();
        Start = reader.ReadUInt64();
        End = reader.ReadUInt64();
    }
}