using NativeAot2IL.Logging;
using NativeAot2IL.Metadata;
using NativeAot2IL.Rtr;

namespace NativeAot2IL;

internal static class Program
{
    static void Main(string[] args)
    {
        ConsoleLogger.Initialize();
        LibLogger.Writer = new LibLogWriter();
        ConsoleLogger.ShowVerbose = LibLogger.ShowVerbose = true;
        Logger.InfoNewline("Starting application...", "Main");
        
        if(args.Length == 0)
        {
            Logger.ErrorNewline("No arguments provided.", "Main");
            return;
        }
        
        var peFilePath = args[0];
        Logger.InfoNewline($"Processing PE file: {peFilePath}", "Main");
        
        var contents = File.ReadAllBytes(peFilePath);
        using var memStream = new MemoryStream(contents, 0, contents.Length, true, true);
        using var pe = new PE.PE(memStream);

        var searcher = new BinarySearcher(pe);
        var headerAddr = searcher.FindRtrHeader();

        var header = pe.ReadReadableAtVirtualAddress<ReadyToRunDirectory>(headerAddr);
        Logger.InfoNewline($"Found ReadyToRun header at virtual address: 0x{headerAddr:X}", "Main");
        
        foreach (var headerSection in header.Sections!)
        {
            Logger.InfoNewline($"Section: {headerSection.SectionType}, Start: 0x{headerSection.Start:x8}, End: 0x{headerSection.End:x8} (length 0x{(headerSection.End - headerSection.Start):x8}), Flags: {headerSection.Flags}", "Main");
        }

        var metadataSection = header.Sections.Single(s => s.SectionType == RtrSectionType.EmbeddedMetadata);

        var start = pe.MapVirtualAddressToRaw(metadataSection.Start);
        var end = pe.MapVirtualAddressToRaw(metadataSection.End);
        
        var metadataBytes = new byte[end - start];
        pe.GetRawBinaryContent().AsSpan((int)start, (int)(end - start)).CopyTo(metadataBytes);
        
        using var metadataStream = new MemoryStream(metadataBytes, 0, metadataBytes.Length, true, true);
        using var metadataReader = new ClassReadingBinaryReader(metadataStream);
        
        var metadataHeader = pe.ReadReadableAtVirtualAddress<MetadataHeader>(metadataSection.Start);
        
        foreach (var scopeDefinitionHandle in metadataHeader.ScopeDefinitionHandles)
        {
            var scopeDefinition = scopeDefinitionHandle.Resolve<ScopeDefinition>(metadataReader);
            Logger.InfoNewline($"Scope Definition: {scopeDefinition}", "Main");
        }
    }
}