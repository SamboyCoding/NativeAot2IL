# NativeAot2IL

[Cpp2IL](https://github.com/SamboyCoding/Cpp2IL)-style dumper/eventually-decompiler-maybe for NativeAOT binaries.

Currently might as well be nonfunctional, posted only for visibility and information purposes. 
Definitely will only work on .NET 10 Windows binaries, likely x64-only, and is not of any use unless you're poking around with a debugger attached.

## Features
- Finds R2R header
- Finds all R2R Sections
- Parses very basic information out of the metadata section
- More features coming soon™

## Credits

- A bunch of this code is just ripped straight from Cpp2IL (which is, of course, my project anyway)
- Data formats and parsing code is a mismash of various sources. To call a few out here:
  - The original [metadata reader c# file](https://github.com/dotnet/runtime/blob/main/src/coreclr/tools/Common/Internal/Metadata/NativeFormat/NativeFormatReaderGen.cs) on the dotnet/runtime repo
  - [This](https://blog.washi.dev/posts/recovering-nativeaot-metadata/) blog post by Washi1337 and the accompanying [ghidra plugin](https://github.com/Washi1337/ghidra-nativeaot)
  - [Naotilus](https://github.com/BadRyuner/Naotilus) by BadRyuner
  - Both non-runtime repos are MIT licensed, and the dotnet repo has its own licensing.