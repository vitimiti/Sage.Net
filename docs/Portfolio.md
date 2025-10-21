# The Recreation of the SAGE Engine in Modern Dotnet

This document is here to document my process to recreate the SAGE engine as published by
[EA on their official GitHub](https://github.com/electronicarts/CnC_Generals_Zero_Hour).

The objective is to create a full system without the use of external libraries. While libraries
like the ones in [Silk.NET](https://github.com/dotnet/Silk.NET) and the ones in [Veldrid](https://github.com/veldrid/veldrid)
would speed up the process, and projects like [OpenSAGE](https://github.com/OpenSAGE) already exist,
this is meant for my personal development of programming skills in dotnet, forcing me to better understand
graphics programming and the development of low level systems in dotnet, such as marshalling and p/invoke.

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
