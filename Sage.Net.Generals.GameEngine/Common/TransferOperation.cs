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
using System.Runtime.InteropServices;
using System.Text;
using Sage.Net.Generals.GameEngine.GameClient;
using Sage.Net.Generals.Libraries.BaseTypes;

namespace Sage.Net.Generals.GameEngine.Common;

/// <summary>Represents a transfer operation.</summary>
public abstract class TransferOperation
{
    /// <summary>Gets or sets the transfer mode.</summary>
    public TransferMode Mode { get; protected set; } = TransferMode.Invalid;

    /// <summary>Gets or sets the transfer identifier.</summary>
    public string Identifier { get; protected set; } = string.Empty;

    /// <summary>Gets or sets the transfer options.</summary>
    public TransferOptions Options { get; protected set; } = TransferOptions.None;

    /// <summary>Opens a transfer operation using the specified identifier.</summary>
    /// <param name="identifier">The unique identifier for the transfer operation to be opened.</param>
    /// <remarks>This method is intended to initialize and prepare the transfer operation with the given <paramref name="identifier"/>.</remarks>
    public virtual void Open(string identifier) => Identifier = identifier;

    /// <summary>Closes the transfer operation, finalizing and releasing any associated resources.</summary>
    /// <remarks>This method is intended to complete the transfer process and clean up any resources allocated during the operation.</remarks>
    public abstract void Close();

    /// <summary>Begins a new block in the transfer operation.</summary>
    /// <returns>An integer representing the identifier of the newly started block.</returns>
    /// <remarks>This method is intended to initialize a new segment within the transfer operation for further processing.</remarks>
    public abstract int BeginBlock();

    /// <summary>Ends the current block in the transfer operation.</summary>
    /// <remarks>This method is intended to finalize the processing of the current segment within the transfer operation.</remarks>
    public abstract void EndBlock();

    /// <summary>Skips the specified amount of data in the transfer operation.</summary>
    /// <param name="dataSize">The size of the data to skip, in bytes.</param>
    /// <remarks>This method is intended to move the transfer operation's internal pointer forward by the given <paramref name="dataSize"/>, effectively ignoring the data in that range.</remarks>
    public abstract void Skip(int dataSize);

    /// <summary>Transfers a snapshot using the specified snapshot object.</summary>
    /// <param name="snapshot">The snapshot instance that implements <see cref="ISnapshot"/> and will be processed in the transfer operation.</param>
    /// <remarks>This method is intended to facilitate the transfer of state information encapsulated in the specified <paramref name="snapshot"/> during save, load, or CRC operations.</remarks>
    public abstract void TransferSnapshot(ISnapshot? snapshot);

    /// <summary>Sets the transfer options for the transfer operation.</summary>
    /// <param name="options">The transfer options to be applied.</param>
    /// <remarks>This will add the given <paramref name="options"/> to any existing ones.</remarks>
    public virtual void SetOptions(TransferOptions options) => Options |= options;

    /// <summary>Clears the transfer options for the transfer operation.</summary>
    /// <param name="options">The transfer options to be cleared.</param>
    /// <remarks>This will remove the given <paramref name="options"/> from any existing ones.</remarks>
    public virtual void ClearOptions(TransferOptions options) => Options &= ~options;

    /// <summary>Transfers a version number using the specified version data.</summary>
    /// <param name="versionData">The version data to be transferred.</param>
    /// <param name="currentVersion">The current version of the game.</param>
    /// <exception cref="InvalidOperationException">Thrown when the transferred version is higher than the specified <paramref name="currentVersion"/>.</exception>
    public virtual void TransferVersion(ref uint versionData, uint currentVersion)
    {
        TransferUnmanaged(ref versionData);
        if (versionData <= currentVersion)
        {
            return;
        }

        var message = $"Unknown version '{versionData}' should be no higher than '{currentVersion}'.";
        Debug.Fail(message);

        throw new InvalidOperationException(message);
    }

    /// <summary>Transfers an <see cref="sbyte"/> using the specified <see cref="sbyte"/> data.</summary>
    /// <param name="sbyteData">The <see cref="sbyte"/> data to be transferred.</param>
    public virtual void TransferSByte(ref sbyte sbyteData) => TransferUnmanaged(ref sbyteData);

    /// <summary>Transfers an <see cref="byte"/> using the specified <see cref="byte"/> data.</summary>
    /// <param name="byteData">The <see cref="byte"/> data to be transferred.</param>
    public virtual void TransferByte(ref byte byteData) => TransferUnmanaged(ref byteData);

