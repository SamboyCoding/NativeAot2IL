namespace NativeAot2IL.Metadata;

public enum AssemblyFlags : uint
{
    /// The assembly reference holds the full (unhashed) public key.
    PublicKey = 0x1,

    /// The implementation of this assembly used at runtime is not expected to match the version seen at compile time.
    Retargetable = 0x100,

    /// Content type mask. Masked bits correspond to values of System.Reflection.AssemblyContentType
    ContentTypeMask = 0x00000e00,

}