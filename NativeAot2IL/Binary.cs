using System.Diagnostics.CodeAnalysis;

namespace NativeAot2IL;

public abstract class Binary(MemoryStream input) : ClassReadingBinaryReader(input)
{
    protected const long VirtToRawInvalidNoMatch = long.MinValue + 1000;
    protected const long VirtToRawInvalidOutOfBounds = long.MinValue + 1001;
    
    public InstructionSetId InstructionSetId = null!;
    
    public abstract long RawLength { get; }

    /// <summary>
    /// Can be overriden if, like the wasm format, your data has to be unpacked and you need to use a different reader
    /// </summary>
    public virtual ClassReadingBinaryReader Reader => this;

    // private float _metadataVersion;
    // public sealed override float MetadataVersion => _metadataVersion; 

    public int InBinaryMetadataSize { get; private set; }

    public void Init(ulong pCodeRegistration, ulong pMetadataRegistration)
    {
        
    }

    public abstract byte GetByteAtRawAddress(ulong addr);
    public abstract long MapVirtualAddressToRaw(ulong uiAddr, bool throwOnError = true);
    public abstract ulong MapRawAddressToVirtual(uint offset);
    public abstract ulong GetRva(ulong pointer);

    public bool TryMapRawAddressToVirtual(in uint offset, out ulong va)
    {
        try
        {
            va = MapRawAddressToVirtual(offset);
            return true;
        }
        catch (Exception)
        {
            va = 0;
            return false;
        }
    }

    public bool TryMapVirtualAddressToRaw(ulong virtAddr, out long result)
    {
        result = MapVirtualAddressToRaw(virtAddr, false);

        if (result != VirtToRawInvalidNoMatch)
            return true;

        result = 0;
        return false;
    }

    public T[] ReadReadableArrayAtVirtualAddress<T>(ulong va, long count) where T : ReadableClass, new() => Reader.ReadReadableArrayAtRawAddr<T>(MapVirtualAddressToRaw(va), count);

    public T ReadReadableAtVirtualAddress<T>(ulong va) where T : ReadableClass, new() => Reader.ReadReadable<T>(MapVirtualAddressToRaw(va));

    public ulong[] ReadNUintArrayAtVirtualAddress(ulong addr, long count) => Reader.ReadNUintArrayAtRawAddress(MapVirtualAddressToRaw(addr), (int)count);

    public override long ReadNInt() => is32Bit ? Reader.ReadInt32() : Reader.ReadInt64();

    public override ulong ReadNUint() => is32Bit ? Reader.ReadUInt32() : Reader.ReadUInt64();

    public ulong ReadPointerAtVirtualAddress(ulong addr)
    {
        return Reader.ReadNUintAtRawAddress(MapVirtualAddressToRaw(addr));
    }

    public abstract byte[] GetRawBinaryContent();
    public abstract ulong GetVirtualAddressOfExportedFunctionByName(string toFind);
    public virtual bool IsExportedFunction(ulong addr) => false;

    public virtual bool TryGetExportedFunctionName(ulong addr, [NotNullWhen(true)] out string? name)
    {
        name = null;
        return false;
    }

    public virtual IEnumerable<KeyValuePair<string, ulong>> GetExportedFunctions() => [];

    public abstract byte[] GetEntirePrimaryExecutableSection();

    public abstract ulong GetVirtualAddressOfPrimaryExecutableSection();
    
    public abstract bool IsInDataSection(ulong virtualAddress);
}