    /// <summary>Transfers an <see cref="bool"/> using the specified <see cref="bool"/> data.</summary>
    /// <param name="boolData">The <see cref="bool"/> data to be transferred.</param>
    public virtual void TransferBool(ref bool boolData) => TransferUnmanaged(ref boolData);

    /// <summary>Transfers an <see cref="int"/> using the specified <see cref="int"/> data.</summary>
    /// <param name="int32Data">The <see cref="int"/> data to be transferred.</param>
    public virtual void TransferInt32(ref int int32Data) => TransferUnmanaged(ref int32Data);

    /// <summary>Transfers an <see cref="long"/> using the specified <see cref="long"/> data.</summary>
    /// <param name="int64Data">The <see cref="long"/> data to be transferred.</param>
    public virtual void TransferInt64(ref long int64Data) => TransferUnmanaged(ref int64Data);

    /// <summary>Transfers an <see cref="uint"/> using the specified <see cref="uint"/> data.</summary>
    /// <param name="uint32Data">The <see cref="uint"/> data to be transferred.</param>
    public virtual void TransferUInt32(ref uint uint32Data) => TransferUnmanaged(ref uint32Data);

    /// <summary>Transfers an <see cref="short"/> using the specified <see cref="short"/> data.</summary>
    /// <param name="int16Data">The <see cref="short"/> data to be transferred.</param>
    public virtual void TransferInt16(ref short int16Data) => TransferUnmanaged(ref int16Data);

    /// <summary>Transfers an <see cref="ushort"/> using the specified <see cref="ushort"/> data.</summary>
    /// <param name="uint16Data">The <see cref="ushort"/> data to be transferred.</param>
    public virtual void TransferUInt16(ref ushort uint16Data) => TransferUnmanaged(ref uint16Data);

    /// <summary>Transfers an <see cref="float"/> using the specified <see cref="float"/> data.</summary>
    /// <param name="floatData">The <see cref="float"/> data to be transferred.</param>
    public virtual void TransferFloat(ref float floatData) => TransferUnmanaged(ref floatData);

    /// <summary>Transfers an ASCII <see cref="string"/> marker label using the specified <see cref="string"/> marker label data.</summary>
    /// <param name="asciiStringData">The ASCII <see cref="string"/> data to be transferred.</param>
    /// <remarks>By default, this method does nothing.</remarks>
    public virtual void TransferMarkerLabel(string asciiStringData) { }

    /// <summary>Transfers an ASCII <see cref="string"/> using the specified <see cref="string"/> data.</summary>
    /// <param name="asciiStringData">The ASCII <see cref="string"/> data to be transferred.</param>
    public virtual void TransferAsciiString(string asciiStringData) =>
        TransferString(ref asciiStringData, Encoding.ASCII);

    /// <summary>Transfers a Unicode <see cref="string"/> using the specified <see cref="string"/> data.</summary>
    /// <param name="unicodeStringData">The Unicode <see cref="string"/> data to be transferred.</param>
    public virtual void TransferUnicodeString(string unicodeStringData) =>
        TransferString(ref unicodeStringData, Encoding.Unicode);

    /// <summary>Transfers a <see cref="FCoord3D"/> using the specified <see cref="FCoord3D"/> data.</summary>
    /// <param name="coord3D">The <see cref="FCoord3D"/> data to be transferred.</param>
    public virtual void TransferFCoord3D(ref FCoord3D coord3D)
    {
        ArgumentNullException.ThrowIfNull(coord3D);

        var x = coord3D.X;
        var y = coord3D.Y;
        var z = coord3D.Z;

        TransferFloat(ref x);
        TransferFloat(ref y);
        TransferFloat(ref z);

        coord3D = new FCoord3D(x, y, z);
    }

    /// <summary>Transfers a <see cref="Coord3D"/> using the specified <see cref="Coord3D"/> data.</summary>
    /// <param name="coord3D">The <see cref="Coord3D"/> data to be transferred.</param>
    public virtual void TransferCoord3D(ref Coord3D coord3D)
    {
        ArgumentNullException.ThrowIfNull(coord3D);

        var x = coord3D.X;
        var y = coord3D.Y;
        var z = coord3D.Z;

        TransferInt32(ref x);
        TransferInt32(ref y);
        TransferInt32(ref z);

        coord3D = new Coord3D(x, y, z);
    }

    /// <summary>Transfers an <see cref="FRegion3D"/> using the specified <see cref="FRegion3D"/> data.</summary>
    /// <param name="region3D">The <see cref="FRegion3D"/> data to be transferred.</param>
    public virtual void TransferFRegion3D(ref FRegion3D region3D)
    {
        ArgumentNullException.ThrowIfNull(region3D);

        FCoord3D hi = region3D.Hi;
        FCoord3D lo = region3D.Lo;

        TransferFCoord3D(ref hi);
        TransferFCoord3D(ref lo);

        region3D = new FRegion3D(hi, lo);
    }

