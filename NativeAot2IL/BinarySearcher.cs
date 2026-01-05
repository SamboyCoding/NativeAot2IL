using System.Text;
using NativeAot2IL.Logging;
using NativeAot2IL.Rtr;

namespace NativeAot2IL;

public class BinarySearcher(Binary binary)
{
    private readonly byte[] _binaryBytes = binary.GetRawBinaryContent();

    //Used for codereg location pre-2019
    //Used for metadata reg location in 24.5+

    private static int FindSequence(byte[] haystack, byte[] needle, int requiredAlignment = 1, int startOffset = 0)
    {
        //Convert needle to a span now, rather than in the loop (implicitly as call to SequenceEqual)
        var needleSpan = new ReadOnlySpan<byte>(needle);
        var haystackSpan = haystack.AsSpan();
        var firstByte = needleSpan[0];

        //Find the first occurrence of the first byte of the needle
        var nextMatchIdx = Array.IndexOf(haystack, firstByte, startOffset);

        var needleLength = needleSpan.Length;
        var endIdx = haystack.Length - needleLength;
        var checkAlignment = requiredAlignment > 1;

        while (0 <= nextMatchIdx && nextMatchIdx <= endIdx)
        {
            //If we're not aligned, skip this match
            if (!checkAlignment || nextMatchIdx % requiredAlignment == 0)
            {
                //Take a slice of the array at this position and the length of the needle, and compare
                if (haystackSpan.Slice(nextMatchIdx, needleLength).SequenceEqual(needleSpan))
                    return nextMatchIdx;
            }

            //Find the next occurrence of the first byte of the needle
            nextMatchIdx = Array.IndexOf(haystack, firstByte, nextMatchIdx + 1);
        }

        //No match found
        return -1;
    }

    // Find all occurrences of a sequence of bytes, using word alignment by default
    private IEnumerable<uint> FindAllBytes(byte[] signature, int alignment = 0)
    {
        LibLogger.VerboseNewline($"\t\t\tLooking for bytes: {string.Join(" ", signature.Select(b => b.ToString("x2")))}");
        var offset = 0;
        var ptrSize = binary.is32Bit ? 4 : 8;
        while (offset != -1)
        {
            offset = FindSequence(_binaryBytes, signature, alignment != 0 ? alignment : ptrSize, offset);
            if (offset != -1)
            {
                yield return (uint)offset;
                offset += ptrSize;
            }
        }
    }

    // Find strings
    public IEnumerable<uint> FindAllStrings(string str) => FindAllBytes(Encoding.ASCII.GetBytes(str), 1);

    // Find 32-bit words
    private IEnumerable<uint> FindAllDWords(uint word) => FindAllBytes(BitConverter.GetBytes(word),  4);

    // Find 64-bit words
    private IEnumerable<uint> FindAllQWords(ulong word) => FindAllBytes(BitConverter.GetBytes(word), 8);

    // Find words for the current binary size
    private IEnumerable<uint> FindAllWords(ulong word)
        => binary.is32Bit ? FindAllDWords((uint)word) : FindAllQWords(word);

    private IEnumerable<ulong> MapOffsetsToVirt(IEnumerable<uint> offsets)
    {
        foreach (var offset in offsets)
            if (binary.TryMapRawAddressToVirtual(offset, out var word))
                yield return word;
    }

    // Find all valid virtual address pointers to a virtual address
    public IEnumerable<ulong> FindAllMappedWords(ulong word)
    {
        var fileOffsets = FindAllWords(word).ToList();
        return MapOffsetsToVirt(fileOffsets);
    }

    // Find all valid virtual address pointers to a set of virtual addresses
    public IEnumerable<ulong> FindAllMappedWords(IEnumerable<ulong> va) => va.SelectMany(FindAllMappedWords);

    public IEnumerable<ulong> FindAllMappedWords(IEnumerable<uint> va) => va.SelectMany(a => FindAllMappedWords(a));

    public ulong FindRtrHeader()
    {
        // ReadyToRun header magic is RTR\0, i.e., 0x00525452
        LibLogger.VerboseNewline("\tSearching for ReadyToRun header...");
        
        var rtrMagic = 0x00525452u;
        var candidates = FindAllDWords(rtrMagic);
        
        foreach (var candidate in candidates)
        {
            LibLogger.Verbose($"\t\tConsidering potential RTR header at raw offset 0x{candidate:X}...");
            if (!binary.TryMapRawAddressToVirtual(candidate, out var vaCandidate))
            {
                LibLogger.VerboseNewline($"Could not map to virtual address, skipping.");
                continue;
            }

            if (!binary.IsInDataSection(vaCandidate))
            {
                LibLogger.VerboseNewline($"Not in a data section, skipping.");
                //RTR header should be in a data section (specifically, .rdata)
                continue;
            }

            //Read the ReadyToRun header at this location
            try
            {
                var rtrHeader = binary.ReadReadableAtVirtualAddress<ReadyToRunDirectory>(vaCandidate);
                
                LibLogger.Verbose("Seems valid, checking further...");

                //Check the number of sections and the size of the entries
                if (rtrHeader is { NumberOfSections: > 0 and < 0x30, EntrySize: RtrSection.SizeInBytes })
                {
                    LibLogger.VerboseNewline($"Accepting as valid RTR header at VA 0x{vaCandidate:X}.");
                    return vaCandidate;
                }
                
                LibLogger.VerboseNewline($"Invalid ReadyToRun header (NumberOfSections: {rtrHeader.NumberOfSections}, EntrySize: {rtrHeader.EntrySize}), skipping.");
            }
            catch (Exception e)
            {
                //Ignore and try next
                LibLogger.VerboseNewline("Exception trying to read as ReadyToRun header, skipping.");
            }
        }
        
        throw new Exception("Could not find ReadyToRun header in binary.");
    }
}
