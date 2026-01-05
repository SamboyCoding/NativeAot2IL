using System.Text;

#pragma warning disable 8618
//Disable null check because this stuff is initialized by reflection
namespace NativeAot2IL.PE;

public class SectionHeader : ReadableClass
{
    public string Name;
    public uint VirtualSize;
    public uint VirtualAddress;
    public uint SizeOfRawData;
    public uint PointerToRawData;
    public uint PointerToRelocations;
    public uint PointerToLinenumbers;
    public ushort NumberOfRelocations;
    public ushort NumberOfLinenumbers;
    public SectionFlags Characteristics;
    
    public bool IsData => (Characteristics & SectionFlags.ContentInitializedData) != 0 || (Characteristics & SectionFlags.ContentUninitializedData) != 0;
    public bool IsCode => (Characteristics & SectionFlags.ContentCode) != 0;

    public override void Read(ClassReadingBinaryReader reader)
    {
        Name = Encoding.UTF8.GetString(reader.ReadBytes(8)).TrimEnd('\0');
        VirtualSize = reader.ReadUInt32();
        VirtualAddress = reader.ReadUInt32();
        SizeOfRawData = reader.ReadUInt32();
        PointerToRawData = reader.ReadUInt32();
        PointerToRelocations = reader.ReadUInt32();
        PointerToLinenumbers = reader.ReadUInt32();
        NumberOfRelocations = reader.ReadUInt16();
        NumberOfLinenumbers = reader.ReadUInt16();
        Characteristics = (SectionFlags)reader.ReadUInt32();
    }
}