    /// <summary>Transfers a <see cref="Region3D"/> using the specified <see cref="Region3D"/> data.</summary>
    /// <param name="region3D">The <see cref="Region3D"/> data to be transferred.</param>
    public virtual void TransferRegion3D(ref Region3D region3D)
    {
        ArgumentNullException.ThrowIfNull(region3D);

        Coord3D hi = region3D.Hi;
        Coord3D lo = region3D.Lo;

        TransferCoord3D(ref hi);
        TransferCoord3D(ref lo);

        region3D = new Region3D(hi, lo);
    }

    /// <summary>Transfers an <see cref="FCoord2D"/> using the specified <see cref="FCoord2D"/> data.</summary>
    /// <param name="coord2D">The <see cref="FCoord2D"/> data to be transferred.</param>
    public virtual void TransferFCoord2D(ref FCoord2D coord2D)
    {
        ArgumentNullException.ThrowIfNull(coord2D);

        var x = coord2D.X;
        var y = coord2D.Y;

        TransferFloat(ref x);
        TransferFloat(ref y);

        coord2D = new FCoord2D(x, y);
    }

    /// <summary>Transfers a <see cref="Coord2D"/> using the specified <see cref="Coord2D"/> data.</summary>
    /// <param name="coord2D">The <see cref="Coord2D"/> data to be transferred.</param>
    public virtual void TransferCoord2D(ref Coord2D coord2D)
    {
        ArgumentNullException.ThrowIfNull(coord2D);

        var x = coord2D.X;
        var y = coord2D.Y;

        TransferInt32(ref x);
        TransferInt32(ref y);

        coord2D = new Coord2D(x, y);
    }

    /// <summary>Transfers an <see cref="FRegion2D"/> using the specified <see cref="FRegion2D"/> data.</summary>
    /// <param name="region2D">The <see cref="FRegion2D"/> data to be transferred.</param>
    public virtual void TransferFRegion2D(ref FRegion2D region2D)
    {
        ArgumentNullException.ThrowIfNull(region2D);

        FCoord2D hi = region2D.Hi;
        FCoord2D lo = region2D.Lo;

        TransferFCoord2D(ref hi);
        TransferFCoord2D(ref lo);

        region2D = new FRegion2D(hi, lo);
    }

    /// <summary>Transfers a <see cref="Region2D"/> using the specified <see cref="Region2D"/> data.</summary>
    /// <param name="region2D">The <see cref="Region2D"/> data to be transferred.</param>
    public virtual void TransferRegion2D(ref Region2D region2D)
    {
        ArgumentNullException.ThrowIfNull(region2D);

        Coord2D hi = region2D.Hi;
        Coord2D lo = region2D.Lo;

        TransferCoord2D(ref hi);
        TransferCoord2D(ref lo);

        region2D = new Region2D(hi, lo);
    }

    /// <summary>Transfers an <see cref="FRange"/> using the specified <see cref="FRange"/> data.</summary>
    /// <param name="range">The <see cref="FRange"/> data to be transferred.</param>
    public virtual void TransferFRange(ref FRange range)
    {
        ArgumentNullException.ThrowIfNull(range);

        var lo = range.Lo;
        var hi = range.Hi;

        TransferFloat(ref lo);
        TransferFloat(ref hi);

        range = new FRange(lo, hi);
    }

    /// <summary>Transfers a <see cref="Color"/> using the specified <see cref="Color"/> data.</summary>
    /// <param name="color">The <see cref="Color"/> data to be transferred.</param>
    public virtual void TransferColor(ref Color color)
    {
        ArgumentNullException.ThrowIfNull(color);

        var value = color.Value;
        TransferInt32(ref value);
        color = new Color(value);
    }

    /// <summary>Transfers an <see cref="RgbColor"/> using the specified <see cref="RgbColor"/> data.</summary>
    /// <param name="color">The <see cref="RgbColor"/> data to be transferred.</param>
    public virtual void TransferRgbColor(ref RgbColor color)
    {
        ArgumentNullException.ThrowIfNull(color);

        var red = color.Red;
        var green = color.Green;
        var blue = color.Blue;

        TransferFloat(ref red);
        TransferFloat(ref green);
        TransferFloat(ref blue);

        color = new RgbColor(red, green, blue);
    }

