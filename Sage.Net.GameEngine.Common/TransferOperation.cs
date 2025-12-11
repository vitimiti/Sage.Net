// -----------------------------------------------------------------------
// <copyright file="TransferOperation.cs" company="Sage.Net">
// A transliteration and update of the CnC Generals (Zero Hour) engine and games with mod-first support.
// Copyright (C) 2025 Sage.Net Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see https://www.gnu.org/licenses/.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Sage.Net.BaseTypes;
using Sage.Net.GameEngine.Common.Exceptions.TransferExceptions;

namespace Sage.Net.GameEngine.Common;

/// <summary>The base class for transfer operations.</summary>
public abstract class TransferOperation
{
    /// <summary>Gets or sets the <see cref="TransferMode"/> of the transfer operation.</summary>
    public TransferMode TransferMode { get; protected set; } = TransferMode.Invalid;

    /// <summary>Gets or sets the identifier of the transfer operation.</summary>
    public string Identifier { get; protected set; } = string.Empty;

    /// <summary>Gets or sets the <see cref="TransferOptions"/> of the transfer operation.</summary>
    [SuppressMessage(
        "StyleCop.CSharp.LayoutRules",
        "SA1513:Closing brace should be followed by blank line",
        Justification = "This is a false positive."
    )]
    [SuppressMessage(
        "csharpsquid",
        "S2325:Methods and properties that don't access instance data should be static",
        Justification = "This is a false positive."
    )]
    public TransferOptions Options { get; set; } = TransferOptions.None;

    /// <summary>Adds the given options.</summary>
    /// <param name="options">The <see cref="TransferOptions"/> to add.</param>
    public void AddOptions(TransferOptions options) =>
        Options = new TransferOptions(BitUtility.BitSet((uint)Options, (uint)options));

    /// <summary>Clears the given options.</summary>
    /// <param name="options">The <see cref="TransferOptions"/> to clear.</param>
    public void ClearOptions(TransferOptions options) =>
        Options = new TransferOptions(BitUtility.BitClear((uint)Options, (uint)options));

    /// <summary>Opens the data transfer operation.</summary>
    /// <param name="identifier">A <see cref="string"/> with an identifier for the data transfer operation.</param>
    public virtual void Open(string identifier) => Identifier = identifier;

    /// <summary>Closes the data transfer operation.</summary>
    public abstract void Close();

    /// <summary>Starts a block of data transfer.</summary>
    /// <returns>A <see cref="int"/> with the size of the transferred data in the block.</returns>
    public abstract int BeginBlock();

    /// <summary>Closes a block of data transfer.</summary>
    public abstract void EndBlock();

    /// <summary>Skips an amount of data.</summary>
    /// <param name="dataSize">An <see cref="int"/> with the size of the data to skip.</param>
    public abstract void Skip(int dataSize);

    /// <summary>Transfer a version number in relation to the current version number.</summary>
    /// <param name="versionData">A ref value to a <see cref="byte"/> with the version number to transfer.</param>
    /// <param name="currentVersion">A <see cref="byte"/> with the current version number.</param>
    /// <exception cref="TransferInvalidVersionException">When the resulting <paramref name="versionData"/> value is higher than the <paramref name="currentVersion"/> value.</exception>
    public void TransferVersion(ref byte versionData, byte currentVersion)
    {
        TransferStruct(ref currentVersion);
        if (versionData <= currentVersion)
        {
            return;
        }

        var message = $"Unknown version '{versionData}' should be no higher than '{currentVersion}'.";
        Debug.Fail(message);
        throw new TransferInvalidVersionException(message);
    }

    /// <summary>Transfer an <see cref="sbyte"/> value.</summary>
    /// <param name="sbyteData">A ref value to an <see cref="sbyte"/> with the data to transfer.</param>
    public void TransferSByte(ref sbyte sbyteData) => TransferStruct(ref sbyteData);

    /// <summary>Transfer a <see cref="byte"/> value.</summary>
    /// <param name="byteData">A ref value to a <see cref="byte"/> with the data to transfer.</param>
    public void TransferByte(ref byte byteData) => TransferStruct(ref byteData);

    /// <summary>Transfer a <see cref="bool"/> value.</summary>
    /// <param name="boolData">A ref value to a <see cref="bool"/> with the data to transfer.</param>
    public void TransferBool(ref bool boolData) => TransferStruct(ref boolData);

    /// <summary>Transfer a <see cref="int"/> value.</summary>
    /// <param name="int32Data">A ref value to a <see cref="int"/> with the data to transfer.</param>
    public void TransferInt32(ref int int32Data) => TransferStruct(ref int32Data);

    /// <summary>Transfer a <see cref="long"/> value.</summary>
    /// <param name="int64Data">A ref value to a <see cref="long"/> with the data to transfer.</param>
    public void TransferInt64(ref long int64Data) => TransferStruct(ref int64Data);

