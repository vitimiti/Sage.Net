// -----------------------------------------------------------------------
// <copyright file="RectangleMarshaller.cs" company="Sage.Net">
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

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Sage.Net.NativeHelpers.Sdl3.NativeImports;

namespace Sage.Net.NativeHelpers.Sdl3.CustomMarshallers;

[CustomMarshaller(typeof(Rectangle), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToUnmanagedIn))]
internal static class RectangleMarshaller
{
    public unsafe ref struct ManagedToUnmanagedIn
    {
        private Sdl.Rect* _unmanaged;

        public void FromManaged(Rectangle? managed)
        {
            if (managed is null)
            {
                _unmanaged = null;
                return;
            }

            _unmanaged = (Sdl.Rect*)NativeMemory.Alloc((uint)Marshal.SizeOf<Sdl.Rect>());
            if (_unmanaged is null)
            {
                return;
            }

            _unmanaged->X = managed.X;
            _unmanaged->Y = managed.Y;
            _unmanaged->W = managed.Width;
            _unmanaged->H = managed.Height;
        }

        public readonly Sdl.Rect* ToUnmanaged() => _unmanaged;

        public void Free()
        {
            if (_unmanaged is null)
            {
                return;
            }

            NativeMemory.Free(_unmanaged);
            _unmanaged = null;
        }
    }
}
