namespace NativeAot2IL.Rtr;

[Flags]
public enum RtrFlags
{
    PLATFORM_NEUTRAL_SOURCE     = 0x00000001,   // Set if the original IL assembly was platform-neutral
    SKIP_TYPE_VALIDATION        = 0x00000002,   // Set of methods with native code was determined using profile data
    PARTIAL                     = 0x00000004,
    NONSHARED_PINVOKE_STUBS     = 0x00000008,   // PInvoke stubs compiled into image are non-shareable (no secret parameter)
    EMBEDDED_MSIL               = 0x00000010,   // MSIL is embedded in the composite R2R executable
    COMPONENT                   = 0x00000020,   // This is the header describing a component assembly of composite R2R
    MULTIMODULE_VERSION_BUBBLE  = 0x00000040,   // This R2R module has multiple modules within its version bubble (For versions before version 6.2, all modules are assumed to possibly have this characteristic)
    UNRELATED_R2R_CODE          = 0x00000080,   // This R2R module has code in it that would not be naturally encoded into this module
    PLATFORM_NATIVE_IMAGE       = 0x00000100,   // The owning composite executable is in the platform native format
}