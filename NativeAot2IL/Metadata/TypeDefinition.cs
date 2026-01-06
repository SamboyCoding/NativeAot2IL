using System.Reflection;
using System.Reflection.Metadata;

namespace NativeAot2IL.Metadata;

public class TypeDefinition : ReadableClass
{
    public TypeAttributes Flags { get; private set; }

    /// One of: TypeDefinition, TypeReference, TypeSpecification
    public MetadataHandle BaseType { get; private set; }
    public MetadataHandle NamespaceDefinition { get; private set; }

    private MetadataHandle _nameHandle;
    public string? Name { get; private set; }
    
    public uint Size { get; private set; }
    public ushort PackingSize { get; private set; }
    public MetadataHandle EnclosingType { get; private set; }
    public MetadataHandle[] NestedTypes { get; private set; }
    public MetadataHandle[] Methods { get; private set; }
    public MetadataHandle[] Fields { get; private set; }
    public MetadataHandle[] Properties { get; private set; }
    public MetadataHandle[] Events { get; private set; }
    public MetadataHandle[] GenericParameters { get; private set; }

    /// One of: TypeDefinition, TypeReference, TypeSpecification
    public MetadataHandle[] Interfaces { get; private set; }
    public MetadataHandle[] CustomAttributes { get; private set; }
    
    public override void Read(ClassReadingBinaryReader reader)
    {
        Flags = (TypeAttributes)reader.ReadCompressedUIntHereNoLock();
        BaseType = reader.ReadMetadataHandleHereNoLock(HandleType.Null); //One of three possible types so we pass null and it reads differently
        NamespaceDefinition = reader.ReadMetadataHandleHereNoLock(HandleType.NamespaceDefinition);
        
        _nameHandle = reader.ReadMetadataHandleHereNoLock(HandleType.ConstantStringValue);
        
        Size = reader.ReadCompressedUIntHereNoLock();
        PackingSize = (ushort)reader.ReadCompressedUIntHereNoLock();
        EnclosingType = reader.ReadMetadataHandleHereNoLock(HandleType.TypeDefinition);
        NestedTypes = reader.ReadMetadataHandleArrayHereNoLock(HandleType.TypeDefinition);
        Methods = reader.ReadMetadataHandleArrayHereNoLock(HandleType.Method);
        Fields = reader.ReadMetadataHandleArrayHereNoLock(HandleType.Field);
        Properties = reader.ReadMetadataHandleArrayHereNoLock(HandleType.Property);
        Events = reader.ReadMetadataHandleArrayHereNoLock(HandleType.Event);
        GenericParameters = reader.ReadMetadataHandleArrayHereNoLock(HandleType.GenericParameter);
        Interfaces = reader.ReadMetadataHandleArrayHereNoLock(HandleType.Null); //One of three possible types so we pass null and it reads differently
        CustomAttributes = reader.ReadMetadataHandleArrayHereNoLock(HandleType.CustomAttribute);
        
        Name = _nameHandle.ResolveString(reader, false);
    }

    public override string ToString()
    {
        return $"TypeDefinition: {Name}, Flags={Flags}, Size={Size}, PackingSize={PackingSize}, Methods={Methods.Length}, Fields={Fields.Length}, Properties={Properties.Length}, Events={Events.Length}, NestedTypes={NestedTypes.Length}";
    }
}