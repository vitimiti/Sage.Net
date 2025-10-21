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
