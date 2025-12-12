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

    /// <summary>Transfer a <see cref="Coord3D"/> value.</summary>
    /// <param name="coord3DData">A ref value to a <see cref="Coord3D"/> with the data to transfer.</param>
    public void TransferCoord3D(ref Coord3D coord3DData)
    {
        ArgumentNullException.ThrowIfNull(coord3DData);

        var x = coord3DData.X;
        var y = coord3DData.Y;
        var z = coord3DData.Z;

        TransferSingle(ref x);
        TransferSingle(ref y);
        TransferSingle(ref z);

        coord3DData = new Coord3D(x, y, z);
    }

    /// <summary>Transfer an <see cref="Int32Coord3D"/> value.</summary>
    /// <param name="int32Coord3DData">A ref value to an <see cref="Int32Coord3D"/> with the data to transfer.</param>
    public void TransferInt32Coord3D(ref Int32Coord3D int32Coord3DData)
    {
        ArgumentNullException.ThrowIfNull(int32Coord3DData);

        var x = int32Coord3DData.X;
        var y = int32Coord3DData.Y;
        var z = int32Coord3DData.Z;

        TransferInt32(ref x);
        TransferInt32(ref y);
        TransferInt32(ref z);

        int32Coord3DData = new Int32Coord3D(x, y, z);
    }

    /// <summary>Transfer a <see cref="Region3D"/> value.</summary>
    /// <param name="region3DData">A ref value to a <see cref="Region3D"/> with the data to transfer.</param>
    public void TransferRegion3D(ref Region3D region3DData)
    {
        ArgumentNullException.ThrowIfNull(region3DData);

        Coord3D lo = region3DData.Lo;
        Coord3D hi = region3DData.Hi;

        TransferCoord3D(ref lo);
        TransferCoord3D(ref hi);

        region3DData = new Region3D(lo, hi);
    }

    /// <summary>Transfer an <see cref="Int32Region3D"/> value.</summary>
    /// <param name="int32Region3DData">A ref value to an <see cref="Int32Region3D"/> with the data to transfer.</param>
    public void TransferInt32Region3D(ref Int32Region3D int32Region3DData)
    {
        ArgumentNullException.ThrowIfNull(int32Region3DData);

        Int32Coord3D lo = int32Region3DData.Lo;
        Int32Coord3D hi = int32Region3DData.Hi;

        TransferInt32Coord3D(ref lo);
        TransferInt32Coord3D(ref hi);

        int32Region3DData = new Int32Region3D(lo, hi);
    }

    /// <summary>Transfer a <see cref="Coord2D"/> value.</summary>
    /// <param name="coord2DData">A ref value to a <see cref="Coord2D"/> with the data to transfer.</param>
    public void TransferCoord2D(ref Coord2D coord2DData)
    {
        ArgumentNullException.ThrowIfNull(coord2DData);

        var x = coord2DData.X;
        var y = coord2DData.Y;

        TransferSingle(ref x);
        TransferSingle(ref y);

        coord2DData = new Coord2D(x, y);
    }

    /// <summary>Transfer an <see cref="Int32Coord2D"/> value.</summary>
    /// <param name="int32Coord2DData">A ref value to an <see cref="Int32Coord2D"/> with the data to transfer.</param>
    public void TransferInt32Coord2D(ref Int32Coord2D int32Coord2DData)
    {
        ArgumentNullException.ThrowIfNull(int32Coord2DData);

        var x = int32Coord2DData.X;
        var y = int32Coord2DData.Y;

        TransferInt32(ref x);
        TransferInt32(ref y);

        int32Coord2DData = new Int32Coord2D(x, y);
    }

    /// <summary>Transfer a <see cref="Region2D"/> value.</summary>
    /// <param name="region2DData">A ref value to a <see cref="Region2D"/> with the data to transfer.</param>
    public void TransferRegion2D(ref Region2D region2DData)
    {
        ArgumentNullException.ThrowIfNull(region2DData);

        Coord2D lo = region2DData.Lo;
        Coord2D hi = region2DData.Hi;

        TransferCoord2D(ref lo);
        TransferCoord2D(ref hi);

        region2DData = new Region2D(lo, hi);
    }

    /// <summary>Transfer an <see cref="Int32Region2D"/> value.</summary>
    /// <param name="int32Region2DData">A ref value to an <see cref="Int32Region2D"/> with the data to transfer.</param>
    public void TransferInt32Region2D(ref Int32Region2D int32Region2DData)
    {
        ArgumentNullException.ThrowIfNull(int32Region2DData);

        Int32Coord2D lo = int32Region2DData.Lo;
        Int32Coord2D hi = int32Region2DData.Hi;

        TransferInt32Coord2D(ref lo);
        TransferInt32Coord2D(ref hi);

        int32Region2DData = new Int32Region2D(lo, hi);
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

    /// <summary>Transfer an <see cref="int"/> representing a color value.</summary>
    /// <param name="colorData">A ref value to an <see cref="int"/> with the color data to transfer.</param>
    public void TransferColor(ref int colorData) => TransferStruct(ref colorData);

    /// <summary>Transfer a <see cref="RgbColor"/> value.</summary>
    /// <param name="rgbColorData">A ref value to a <see cref="RgbColor"/> with the data to transfer.</param>
    public void TransferRgbColor(ref RgbColor rgbColorData)
    {
        ArgumentNullException.ThrowIfNull(rgbColorData);

        var red = rgbColorData.Red;
        var green = rgbColorData.Green;
        var blue = rgbColorData.Blue;

        TransferSingle(ref red);
        TransferSingle(ref green);
        TransferSingle(ref blue);

        rgbColorData = new RgbColor(red, green, blue);
    }

    /// <summary>Transfer a <see cref="RgbaColorSingle"/> value.</summary>
    /// <param name="rgbaColorSingleData">A ref value to a <see cref="RgbaColorSingle"/> with the data to transfer.</param>
    public void TransferRgbaColorSingle(ref RgbaColorSingle rgbaColorSingleData)
    {
        ArgumentNullException.ThrowIfNull(rgbaColorSingleData);

        var red = rgbaColorSingleData.Red;
        var green = rgbaColorSingleData.Green;
        var blue = rgbaColorSingleData.Blue;
        var alpha = rgbaColorSingleData.Alpha;

        TransferSingle(ref red);
        TransferSingle(ref green);
        TransferSingle(ref blue);
        TransferSingle(ref alpha);

        rgbaColorSingleData = new RgbaColorSingle(red, green, blue, alpha);
    }

    /// <summary>Transfer a <see cref="RgbaColorInt32"/> value.</summary>
    /// <param name="rgbaColorInt32Data">A ref value to a <see cref="RgbaColorInt32"/> with the data to transfer.</param>
    public void TransferRgbaColorInt32(ref RgbaColorInt32 rgbaColorInt32Data)
    {
        ArgumentNullException.ThrowIfNull(rgbaColorInt32Data);

        var red = rgbaColorInt32Data.Red;
        var green = rgbaColorInt32Data.Green;
        var blue = rgbaColorInt32Data.Blue;
        var alpha = rgbaColorInt32Data.Alpha;

        TransferUInt32(ref red);
        TransferUInt32(ref green);
        TransferUInt32(ref blue);
        TransferUInt32(ref alpha);

        rgbaColorInt32Data = new RgbaColorInt32(red, green, blue, alpha);
    }

    /// <summary>Transfer a <see cref="ObjectId"/> value.</summary>
    /// <param name="objectIdData">A ref value to a <see cref="ObjectId"/> with the data to transfer.</param>
    public void TransferObjectId(ref ObjectId objectIdData)
    {
        ArgumentNullException.ThrowIfNull(objectIdData);

        var value = objectIdData.Value;
        TransferInt32(ref value);
        objectIdData = new ObjectId(value);
    }

    /// <summary>Transfer a <see cref="DrawableId"/> value.</summary>
    /// <param name="drawableIdData">A ref value to a <see cref="DrawableId"/> with the data to transfer.</param>
    public void TransferDrawableId(ref DrawableId drawableIdData)
    {
        ArgumentNullException.ThrowIfNull(drawableIdData);

        var value = drawableIdData.Value;
        TransferInt32(ref value);
        drawableIdData = new DrawableId(value);
    }

    /// <summary>Transfer a list of <see cref="ObjectId"/> values.</summary>
    /// <param name="objectIdListData">A ref value to a list of <see cref="ObjectId"/> with the data to transfer.</param>
    /// <exception cref="TransferListNotEmptyException">Thrown when the <paramref name="objectIdListData"/> is not empty and the transfer mode is <see cref="TransferMode.Load"/>.</exception>
    /// <exception cref="TransferModeUnknownException">Thrown when the transfer mode is unknown.</exception>
    public void TransferObjectIdList(ref IList<ObjectId> objectIdListData)
    {
        ArgumentNullException.ThrowIfNull(objectIdListData);

        const byte currentVersion = 1;
        var version = currentVersion;
        TransferVersion(ref version, currentVersion);

        var count = (ushort)objectIdListData.Count;
        TransferUInt16(ref count);

        ObjectId objectId = ObjectId.InvalidId;
        switch (TransferMode)
        {
            case TransferMode.Save or TransferMode.Crc:
            {
                for (var i = 0; i < count; i++)
                {
                    objectId = objectIdListData[i];
                    TransferObjectId(ref objectId);
                }

                break;
            }

            case TransferMode.Load when objectIdListData.Count != 0:
            {
                const string message = $"{nameof(objectIdListData)} should be empty before loading.";
                Debug.Fail(message);
                throw new TransferListNotEmptyException(message);
            }

            case TransferMode.Load:
            {
                for (var i = 0; i < count; i++)
                {
                    TransferObjectId(ref objectId);
                    objectIdListData.Add(objectId);
                }

                break;
            }

            case TransferMode.Invalid:
            default:
            {
                var message = $"Unknown transfer mode '{TransferMode}'.";
                Debug.Fail(message);
                throw new TransferModeUnknownException(message);
            }
        }
    }

    /// <summary>Transfer a <see cref="LinkedList{T}"/> of <see cref="ObjectId"/> values.</summary>
    /// <param name="objectIdLinkedListData">A ref value to a <see cref="LinkedList{T}"/> of <see cref="ObjectId"/> with the data to transfer.</param>
    /// <exception cref="TransferListNotEmptyException">Thrown when the <paramref name="objectIdLinkedListData"/> is not empty and the transfer mode is <see cref="TransferMode.Load"/>.</exception>
    /// <exception cref="TransferModeUnknownException">Thrown when the transfer mode is unknown.</exception>
    public void TransferObjectIdLinkedList(ref LinkedList<ObjectId> objectIdLinkedListData)
    {
        ArgumentNullException.ThrowIfNull(objectIdLinkedListData);

        const byte currentVersion = 1;
        var version = currentVersion;
        TransferVersion(ref version, currentVersion);

        var count = (ushort)objectIdLinkedListData.Count;
        TransferUInt16(ref count);

        ObjectId objectId = ObjectId.InvalidId;
        switch (TransferMode)
        {
            case TransferMode.Save or TransferMode.Crc:
            {
                LinkedListNode<ObjectId>? node = objectIdLinkedListData.First;
                while (node is not null)
                {
                    objectId = node.Value;
                    TransferObjectId(ref objectId);

                    node = node.Next;
                }

                break;
            }

            case TransferMode.Load when objectIdLinkedListData.Count != 0:
            {
                const string message = $"{nameof(objectIdLinkedListData)} should be empty before loading.";
                Debug.Fail(message);
                throw new TransferListNotEmptyException(message);
            }

            case TransferMode.Load:
            {
                for (var i = 0; i < count; i++)
                {
                    TransferObjectId(ref objectId);
                    _ = objectIdLinkedListData.Append(objectId);
                }

                break;
            }

            case TransferMode.Invalid:
            default:
            {
                var message = $"Unknown transfer mode '{TransferMode}'.";
                Debug.Fail(message);
                throw new TransferModeUnknownException(message);
            }
        }
    }

    /// <summary>Transfer a <see cref="LinkedList{T}"/> of <see cref="int"/> values.</summary>
    /// <param name="int32LinkedListData">A ref value to a <see cref="LinkedList{T}"/> of <see cref="int"/> with the data to transfer.</param>
    /// <exception cref="TransferListNotEmptyException">Thrown when the <paramref name="int32LinkedListData"/> is not empty and the transfer mode is <see cref="TransferMode.Load"/>.</exception>
    /// <exception cref="TransferModeUnknownException">Thrown when the transfer mode is unknown.</exception>
    public void TransferInt32LinkedList(ref LinkedList<int> int32LinkedListData)
    {
        ArgumentNullException.ThrowIfNull(int32LinkedListData);

        const byte currentVersion = 1;
        var version = currentVersion;
        TransferVersion(ref version, currentVersion);

        var count = (ushort)int32LinkedListData.Count;
        TransferUInt16(ref count);

        var intData = 0;
        switch (TransferMode)
        {
            case TransferMode.Save or TransferMode.Crc:
            {
                LinkedListNode<int>? node = int32LinkedListData.First;
                while (node is not null)
                {
                    intData = node.Value;
                    TransferInt32(ref intData);

                    node = node.Next;
                }

                break;
            }

            case TransferMode.Load when int32LinkedListData.Count != 0:
            {
                const string message = $"{nameof(int32LinkedListData)} should be empty before loading.";
                Debug.Fail(message);
                throw new TransferListNotEmptyException(message);
            }

            case TransferMode.Load:
            {
                for (var i = 0; i < count; i++)
                {
                    TransferInt32(ref intData);
                    _ = int32LinkedListData.Append(intData);
                }

                break;
            }

            case TransferMode.Invalid:
            default:
            {
                var message = $"Unknown transfer mode '{TransferMode}'.";
                Debug.Fail(message);
                throw new TransferModeUnknownException(message);
            }
        }
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
