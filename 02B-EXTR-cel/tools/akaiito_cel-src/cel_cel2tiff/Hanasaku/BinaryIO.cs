using System;
using System.Collections.Generic;
using System.IO;

using System.Runtime.InteropServices;

namespace Hanasaku
{
    /// <summary>
    /// A compilation of standalone (static) functions related to lower-level I/O.
    /// </summary>
    static class BinaryIO
    {
        /// <summary>
        /// Creates a <typeparamref name="T"/> object from a readable Stream.
        /// </summary>
        /// <typeparam name="T">A <c>struct</c> that has the <see cref="System.Runtime.InteropServices.StructLayoutAttribute"/> attribute
        /// specifying the <value>System.Runtime.InteropServices.LayoutKind.Sequential</value> arrangement.</typeparam>
        /// <param name="reader">A reader attached to a readable <see cref="System.IO.Stream"/>.</param>
        /// <returns>A <typeparamref name="T"/> reference</returns>
        /// <exception cref="StreamTooShortException">Thrown if not enough data could be read from the underlying Stream.</exception>
        public static T FromBinaryReader<T>(BinaryReader reader) where T : struct
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            if (bytes.Length < Marshal.SizeOf(typeof(T)))
            {
                throw new StreamTooShortException();
            }

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

        /// <summary>
        /// Creates a byte[] representation of a <typeparamref name="T"/> object
        /// </summary>
        /// <typeparam name="T">A <c>struct</c> that has the <see cref="System.Runtime.InteropServices.StructLayoutAttribute"/> attribute
        /// specifying the <value>System.Runtime.InteropServices.LayoutKind.Sequential</value> arrangement.</typeparam>
        /// <param name="theStructure">A non-<value>null</value> <typeparamref name="T"/> reference</param>
        /// <returns>A byte array</returns>
        public static byte[] ToByteArray<T>(T theStructure) where T : struct
        {
            byte[] bytes = new byte[Marshal.SizeOf(typeof(T))];

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            Marshal.StructureToPtr(theStructure, handle.AddrOfPinnedObject(), false);
            handle.Free();

            return bytes;
        }

        /// <summary>
        /// CopyTo for generic Stream objects.
        /// </summary>
        /// <param name="src">A readable <see cref="System.IO.Stream"/></param>
        /// <param name="dest">A writable <see cref="System.IO.Stream"/></param>
        /// <param name="bytesCount">Number of bytes to copy.</param>
        public static void CopyTo(Stream src, Stream dest, int bytesCount)
        {
            int size = (src.CanSeek) ? Math.Min((int)(src.Length - src.Position), 0x2000) : 0x2000;
            byte[] buffer = new byte[size];
            int n;
            do
            {
                int toRead = Math.Min(size, bytesCount);
                n = src.Read(buffer, 0, toRead);
                bytesCount -= n;
                dest.Write(buffer, 0, n);
            } while (n != 0);
        }

        /// <summary>
        /// Specialized CopyTo, targetting a MemoryStream
        /// </summary>
        /// <param name="src">A readable <see cref="System.IO.Stream"/></param>
        /// <param name="dest">A writable <see cref="System.IO.MemoryStream"/></param>
        /// <param name="bytesCount">Number of bytes to copy.</param>
        public static void CopyTo(Stream src, MemoryStream dest, int bytesCount)
        {
            if (src.CanSeek)
            {
                int pos = (int)dest.Position;
                int length = bytesCount + pos;
                dest.SetLength(length);

                while (pos < length)
                    pos += src.Read(dest.GetBuffer(), pos, length - pos);
            }
            else
                CopyTo(src, (Stream)dest, bytesCount);
        }
    }
}