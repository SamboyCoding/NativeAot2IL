namespace NativeAot2IL.Rtr;

public enum RtrSectionType
{
    CompilerIdentifier = 0x64, //100
    ImportSections = 0x65,
    RuntimeFunctions = 0x66,
    MethodDefEntryPoints = 0x67,
    ExceptionInfo = 0x68,
    DebugInfo = 0x69,
    DelayLoadMethodCallThunks = 0x6A,

    // 107 used by an older format of AvailableTypes
    AvailableTypes = 0x6C,
    InstanceMethodEntryPoints = 0x6D,
    InliningInfo = 0x6E, // Added in V2.1, deprecated in 4.1
    ProfileDataInfo = 0x6F, // Added in V2.2
    ManifestMetadata = 0x70, // Added in V2.3
    AttributePresence = 0x71, // Added in V3.1
    InliningInfo2 = 0x72, // Added in V4.1
    ComponentAssemblies = 0x73, // Added in V4.1
    OwnerCompositeExecutable = 0x74, // Added in V4.1
    PgoInstrumentationData = 0x75, // Added in V5.2
    ManifestAssemblyMvids = 0x76, // Added in V5.3
    CrossModuleInlineInfo = 0x77, // Added in V6.2
    HotColdMap = 0x78, // Added in V8.0
    MethodIsGenericMap = 0x79, // Added in V9.0
    EnclosingTypeMap = 0x7A, // Added in V9.0
    TypeGenericInfoMap = 0x7B, // Added in V9.0

    //
    // NativeAOT ReadyToRun sections
    //
    StringTable = 0xC8, // 200, Unused
    GCStaticRegion = 0xC9,
    ThreadStaticRegion = 0xCA,

    // Unused = 0xCB,
    TypeManagerIndirection = 0xCC,
    EagerCctor = 0xCD,
    FrozenObjectRegion = 0xCE,
    DehydratedData = 0xCF,
    ThreadStaticOffsetRegion = 0xD0,

    // 209 is unused - it was used by ThreadStaticGCDescRegion
    // 210 is unused - it was used by ThreadStaticIndex
    // 211 is unused - it was used by LoopHijackFlag
    ImportAddressTables = 0xD4,
    ModuleInitializerList = 0xD5,

    // Sections 300 - 399 are reserved for RhFindBlob backwards compatibility, so these values are ReflectionMapBlob + 300
    TypeMap = 0x12D, //300
    ArrayMap = 0x12E,
    GenericInstanceMap = 0x12F, // unused
    GenericParameterMap = 0x130, // unused
    BlockReflectionTypeMap = 0x131,
    InvokeMap = 0x132,
    VirtualInvokeMap = 0x133,
    CommonFixupsTable = 0x134,
    FieldAccessMap = 0x135,
    CCtorContextMap = 0x136,
    DiagGenericInstanceMap = 0x137, // unused
    DiagGenericParameterMap = 0x138, // unused
    EmbeddedMetadata = 0x139,
    DefaultConstructorMap = 0x13A,
    UnboxingAndInstantiatingStubMap = 0x13B,
    StructMarshallingStubMap = 0x13C,
    DelegateMarshallingStubMap = 0x13D,
    GenericVirtualMethodTable = 0x13E,
    InterfaceGenericVirtualMethodTable = 0x13F,

    // Reflection template types/methods blobs:
    TypeTemplateMap = 0x141,
    GenericMethodsTemplateMap = 0x142,
    DynamicInvokeTemplateData = 0x143,
    BlobIdResourceIndex = 0x144,
    BlobIdResourceData = 0x145,
    BlobIdStackTraceEmbeddedMetadata = 0x146,
    BlobIdStackTraceMethodRvaToTokenMapping = 0x147,

    //Native layout blobs:
    NativeLayoutInfo = 0x14A,
    NativeReferences = 0x14B,
    GenericsHashtable = 0x14C,
    NativeStatics = 0x14D,
    StaticsInfoHashtable = 0x14E,
    GenericMethodsHashtable = 0x14F,
    ExactMethodInstantiationsHashtable = 0x150,
}