    /// <summary>Transfers an <see cref="FRgbaColor"/> using the specified <see cref="FRgbaColor"/> data.</summary>
    /// <param name="color">The <see cref="FRgbaColor"/> data to be transferred.</param>
    public virtual void TransferFRgbaColor(ref FRgbaColor color)
    {
        ArgumentNullException.ThrowIfNull(color);

        var red = color.Red;
        var green = color.Green;
        var blue = color.Blue;
        var alpha = color.Alpha;

        TransferFloat(ref red);
        TransferFloat(ref green);
        TransferFloat(ref blue);
        TransferFloat(ref alpha);

        color = new FRgbaColor(red, green, blue, alpha);
    }

    /// <summary>Transfers an <see cref="RgbaColor"/> using the specified <see cref="RgbaColor"/> data.</summary>
    /// <param name="color">The <see cref="RgbaColor"/> data to be transferred.</param>
    public virtual void TransferRgbaColor(ref RgbaColor color)
    {
        ArgumentNullException.ThrowIfNull(color);

        var red = color.Red;
        var green = color.Green;
        var blue = color.Blue;
        var alpha = color.Alpha;

        TransferByte(ref red);
        TransferByte(ref green);
        TransferByte(ref blue);
        TransferByte(ref alpha);

        color = new RgbaColor(red, green, blue, alpha);
    }

    /// <summary>Transfers an <see cref="ObjectId"/> using the specified <see cref="ObjectId"/> data.</summary>
    /// <param name="objectId">The <see cref="ObjectId"/> data to be transferred.</param>
    public virtual void TransferObjectId(ref ObjectId objectId)
    {
        ArgumentNullException.ThrowIfNull(objectId);

        var value = objectId.Value;
        TransferInt32(ref value);
        objectId = new ObjectId(value);
    }

    /// <summary>Transfers a <see cref="DrawableId"/> using the specified <see cref="DrawableId"/> data.</summary>
    /// <param name="drawableId">The <see cref="DrawableId"/> data to be transferred.</param>
    public virtual void TransferDrawableId(ref DrawableId drawableId)
    {
        ArgumentNullException.ThrowIfNull(drawableId);

        var value = drawableId.Value;
        TransferInt32(ref value);
        drawableId = new DrawableId(value);
    }

    /// <summary>Transfers a list of <see cref="ObjectId"/> values.</summary>
    /// <param name="objectIdListData">The list of <see cref="ObjectId"/> values to be transferred.</param>
    /// <remarks>
    /// Behavior depends on <see cref="Mode"/>:
    /// - <see cref="TransferMode.Save"/>/<see cref="TransferMode.Crc"/>: iterates current items and transfers each.
    /// - <see cref="TransferMode.Load"/>: expects an empty list and fills it from the stream.
    /// </remarks>
    public virtual void TransferObjectIdList(ref IList<ObjectId> objectIdListData)
    {
        ArgumentNullException.ThrowIfNull(objectIdListData);

        const uint currentVersion = 1U;
        var version = currentVersion;
        TransferVersion(ref version, currentVersion);

        var listCount = (ushort)objectIdListData.Count;
        TransferUInt16(ref listCount);

        ObjectId objectId = ObjectId.Invalid;
        switch (Mode)
        {
            case TransferMode.Save or TransferMode.Crc:
            {
                foreach (ObjectId item in objectIdListData)
                {
                    objectId = item;
                    TransferObjectId(ref objectId);
                }

                break;
            }

            case TransferMode.Load when objectIdListData.Count != 0:
            {
                const string message = "Object list should be empty before loading";
                Debug.Fail(message);
                throw new InvalidOperationException(message);
            }

            case TransferMode.Load:
            {
                for (ushort i = 0; i < listCount; i++)
                {
                    TransferObjectId(ref objectId);
                    objectIdListData.Add(objectId);
                }

                break;
            }

            case TransferMode.Invalid:
            default:
            {
                var message = $"Unknown transfer mode '{Mode}' for object ID list.";
                Debug.Fail(message);
                throw new InvalidOperationException(message);
            }
        }
    }

