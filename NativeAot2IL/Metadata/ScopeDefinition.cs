namespace NativeAot2IL.Metadata;

public class ScopeDefinition : ReadableClass
{
    public AssemblyFlags Flags { get; private set; }
    public MetadataHandle NameHandle { get; private set; } //ConstantStringValue
    public string? Name { get; private set; }
    public AssemblyHashAlgorithm HashAlgorithm { get; private set; }
    public ushort MajorVersion { get; private set; }
    public ushort MinorVersion { get; private set; }
    public ushort BuildNumber { get; private set; }
    public ushort RevisionNumber { get; private set; }
    public byte[] PublicKey { get; private set; }
    public MetadataHandle CultureHandle { get; private set; } //ConstantStringValue
    public string? Culture { get; private set; }
    public MetadataHandle RootNamespaceDefinitionHandle { get; private set; } //NamespaceDefinition
    public NamespaceDefinition RootNamespaceDefinition { get; private set; }
    public MetadataHandle EntryPoint { get; private set; } //QualifiedMethod
    public MetadataHandle GlobalModuleType { get; private set; } //TypeDefinition
    public MetadataHandle[] CustomAttributes { get; private set; } //CustomAttribute
    public MetadataHandle ModuleNameHandle { get; private set; } //ConstantStringValue
    public string? ModuleName { get; private set; }
    public byte[] Mvid { get; private set; }
    public MetadataHandle[] ModuleCustomAttributes { get; private set; } //CustomAttribute
    
    public override void Read(ClassReadingBinaryReader reader)
    {
        Flags = (AssemblyFlags)reader.ReadCompressedUIntHereNoLock();
        NameHandle = reader.ReadMetadataHandleHereNoLock(HandleType.ConstantStringValue);
        HashAlgorithm = (AssemblyHashAlgorithm)reader.ReadCompressedUIntHereNoLock();
        MajorVersion = (ushort) reader.ReadCompressedUIntHereNoLock();
        MinorVersion = (ushort) reader.ReadCompressedUIntHereNoLock();
        BuildNumber = (ushort) reader.ReadCompressedUIntHereNoLock();
        RevisionNumber = (ushort) reader.ReadCompressedUIntHereNoLock();
        PublicKey = reader.ReadByteCollectionHereNoLock();
        CultureHandle = reader.ReadMetadataHandleHereNoLock(HandleType.ConstantStringValue);
        RootNamespaceDefinitionHandle = reader.ReadMetadataHandleHereNoLock(HandleType.NamespaceDefinition);
        EntryPoint = reader.ReadMetadataHandleHereNoLock(HandleType.QualifiedMethod);
        GlobalModuleType = reader.ReadMetadataHandleHereNoLock(HandleType.TypeDefinition);
        CustomAttributes = reader.ReadMetadataHandleArrayHereNoLock(HandleType.CustomAttribute);
        ModuleNameHandle = reader.ReadMetadataHandleHereNoLock(HandleType.ConstantStringValue);
        Mvid = reader.ReadByteCollectionHereNoLock();
        ModuleCustomAttributes = reader.ReadMetadataHandleArrayHereNoLock(HandleType.CustomAttribute);

        Name = NameHandle.ResolveString(reader, false);
        Culture = CultureHandle.ResolveString(reader, false);
        ModuleName = ModuleNameHandle.ResolveString(reader, false);
        
        RootNamespaceDefinition = RootNamespaceDefinitionHandle.Resolve<NamespaceDefinition>(reader, false) ?? throw new InvalidOperationException("RootNamespaceDefinition cannot be null");
    }

    public override string ToString()
    {
        return $"ScopeDefinition: {Name}, Version={MajorVersion}.{MinorVersion}.{BuildNumber}.{RevisionNumber}, ModuleName={ModuleName}, RootNamespace=({RootNamespaceDefinition})";
    }
}