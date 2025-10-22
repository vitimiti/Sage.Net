# The Recreation of the SAGE Engine in Modern Dotnet

This document is here to document my process to recreate the SAGE engine as published by
[EA on their official GitHub](https://github.com/electronicarts/CnC_Generals_Zero_Hour).

The objective is to create a full system without the use of external libraries. While libraries
like the ones in [Silk.NET](https://github.com/dotnet/Silk.NET) and the ones in [Veldrid](https://github.com/veldrid/veldrid)
would speed up the process, and projects like [OpenSAGE](https://github.com/OpenSAGE) already exist,
this is meant for my personal development of programming skills in dotnet, forcing me to better understand
graphics programming and the development of low level systems in dotnet, such as marshalling and p/invoke.

We will be using the `GeneralsMD` (the expansion) code, and assuming all improvements are, in fact, improvements.

## Table of Contents

- [The Recreation of the SAGE Engine in Modern Dotnet](#the-recreation-of-the-sage-engine-in-modern-dotnet)
  - [Table of Contents](#table-of-contents)
  - [The "Easiest" Start](#the-easiest-start)
    - [Preparation](#preparation)
    - [Extensions of the Core Language](#extensions-of-the-core-language)
    - [RefPack Codex](#refpack-codex)

## The "Easiest" Start

Because they don't depend on anything else, the easiest place to start would be the compression libraries.
They are not all actually used in the actual games, but I will implement all of them regardless, because
that way the engine can be more future proof and usable outside of the original games. I may even make my own.

As such, we will need both a library that can be used by the engine and by a CLI and GUI tool to do the compression.
We start with the commands:

```shell
dotnet new classlib -o Sage.Net.Compression
dotnet sln add Sage.Net.Compression -s Compression
```

We will be using solution folder to better organize common functionality.

Now, there are multiple libraries we will need to implement to actually have the core functionality. In the style of
`ZLibStream`, the official ZLib compression/decompression system in dotnet; we will be only exposing streams for each
compression algorithm.

I will be starting with the RefPack compression algorithm, as I find it the easiest to implement, and we will be using
the existing `ZLibStream` for ZLib. All in all, we require the following:

- EAC
  - RefPack
  - BinaryTree
  - HuffmanWithRunlength
- NoxCompressor
  - LightZHL
- ZLib

### Preparation

We proceed to create the RefPack algorithm and will add its dependency to the `Sage.Net.Compression` library, as it will
be used by it:

```shell
dotnet new classlib -o Sage.Net.Compression.Eac.RefPack
dotnet sln add Sage.Net.Compression.Eac.RefPack -s Compression/Eac
dotnet add reference --project Sage.Net.Compression Sage.Net.Compression.Eac.RefPack
```

The files that, in conjuction, form the RefPack codex, are defined in the original code at:

- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/codex.h`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/gimex.h`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/refcodex.h`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/refabout.cpp`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/refdecode.cpp`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/refencode.cpp`

Of these files, because we want to implement the system in a dotnet idiomatic manner, we are only interested in
implementing the reading and writing in big-endian from `gimex.h` and the `refdecode.cpp` and `refencode.cpp` files
that hold the decoding and encoding algorithms, respectively.

Dotnet has systems to read and write little and big endian bytes, but it may require more than 1 command in some occasions.
For this, we will be creating an extensions library:

```shell
dotnet new classlib -o Sage.Net.Extensions
dotnet sln add Sage.Net.Extensions
dotnet add reference --project Sage.Net.Compression.Eac.RefPack Sage.Net.Extensions
```

### Extensions of the Core Language

Okay, so we are going to need our first extension. We know for a FACT that we will be using streams
for compression/decompression to make this system idiomatic. As such, we will be dealing with the
`BinaryReader` and `BinaryWriter` classes quite a lot to read and write bytes.

The `BinaryReader` class will be used for the `REF_is` method, which requires two considerations:

1. `BinaryReader` CANNOT read big-endian data and requires further action with `BinaryPrimitives`.
2. `BinaryReader` will take in an `Encoding` value, and the original engine uses the
  [Windows ANSI encoding (page 1252)](https://en.wikipedia.org/wiki/Windows-1252)

Both of these limitations can be resolved through the `Sage.Net.Extensions` library, so we will work on that first.

For the first problem, we can add as many options as the `GIMEX` system has, which is for 2, 3 and 4 bytes reading and writing. These are defined in the `gimex.h` header as
`static __inline unsigned int ggetm(const void *src, int bytes)` and
`static __inline void gputm(void *dst, unsigned int data, int bytes)` respectively.

We will also add extensions to read and write specifically in little-endian, regardless of the machine, defined
in the `gimex.h` header as `static __inline unsigned int ggeti(const void *src, int bytes)` and
`static __inline void gputi(void *dst, unsigned int data, int bytes)` respectively and, again; up to 4 bytes.

We will define the equivalents in the
[`BinaryReaderExtensions`](../Sage.Net.Extensions/BinaryReaderExtensions.cs) static class first.
This defines the reading and writing of 2, 3 and 4 big-endian and little-endian bytes in a dotnet idiomatic way:

```csharp
using BinaryReader reader = new(myStream1);
var value = reader.ReadUInt16BigEndian();

using BinaryWriter writer = new(myStream2);
reader.WriteUInt16BigEndian(value);
```

```csharp
using BinaryReader reader = new(myStream1);
var value = reader.ReadUInt16LittleEndian();

using BinaryWriter writer = new(myStream2);
writer.WriteUInt16LittleEndian(value);
```

With this, we must have into mind that, as estated above, the engine was using ANSI characters for their configuration files.
These are legacy characters that aren't found on the standard `Encoding` class, and we will be resolving this by creating
our own extension in [`LegacyEncodings`](../Sage.Net.Extensions/LegacyEncodings.cs). This is accomplish trough the
[`Encoding.GetEncoding`](https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.getencoding?view=net-9.0)
method in dotnet, and by registering it on first usage through the `LegacyEncodings` static constructor with the
[`Encoding.RegisterProvider`](https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.registerprovider?view=net-9.0)
method.

With this, we will be able to create `BinaryReader`s and `BinaryWriter`s by using their corresponding constructors,
[`BinaryReader(Stream, Encoding, Boolean)`](https://learn.microsoft.com/en-us/dotnet/api/system.io.binaryreader.-ctor?view=net-9.0#system-io-binaryreader-ctor(system-io-stream-system-text-encoding-system-boolean))
and [`BinaryWriter(Stream, Encoding, Boolean)`](https://learn.microsoft.com/en-us/dotnet/api/system.io.binarywriter.-ctor?view=net-9.0#system-io-binarywriter-ctor(system-io-stream-system-text-encoding-system-boolean))
constructors respecting the original encoding of the bytes. This is important to read the original strings and to
save strings in a way that is compatible with the original engine's expectations.

Now we are ready to start with the first codex.

### RefPack Codex

The RefPack codex is the preferred compression algorithm within the engine. And while it is not used widely and the other
codexes are not really found in the original games, I intend to allow future usage of this engine to support all possible
systems that may have existed within the original engine.

But with this being the preferred system, we will start here. We want to create a `RefPackStream` in the style of the
existing [`ZLibStream`](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.zlibstream?view=net-9.0)
to help users better understand how to compress and decompress data within the class. In short, `Read` will be
used to decompress and `Write` to compress data.

To recap, the files we are interested in for this codex are:

- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/codex.h`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/gimex.h`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/refcodex.h`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/refabout.cpp`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/refdecode.cpp`
- `CnC_Generals_Zero_Hour/GeneralsMD/Code/Libraries/Source/Compression/EAC/refencode.cpp`

We have now implemented the gimex system, and we can ignore the codex interface due to our idiomatic requirements. We will
also be ignoring the `refabout` data due to, again, the idiomatic requirements. Version information about the library
can be retrieved through the assembly's version, the same with the description.

The first thing to look at will be the following method (comments added for clarity):

```c++
// Check if a given pointer of any type of data is valid RefPack compressed data
bool GCALL REF_is(const void *compresseddata)
{
    bool ok=false; // Initially consider the data to be invalid
    int packtype=ggetm(compresseddata,2); // Get 2 bytes in big-endian format, the data header - This is unsafe, we will add safety to this operation.

    if (packtype==0x10fb
     || packtype==0x11fb
     || packtype==0x90fb
     || packtype==0x91fb)
        ok = true; // If the header bytes are any of the above values, we are reading a valid RefPack

    return(ok);
}
```

We will start by creating an internal class that will deal with the decoding and an internal class that will deal with the
encoding. Both of these classes will NOT work on buffers but rather the `BinaryReader` or `BinaryWriter` classes that the
`RefPackStream` will work with.

We will start with `refdecode.cpp` in the [`Decode`](../Sage.Net.Compression.Eac.RefPack/Internals/Decode.cs) static class.
For this, we need to understand the process of the decoding algorithm. The first step will be to transliterate the
`bool GCALL REF_is(const void *compresseddata)` static method that is used to check if a set of data is valid
RefPack compressed data or not. This method will read 2 bytes off the `compresseddata` and (without safety checks),
checks if these bytes are `0x10FB`, `0x11FB`, `0x90FB` or `0x91FB`. These just seem to be magic numbers, so we will
have to just deal with them.

We will be implemented this as a static, internal method defined as
`internal static bool IsValidRefPackStream(BinaryReader reader)`. We will be adding a safety check without
exceptions to prevent reading on streams that aren't 2 bytes long. This method will also restore the previous
position, avoiding advancing the pointer in the base stream.

```csharp
internal static bool IsValidRefPackStream(BinaryReader reader)
{
    if (reader.BaseStream.Length < 2)
    {
        return false; // Not enough data to read the header
    }

    // Save the current position to restore later
    var currentPosition = reader.BaseStream.Position;
    try
    {
        // Read the first two bytes as big-endian
        _ = reader.BaseStream.Seek(0, SeekOrigin.Begin);
        var headerMagic = reader.ReadUInt16BigEndian();

        // Check if the header matches any of the known RefPack signatures
        return headerMagic is 0x10FB or 0x11FB or 0x90FB or 0x91FB;
    }
    finally
    {
        // Restore the original position
        _ = reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
    }
}
```

The next element will be a size check, to be able to retrieve the size of the encoded data once it
has been decoded.

This is done in the method following method (comments added for clarification):

```c++
int GCALL REF_size(const void *compresseddata)
{
    int len=0; // The length stored
    int packtype=ggetm(compresseddata,2); // The magic number of the header
    int ssize=(packtype&0x8000)?4:3; // Whether the size is stored in 4 or 3 bytes, with a flag check
    // This flag checks whether the magic number is one of the 0x9(...) headers

    // Check if the end of the magic number is 11FB
    if (packtype&0x100)     /* 11fb */
    {
        // This is getting 3/4 bytes big-endian at position 0 + 2 + ssize
        len = ggetm((char *)compresseddata+2+ssize,ssize);
    }
    // Or 10FB
    else                    /* 10fb */
    {
        // This is getting 3/4 bytes big-endian at position 0 + 2
        len = ggetm((char *)compresseddata+2,ssize);
    }

    // Return the length after getting it
    return(len);
}
```

This can be very easily implemented with the following static method:

```csharp
internal static int RetrieveDecompressedRefPackStreamSize(BinaryReader reader)
{
    // Save the current position to restore later
    var currentPosition = reader.BaseStream.Position;
    try
    {
        // Read the first two bytes as big-endian
        _ = reader.BaseStream.Seek(0, SeekOrigin.Begin);
        var headerMagic = reader.ReadUInt16BigEndian();

        // Check if the header starts with 0x9 or 0x1
        var isOx9 = (headerMagic & 0x8000) != 0;

        // Determine the size of the decompressed data based on the header
        var sizeBytes = isOx9 ? 4 : 3;

        // Skip the size bytes if it's version 0x(.)1(..)
        var shouldSkip = (headerMagic & 0x1000) != 0;
        if (shouldSkip)
        {
            _ = reader.BaseStream.Seek(sizeBytes, SeekOrigin.Current);
        }

        // Read and return the decompressed size
        return sizeBytes == 3 ? (int)reader.ReadUInt24BigEndian() : (int)reader.ReadUInt32BigEndian();
    }
    finally
    {
        // Restore the original position
        _ = reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
    }
}
```
