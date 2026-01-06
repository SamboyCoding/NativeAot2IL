using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Text;
using NativeAot2IL.Metadata;

namespace NativeAot2IL;

public class ClassReadingBinaryReader : EndianAwareBinaryReader
{
    /// <summary>
    /// Set this to true to enable storing of amount of bytes read of each readable structure.
    /// </summary>
    public static bool EnableReadableSizeInformation = false;

    private SpinLock PositionShiftLock;

    public bool is32Bit;
    private MemoryStream? _memoryStream;

    public ulong PointerSize => is32Bit ? 4ul : 8ul;

    protected bool _hasFinishedInitialRead;
    private bool _inReadableRead;
    public ConcurrentDictionary<Type, int> BytesReadPerClass = new();


    public ClassReadingBinaryReader(MemoryStream input) : base(input)
    {
        _memoryStream = input;
    }

    public ClassReadingBinaryReader(Stream input) : base(input)
    {
        _memoryStream = null;
    }

    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public long Length => BaseStream.Length;

    public uint ReadCompressedUIntAtRawAddr(long offset, out int bytesRead)
    {
        GetLockOrThrow();

        try
        {
            return ReadCompressedUIntAtRawAddrNoLock(offset, out bytesRead);
        }
        finally
        {
            ReleaseLock();
        }
    }
    
    public uint ReadCompressedUIntHereNoLock() => ReadCompressedUIntAtRawAddrNoLock(-1, out _);

    protected internal uint ReadCompressedUIntAtRawAddrNoLock(long offset, out int bytesRead)
    {
        if (offset >= 0)
            Position = offset;

        //Read first byte
        var b = ReadByte();
        bytesRead = 1;
        if ((b & 1) == 0)
            return (uint)(b >> 1);

        if ((b & 2) == 0)
        {
            var b2 = ReadByte();
            bytesRead = 2;
            return (uint)((b >> 2) | (b2 << 6));
        }
        
        if((b & 4) == 0)
        {
            var b2 = ReadByte();
            var b3 = ReadByte();
            bytesRead = 3;
            return (uint)((b >> 3) | (b2 << 5) | (b3 << 13));
        }
        
        if((b & 8) == 0)
        {
            var b2 = ReadByte();
            var b3 = ReadByte();
            var b4 = ReadByte();
            bytesRead = 4;
            return (uint)((b >> 4) | (b2 << 4) | (b3 << 12) | (b4 << 20));
        }
        
        if((b & 16) == 0)
        {
            bytesRead = 5;
            return ReadUInt32();
        }
        
        throw new Exception($"ReadUnityCompressedUIntAtRawAddrNoLock: Invalid compressed uint format. First byte: {b}");
    }
    
    public int ReadCompressedIntAtRawAddr(long offset, out int bytesRead)
    {
        GetLockOrThrow();

        try
        {
            return ReadCompressedIntAtRawAddrNoLock(offset, out bytesRead);
        }
        finally
        {
            ReleaseLock();
        }
    }

    protected internal int ReadCompressedIntAtRawAddrNoLock(long offset, out int bytesRead)
    {
        if (offset >= 0)
            Position = offset;

        //Read first byte
        var b = ReadByte();
        bytesRead = 1;
        if ((b & 1) == 0)
            return (sbyte)b >> 1;

        //Note only the most significant byte is signed
        if ((b & 2) == 0)
        {
            var b2 = ReadSByte();
            bytesRead = 2;
            return (b >> 2) | (b2 << 6);
        }
        
        if((b & 4) == 0)
        {
            var b2 = ReadByte();
            var b3 = ReadSByte();
            bytesRead = 3;
            return (b >> 3) | (b2 << 5) | (b3 << 13);
        }
        
        if((b & 8) == 0)
        {
            var b2 = ReadByte();
            var b3 = ReadByte();
            var b4 = ReadSByte();
            bytesRead = 4;
            return (b >> 4) | (b2 << 4) | (b3 << 12) | (b4 << 20);
        }
        
        if((b & 16) == 0)
        {
            bytesRead = 5;
            return (int) ReadUInt32(); //This seems odd but this is how the .NET implementation does it
        }
        
        throw new Exception($"ReadUnityCompressedUIntAtRawAddrNoLock: Invalid compressed uint format. First byte: {b}");
    }
    
    public ulong ReadCompressedULongAtRawAddr(long offset, out int bytesRead)
    {
        GetLockOrThrow();

        try
        {
            return ReadCompressedULongAtRawAddrNoLock(offset, out bytesRead);
        }
        finally
        {
            ReleaseLock();
        }
    }

