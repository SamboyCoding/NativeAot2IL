namespace NativeAot2IL.Metadata;

public class NamespaceDefinition : ReadableClass
{
    public MetadataHandle ParentScopeOrNamespaceHandle { get; private set; } //NamespaceDefinition or ScopeDefinition
    
    private MetadataHandle _nameHandle;
    public string? Name { get; private set; }
    
    public MetadataHandle[] TypeDefinitionHandles { get; private set; }
    public List<TypeDefinition> TypeDefinitions { get; } = new();
    public MetadataHandle[] TypeForwarderHandles { get; private set; }
    public MetadataHandle[] NamespaceDefinitionHandles { get; private set; }
    public List<NamespaceDefinition> NamespaceDefinitions { get; } = new();

    public override void Read(ClassReadingBinaryReader reader)
    {
        ParentScopeOrNamespaceHandle = reader.ReadMetadataHandleHereNoLock(HandleType.Null); //One of two possible types so we pass null and it reads differently
        _nameHandle = reader.ReadMetadataHandleHereNoLock(HandleType.ConstantStringValue);
        TypeDefinitionHandles = reader.ReadMetadataHandleArrayHereNoLock(HandleType.TypeDefinition);
        TypeForwarderHandles = reader.ReadMetadataHandleArrayHereNoLock(HandleType.TypeForwarder);
        NamespaceDefinitionHandles = reader.ReadMetadataHandleArrayHereNoLock(HandleType.NamespaceDefinition);

        Name = _nameHandle.ResolveString(reader, false);
        
        NamespaceDefinitions.EnsureCapacity(NamespaceDefinitionHandles.Length);
        foreach (var namespaceDefinitionHandle in NamespaceDefinitionHandles)
        {
            NamespaceDefinitions.Add(namespaceDefinitionHandle.Resolve<NamespaceDefinition>(reader, false) ?? throw new InvalidOperationException($"Failed to resolve NamespaceDefinition for handle {namespaceDefinitionHandle}"));
        }
        
        TypeDefinitions.EnsureCapacity(TypeDefinitionHandles.Length);
        foreach (var typeDefinitionHandle in TypeDefinitionHandles)
        {
            TypeDefinitions.Add(typeDefinitionHandle.Resolve<TypeDefinition>(reader, false) ?? throw new InvalidOperationException($"Failed to resolve TypeDefinition for handle {typeDefinitionHandle}"));
        }
    }

    public override string ToString()
    {
        return $"NamespaceDefinition: {Name ?? "<null name>"}. Types: {TypeDefinitions.Count}, TypeForwards: {TypeForwarderHandles.Length}, Sub-Namespaces: {NamespaceDefinitions.Count}";
    }
}