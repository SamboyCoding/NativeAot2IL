namespace NativeAot2IL;

public abstract class ReadableClass
{
    // internal float MetadataVersion { get; set; }
    //
    // protected bool IsAtLeast(float vers) => MetadataVersion >= vers;
    // protected bool IsLessThan(float vers) => MetadataVersion < vers;
    // protected bool IsAtMost(float vers) => MetadataVersion <= vers;
    // protected bool IsNot(float vers) => Math.Abs(MetadataVersion - vers) > 0.001f;
    // protected bool Is(float vers) => Math.Abs(MetadataVersion - vers) < 0.001f;

    public abstract void Read(ClassReadingBinaryReader reader);
}