    protected internal ulong ReadCompressedULongAtRawAddrNoLock(long offset, out int bytesRead)
    {
        if (offset >= 0)
            Position = offset;
        
        //Read first byte
        var b = ReadByte();

        if ((b & 31) != 31)
        {
            //Rewind and read as uint
            Position -= 1;
            return ReadCompressedUIntAtRawAddrNoLock(-1, out bytesRead);
        }
        
        if ((b & 32) == 0)
        {
            bytesRead = 9;
            return ReadUInt64();
        }
        
        throw new Exception($"ReadUnityCompressedULongAtRawAddrNoLock: Invalid compressed ulong format. First byte: {b}");
    }
    
    public long ReadCompressedLongAtRawAddr(long offset, out int bytesRead)
    {
        GetLockOrThrow();

        try
        {
            return ReadCompressedLongAtRawAddrNoLock(offset, out bytesRead);
        }
        finally
        {
            ReleaseLock();
        }
    }
    
    protected internal long ReadCompressedLongAtRawAddrNoLock(long offset, out int bytesRead)
    {
        if (offset >= 0)
            Position = offset;
        
        //Read first byte
        var b = ReadByte();

        if ((b & 31) != 31)
        {
            //Rewind and read as int
            Position -= 1;
            return ReadCompressedIntAtRawAddrNoLock(-1, out bytesRead);
        }
        
        if ((b & 32) == 0)
        {
            bytesRead = 9;
            return (long) ReadUInt64(); //This seems odd but this is how the .NET implementation does it
        }
        
        throw new Exception($"ReadUnityCompressedLongAtRawAddrNoLock: Invalid compressed ulong format. First byte: {b}");
    }

    private T InternalReadReadableClass<T>() where T : ReadableClass, new()
    {
        // var t = new T { MetadataVersion = MetadataVersion };
        var t = new T();

        if (!_inReadableRead)
        {
            _inReadableRead = true;
            t.Read(this);
            _inReadableRead = false;
        }
        else
        {
            t.Read(this);
        }

        return t;
    }

    public string ReadStringToNull(ulong offset) => ReadStringToNull((long)offset);

    public virtual string ReadStringToNull(long offset)
    {
        GetLockOrThrow();

        try
        {
            return ReadStringToNullNoLock(offset);
        }
        finally
        {
            ReleaseLock();
        }
    }

    internal string ReadStringToNullNoLock(long offset)
    {
        var builder = new List<byte>();

        if (offset != -1)
            Position = offset;

        try
        {
            byte b;
            while ((b = ReadByte()) != 0)
                builder.Add(b);

            return Encoding.UTF8.GetString(builder.ToArray());
        }
        finally
        {
            var bytesRead = (int)(Position - offset);
            TrackRead<string>(bytesRead);
        }
    }

    public string ReadStringToNullAtCurrentPos()
        => ReadStringToNullNoLock(-1);
    
    public string ReadLengthPrefixedStringHereNoLock()
    {
        var length = ReadCompressedUIntHereNoLock();
        
        if(length > 1000)
            throw new Exception($"ReadLengthPrefixedStringHereNoLock: Unreasonable string length {length} at position {Position - sizeof(uint)}");

        var strBytes = new byte[length];
        ReadExactly(strBytes);

        TrackRead<string>((int)length, false);

        return Encoding.UTF8.GetString(strBytes);
    }
    
    public string ReadLengthPrefixedStringAtRawAddress(long offset)
    {
        GetLockOrThrow();

        try
        {
            if (offset != -1)
                Position = offset;

            return ReadLengthPrefixedStringHereNoLock();
        }
        finally
        {
            ReleaseLock();
        }
    }
    
    public string ReadLengthPrefixedStringAtRawAddressNoLock(long offset)
    {
        if (offset != -1)
            Position = offset;

        return ReadLengthPrefixedStringHereNoLock();
    }

    public byte[] ReadByteArrayAtRawAddress(long offset, int count)
    {
        GetLockOrThrow();

        try
        {
            return ReadByteArrayAtRawAddressNoLock(offset, count);
        }
        finally
        {
            ReleaseLock();
        }
    }

    protected internal byte[] ReadByteArrayAtRawAddressNoLock(long offset, int count)
    {
        if (offset != -1)
            Position = offset;

        if (count == 0)
            return [];

        try
        {
            var ret = new byte[count];
            ReadExactly(ret);

            return ret;
        }
        finally
        {
            TrackRead<byte>(count, false);
        }
    }

    protected internal void GetLockOrThrow()
    {
        var obtained = false;
        PositionShiftLock.Enter(ref obtained);

        if (!obtained)
            throw new Exception("Failed to obtain lock");
    }

    protected internal void ReleaseLock()
    {
        PositionShiftLock.Exit();
    }