    /// <summary>Transfer a <see cref="uint"/> value.</summary>
    /// <param name="uint32Data">A ref value to a <see cref="uint"/> with the data to transfer.</param>
    public void TransferUInt32(ref uint uint32Data) => TransferStruct(ref uint32Data);

    /// <summary>Transfer a <see cref="short"/> value.</summary>
    /// <param name="int16Data">A ref value to a <see cref="short"/> with the data to transfer.</param>
    public void TransferInt16(ref short int16Data) => TransferStruct(ref int16Data);

    /// <summary>Transfer a <see cref="ushort"/> value.</summary>
    /// <param name="uint16Data">A ref value to a <see cref="ushort"/> with the data to transfer.</param>
    public void TransferUInt16(ref ushort uint16Data) => TransferStruct(ref uint16Data);

    /// <summary>Transfer a <see cref="float"/> value.</summary>
    /// <param name="float32Data">A ref value to a <see cref="float"/> with the data to transfer.</param>
    public void TransferSingle(ref float float32Data) => TransferStruct(ref float32Data);

    /// <summary>Transfer a marker label.</summary>
    /// <param name="labelData">A ref value to a <see cref="string"/> with the marker label.</param>
    /// <remarks>By default, this is a no-op.</remarks>
    public virtual void TransferMarkerLabel(ref string labelData) { }

    /// <summary>Transfer a <see cref="string"/> value with a given encoding.</summary>
    /// <param name="stringData">A ref value to a <see cref="string"/> with the data to transfer.</param>
    /// <param name="encoding">The <see cref="Encoding"/> to process the <paramref name="stringData"/>.</param>
    public void TransferString(ref string stringData, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        var bytes = encoding.GetBytes(stringData);
        unsafe
        {
            fixed (byte* bytesPtr = bytes)
            {
                TransferCore(bytesPtr, bytes.Length);
                stringData = encoding.GetString(bytesPtr, bytes.Length);
            }
        }
    }

    /// <summary>Transfer a <see cref="SingleRange"/> value.</summary>
    /// <param name="singleRangeData">A ref value to a <see cref="SingleRange"/> with the data to transfer.</param>
    public void TransferSingleRange(ref SingleRange singleRangeData)
    {
        ArgumentNullException.ThrowIfNull(singleRangeData);

        var hi = singleRangeData.Hi;
        var lo = singleRangeData.Lo;

        TransferSingle(ref hi);
        TransferSingle(ref lo);

        singleRangeData = new SingleRange(lo, hi);
    }

    /// <summary>This is the way to call the base class defined core transfer implementation.</summary>
    /// <param name="data">A <see cref="nint"/> with the pointer to the data to transfer.</param>
    /// <param name="dataSize">An <see cref="int"/> with the size of the transferred data.</param>
    /// <remarks>This is a marshalled operation and expects things like <see cref="GCHandle.Alloc(object?)"/> or <see cref="Marshal.StructureToPtr"/> to generate these pointers and access the address of the data being transferred.</remarks>
    public void TransferUser(nint data, int dataSize) => TransferCore(data, dataSize);

    /// <summary>The core implementation of the data transfer operation.</summary>
    /// <param name="data">A generic pointer to the data to transfer.</param>
    /// <param name="dataSize">An <see cref="int"/> with the size of the transferred data.</param>
    /// <remarks>This is an unsafe operation and uses unsafe pointers to access the address of the data being transferred.</remarks>
    public unsafe void TransferUser(void* data, int dataSize) => TransferCore(data, dataSize);

    /// <summary>The core implementation of the data transfer operation.</summary>
    /// <param name="data">A <see cref="nint"/> with the pointer to the data to transfer.</param>
    /// <param name="dataSize">An <see cref="int"/> with the size of the transferred data.</param>
    /// <remarks>This is a marshalled operation and expects things like <see cref="GCHandle.Alloc(object?)"/> or <see cref="Marshal.StructureToPtr"/> to generate these pointers and access the address of the data being transferred.</remarks>
    protected unsafe void TransferCore(nint data, int dataSize) => TransferCore(data.ToPointer(), dataSize);

    /// <summary>The core implementation of the data transfer operation.</summary>
    /// <param name="data">A generic pointer to the data to transfer.</param>
    /// <param name="dataSize">An <see cref="int"/> with the size of the transferred data.</param>
    /// <remarks>This is an unsafe operation and uses unsafe pointers to access the address of the data being transferred.</remarks>
    protected abstract unsafe void TransferCore(void* data, int dataSize);

    private void TransferStruct<T>(ref T structData)
        where T : struct
    {
        var structDataPtr = GCHandle.Alloc(structData, GCHandleType.Pinned);
        try
        {
            TransferCore(structDataPtr.AddrOfPinnedObject(), Marshal.SizeOf<T>());
            structData = Marshal.PtrToStructure<T>(structDataPtr.AddrOfPinnedObject());
        }
        finally
        {
            if (structDataPtr.IsAllocated)
            {
                structDataPtr.Free();
            }
        }
    }
}