    /// <summary>Transfers a linked list of <see cref="ObjectId"/> values.</summary>
    /// <param name="objectIdLinkedListData">The linked list of <see cref="ObjectId"/> values to be transferred.</param>
    /// <remarks>
    /// Behavior depends on <see cref="Mode"/>:
    /// - <see cref="TransferMode.Save"/>/<see cref="TransferMode.Crc"/>: iterates current items and transfers each.
    /// - <see cref="TransferMode.Load"/>: expects an empty list and fills it from the stream.
    /// </remarks>
    public virtual void TransferObjectIdLinkedList(ref LinkedList<ObjectId> objectIdLinkedListData)
    {
        ArgumentNullException.ThrowIfNull(objectIdLinkedListData);

        const uint currentVersion = 1U;
        var version = currentVersion;
        TransferVersion(ref version, currentVersion);

        var listCount = (ushort)objectIdLinkedListData.Count;
        TransferUInt16(ref listCount);

        ObjectId objectId = ObjectId.Invalid;
        switch (Mode)
        {
            case TransferMode.Save or TransferMode.Crc:
            {
                foreach (ObjectId item in objectIdLinkedListData)
                {
                    objectId = item;
                    TransferObjectId(ref objectId);
                }

                break;
            }

            case TransferMode.Load:
            {
                if (objectIdLinkedListData.Count != 0)
                {
                    const string message = "Object linked list should be empty before loading";
                    Debug.Fail(message);
                    throw new InvalidOperationException(message);
                }

                for (ushort i = 0; i < listCount; i++)
                {
                    TransferObjectId(ref objectId);
                    _ = objectIdLinkedListData.AddLast(objectId);
                }

                break;
            }

            case TransferMode.Invalid:
            default:
            {
                var message = $"Unknown transfer mode '{Mode}' for object ID linked list.";
                Debug.Fail(message);
                throw new InvalidOperationException(message);
            }
        }
    }

    /// <summary>Transfers a linked list of <see cref="int"/> values.</summary>
    /// <param name="int32LinkedListData">The linked list of <see cref="int"/> values to be transferred. If <see langword="null"/>, the call is ignored.</param>
    /// <remarks>
    /// Behavior depends on <see cref="Mode"/>:
    /// - <see cref="TransferMode.Save"/>/<see cref="TransferMode.Crc"/>: iterates current items and transfers each.
    /// - <see cref="TransferMode.Load"/>: expects an empty list and fills it from the stream.
    /// </remarks>
    public virtual void TransferInt32LinkedList(ref LinkedList<int>? int32LinkedListData)
    {
        if (int32LinkedListData is null)
        {
            return;
        }

        const uint currentVersion = 1U;
        var version = currentVersion;
        TransferVersion(ref version, currentVersion);

        var listCount = (ushort)int32LinkedListData.Count;
        TransferUInt16(ref listCount);

        var intData = 0;
        switch (Mode)
        {
            case TransferMode.Save or TransferMode.Crc:
            {
                foreach (var item in int32LinkedListData)
                {
                    intData = item;
                    TransferInt32(ref intData);
                }

                break;
            }

            case TransferMode.Load:
            {
                if (int32LinkedListData.Count != 0)
                {
                    const string message = "Int32 linked list should be empty before loading";
                    Debug.Fail(message);
                    throw new InvalidOperationException(message);
                }

                for (ushort i = 0; i < listCount; i++)
                {
                    TransferInt32(ref intData);
                    _ = int32LinkedListData.AddLast(intData);
                }

                break;
            }

            case TransferMode.Invalid:
            default:
            {
                var message = $"Unknown transfer mode '{Mode}' for int32 linked list.";
                Debug.Fail(message);
                throw new InvalidOperationException(message);
            }
        }
    }

    /// <summary>Transfers data in a user-defined way in the context of a transfer operation.</summary>
    /// <param name="data">The pointer to the data to be transferred.</param>
    /// <param name="dataSize">The size of the data to be transferred, in bytes.</param>
    public virtual void TransferUser(nint data, int dataSize) => TransferCore(data, dataSize);

    /// <inheritdoc cref="TransferUser(nint, int)"/>
    public virtual unsafe void TransferUser(void* data, int dataSize) => TransferCore(data, dataSize);

    /// <summary>The actual transfer operation.</summary>
    /// <param name="data">The pointer to the data to be transferred.</param>
    /// <param name="dataSize">The size of the data to be transferred, in bytes.</param>
    protected unsafe void TransferCore(nint data, int dataSize) => TransferCore(data.ToPointer(), dataSize);

    /// <inheritdoc cref="TransferCore(nint, int)"/>
    protected abstract unsafe void TransferCore(void* data, int dataSize);

    private unsafe void TransferUnmanaged<TUnmanaged>(ref TUnmanaged data)
        where TUnmanaged : unmanaged
    {
        TUnmanaged localData = data;
        TransferCore(&localData, Marshal.SizeOf<TUnmanaged>());
        data = localData;
    }

    private unsafe void TransferString(ref string stringData, Encoding encoding)
    {
        var bytes = encoding.GetBytes(stringData);
        fixed (byte* pBytes = bytes)
        {
            TransferCore(pBytes, bytes.Length);
            stringData = encoding.GetString(pBytes, bytes.Length);
        }
    }
}