    public ulong ReadNUintAtRawAddress(long offset)
    {
        if (offset > Length)
            throw new EndOfStreamException($"ReadNUintAtRawAddress: Offset 0x{offset:X} is beyond the end of the stream (length 0x{Length:X})");

        GetLockOrThrow();

        try
        {
            Position = offset;
            return ReadNUint();
        }
        finally
        {
            ReleaseLock();

            TrackRead<ulong>((int)PointerSize, false);
        }
    }

    public ulong[] ReadNUintArrayAtRawAddress(long offset, int count)
    {
        if (offset > Length)
            throw new EndOfStreamException($"ReadNUintArrayAtRawAddress: Offset 0x{offset:X} is beyond the end of the stream (length 0x{Length:X})");

        var inBounds = offset + count * (int)PointerSize <= Length;
        if (!inBounds)
            throw new EndOfStreamException($"ReadNUintArrayAtRawAddress: Attempted to read {count} pointers (pointer length {PointerSize}) at offset 0x{offset:X}, but this goes beyond the end of the stream (length 0x{Length:X})");

        GetLockOrThrow();

        try
        {
            Position = offset;

            var ret = new ulong[count];

            for (var i = 0; i < count; i++)
            {
                ret[i] = ReadNUint();
            }

            return ret;
        }
        finally
        {
            ReleaseLock();

            var bytesRead = count * (int)PointerSize;
            TrackRead<ulong>(bytesRead, false);
        }
    }
    
    public uint[] ReadUIntArrayAtRawAddress(long offset, int count)
    {
        if (offset > Length)
            throw new EndOfStreamException($"ReadUIntArrayAtRawAddress: Offset 0x{offset:X} is beyond the end of the stream (length 0x{Length:X})");

        var inBounds = offset + count * 4 <= Length;
        if (!inBounds)
            throw new EndOfStreamException($"ReadUIntArrayAtRawAddress: Attempted to read {count} uints (uint length 4) at offset 0x{offset:X}, but this goes beyond the end of the stream (length 0x{Length:X})");

        GetLockOrThrow();

        try
        {
            Position = offset;

            var ret = new uint[count];

            for (var i = 0; i < count; i++)
            {
                ret[i] = ReadUInt32();
            }

            return ret;
        }
        finally
        {
            ReleaseLock();

            var bytesRead = count * 4;
            TrackRead<uint>(bytesRead, false);
        }
    }
    
    public ushort[] ReadUShortArrayAtRawAddress(long offset, int count)
    {
        if (offset > Length)
            throw new EndOfStreamException($"ReadUShortArrayAtRawAddress: Offset 0x{offset:X} is beyond the end of the stream (length 0x{Length:X})");

        var inBounds = offset + count * 2 <= Length;
        if (!inBounds)
            throw new EndOfStreamException($"ReadUShortArrayAtRawAddress: Attempted to read {count} ushorts (ushort length 2) at offset 0x{offset:X}, but this goes beyond the end of the stream (length 0x{Length:X})");

        GetLockOrThrow();

        try
        {
            Position = offset;

            var ret = new ushort[count];

            for (var i = 0; i < count; i++)
            {
                ret[i] = ReadUInt16();
            }

            return ret;
        }
        finally
        {
            ReleaseLock();

            var bytesRead = count * 2;
            TrackRead<ushort>(bytesRead, false);
        }
    }


    /// <summary>
    /// Read a native-sized integer (i.e. 32 or 64 bit, depending on platform) at the current position
    /// </summary>
    public virtual long ReadNInt() => is32Bit ? ReadInt32() : ReadInt64();

    /// <summary>
    /// Read a native-sized unsigned integer (i.e. 32 or 64 bit, depending on platform) at the current position
    /// </summary>
    public virtual ulong ReadNUint() => is32Bit ? ReadUInt32() : ReadUInt64();

    protected void WriteWord(int position, ulong word) => WriteWord(position, (long)word);

    /// <summary>
    /// Used for ELF Relocations.
    /// </summary>
    protected void WriteWord(int position, long word)
    {
        if (_memoryStream == null)
            throw new("WriteWord is not supported in non-memory-backed readers");

        GetLockOrThrow();

        try
        {
            byte[] rawBytes;
            if (is32Bit)
            {
                var value = (int)word;
                rawBytes = BitConverter.GetBytes(value);
            }
            else
            {
                rawBytes = BitConverter.GetBytes(word);
            }

            if (ShouldReverseArrays)
                rawBytes = rawBytes.Reverse();

            if (position > _memoryStream.Length)
                throw new Exception($"WriteWord: Position {position} beyond length {_memoryStream.Length}");

            var count = is32Bit ? 4 : 8;
            if (position + count > _memoryStream.Length)
                throw new Exception($"WriteWord: Writing {count} bytes at {position} would go beyond length {_memoryStream.Length}");

            if (rawBytes.Length != count)
                throw new Exception($"WriteWord: Expected {count} bytes from BitConverter, got {position}");

            try
            {
                _memoryStream.Seek(position, SeekOrigin.Begin);
                _memoryStream.Write(rawBytes, 0, count);
            }
            catch
            {
                Logging.LibLogger.ErrorNewline("WriteWord: Unexpected exception!");
                throw;
            }
        }
        finally
        {
            ReleaseLock();
        }
    }

    public T ReadReadableHereNoLock<T>() where T : ReadableClass, new() => InternalReadReadableClass<T>();

    public T ReadReadable<T>(long offset = -1) where T : ReadableClass, new()
    {
        GetLockOrThrow();

        if (offset >= 0)
            Position = offset;

        var initialPos = Position;

        try
        {
            return InternalReadReadableClass<T>();
        }
        finally
        {
            var bytesRead = (int)(Position - initialPos);
            TrackRead<T>(bytesRead, trackIfFinishedReading: true);

            ReleaseLock();
        }
    }
    
    public T ReadReadableNoLock<T>(long offset = -1) where T : ReadableClass, new()
    {
        if (offset >= 0)
            Position = offset;

        var initialPos = Position;

        try
        {
            return InternalReadReadableClass<T>();
        }
        finally
        {
            var bytesRead = (int)(Position - initialPos);
            TrackRead<T>(bytesRead, trackIfFinishedReading: true);
        }
    }

    public T[] ReadReadableArrayAtRawAddr<T>(long offset, long count) where T : ReadableClass, new()
    {
        var t = new T[count];

        GetLockOrThrow();

        if (offset != -1)
            Position = offset;

        try
        {
            //This handles the actual reading into the array, and tracking read counts, for us.
            FillReadableArrayHereNoLock(t);
        }
        finally
        {
            ReleaseLock();
        }

        return t;
    }
    
    /// <summary>
    /// This reads a "Collection" as defined in the R2R metadata, which is a compressed-uint-length-prefixed array.
    /// </summary>
    public T[] ReadReadableCollectionHereNoLock<T>() where T : ReadableClass, new()
    {
        //Read count
        var count = ReadCompressedUIntAtRawAddrNoLock(-1, out _);

        var t = new T[count];

        //This handles the actual reading into the array, and tracking read counts, for us.
        FillReadableArrayHereNoLock(t);

        return t;
    }
    
    public byte[] ReadByteCollectionHereNoLock()
    {
        //Read count
        var count = ReadCompressedUIntAtRawAddrNoLock(-1, out _);

        var ret = new byte[count];

        ReadExactly(ret);

        TrackRead<byte>((int)count, false);

        return ret;
    }

    public void FillReadableArrayHereNoLock<T>(T[] array, int startOffset = 0) where T : ReadableClass, new()
    {
        var initialPos = Position;

        try
        {
            var i = startOffset;
            for (; i < array.Length; i++)
            {
                array[i] = InternalReadReadableClass<T>();
            }
        }
        finally
        {
            var bytesRead = (int)(Position - initialPos);
            TrackRead<T>(bytesRead, trackIfFinishedReading: true);
        }
    }

    public MetadataHandle ReadMetadataHandleHereNoLock(HandleType knownType)
    {
        var handle = new MetadataHandle();
        if (knownType != HandleType.Null)
        {
            handle.ReadWithKnownType(this, knownType);
            return handle;
        }

        handle.ReadWithUnknownType(this);
        return handle;
    }
    
    public MetadataHandle[] ReadMetadataHandleArrayHereNoLock(HandleType knownType) 
    {
        //Read count
        var count = ReadCompressedUIntAtRawAddrNoLock(-1, out _);

        var handles = new MetadataHandle[count];

        var initialPos = Position;

        try
        {
            for (var i = 0; i < count; i++)
            {
                handles[i] = ReadMetadataHandleHereNoLock(knownType);
            }
        }
        finally
        {
            var bytesRead = (int)(Position - initialPos);
            TrackRead<MetadataHandle>(bytesRead, trackIfFinishedReading: true);
        }

        return handles;
    }

    public void TrackRead<T>(int bytesRead, bool trackIfInReadableRead = true, bool trackIfFinishedReading = false)
    {
        if (!EnableReadableSizeInformation)
            return;

        if (!trackIfInReadableRead && _inReadableRead)
            return;

        if (_hasFinishedInitialRead && !trackIfFinishedReading)
            return;

        BytesReadPerClass[typeof(T)] = BytesReadPerClass.GetValueOrDefault(typeof(T)) + bytesRead;
    }
}
