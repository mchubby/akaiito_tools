//-----------------------------------------------------------------------
// <copyright file="CompressionManager.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-01-31</date>
// <summary>Helper methods for file compression.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    
    /// <summary>
    /// Helper methods for file compression.
    /// </summary>
    public sealed class CompressionManager
    {
        private const int BlockSize = 8;
        private const int SlidingWindowSize = 4096;
        private const int LzssBufferSize = 18;
        private const int OnzBufferSize = 16;
        private const byte LzssCompressionType = 0x10;
        private const byte OnzCompressionType = 0x11;
        private const int StandardMinDistance = 1;
        private const int BinaryMinDistance = 3;
        private const byte Arm9FooterLength = 0x0C;
        private const byte Padding = 0xFF;
        
        private CompressionManager()
        {
        }
        
        /// <summary>
        /// Compresses a file using the default encoding (LZSS).
        /// </summary>
        /// <param name="uncompressedData">The uncompressed file data.</param>
        /// <returns>The compressed file data.</returns>
        public static byte[] Compress(byte[] uncompressedData)
        {
            return CompressLzss(uncompressedData);
        }
        
        /// <summary>
        /// Compresses a file using standard LZSS encoding.
        /// </summary>
        /// <param name="uncompressedData">The uncompressed file data.</param>
        /// <returns>The compressed file data.</returns>
        public static byte[] CompressLzss(byte[] uncompressedData)
        {
            return Compress(uncompressedData, LzssBufferSize, LzssCompressionType);
        }
        
        /// <summary>
        /// Compresses a file using ONZ encoding, an extension of LZSS.
        /// </summary>
        /// <param name="uncompressedData">The uncompressed file data.</param>
        /// <returns>The compressed file data.</returns>
        public static byte[] CompressOnz(byte[] uncompressedData)
        {
            return Compress(uncompressedData, OnzBufferSize, OnzCompressionType);
        }
        
        /// <summary>
        /// Decompresses a file, detecting the compression type from the signature at the start of the data.
        /// </summary>
        /// <param name="compressedData">The compressed file data.</param>
        /// <returns>The decompressed file data.</returns>
        public static byte[] Decompress(byte[] compressedData)
        {
            if (compressedData[0] == 0x10)
            {
                return DecompressLzss(compressedData);
            }
            else if (compressedData[0] == 0x11)
            {
                return DecompressOnz(compressedData);
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// Decompresses a file using standard LZSS encoding.
        /// </summary>
        /// <param name="compressedData">The compressed file data.</param>
        /// <returns>The decompressed file data.</returns>
        public static byte[] DecompressLzss(byte[] compressedData)
        {
            if (compressedData[0] != 0x10)
            {
                return null;
            }
            
            int size = compressedData[1] + (compressedData[2] << 8) + (compressedData[3] << 16);
            return DecompressLzss(compressedData, size, 4, 1);
        }
        
        /// <summary>
        /// Decompresses a file using ONZ encoding, an extension of LZSS.
        /// </summary>
        /// <param name="compressedData">The compressed file data.</param>
        /// <returns>The decompressed file data.</returns>
        public static byte[] DecompressOnz(byte[] compressedData)
        {
            if (compressedData[0] != 0x11)
            {
                return null;
            }

            int size = compressedData[1] + (compressedData[2] << 8) + (compressedData[3] << 16);
            List<byte> uncompressedData = new List<byte>();
            int currentPosition = 4;
            while (uncompressedData.Count <= size && currentPosition < compressedData.Length)
            {
                int forward = 1;
                for (int i = 0; i < 8; i++)
                {
                    if (currentPosition + forward >= compressedData.Length)
                    {
                        break;
                    }
                    
                    if (uncompressedData.Count >= size)
                    {
                        break;
                    }
                    
                    if (((compressedData[currentPosition] >> (7 - i)) & 1) == 1)
                    {
                        int amountToCopy;
                        int copyFrom;
                        byte byte1 = compressedData[currentPosition + forward];
                        byte byte2 = compressedData[currentPosition + forward + 1];
                        
                        if ((byte1 & 0xF0) == 0x10)
                        {
                            byte byte3 = compressedData[currentPosition + forward + 2];
                            byte byte4 = compressedData[currentPosition + forward + 3];
                            
                            amountToCopy = 0x111 + ((byte1 & 0xF) << 12) + (byte2 << 4) + (byte3 >> 4);
                            copyFrom = 1 + ((byte3 & 0xF) << 8) + byte4;
                            forward += 4;
                        }
                        else if ((byte1 & 0xF0) == 0x00)
                        {
                            byte byte3 = compressedData[currentPosition + forward + 2];
                            
                            amountToCopy = 0x11 + ((byte1 & 0xF) << 4) + (byte2 >> 4);
                            copyFrom = 1 + ((byte2 & 0xF) << 8) + byte3;
                            forward += 3;
                        }
                        else
                        {
                            amountToCopy = 0x1 + (byte1 >> 4);
                            copyFrom = 1 + ((byte1 & 0xF) << 8) + byte2;
                            forward += 2;
                        }
                        
                        int copyPosition = uncompressedData.Count - copyFrom;
                        for (int u = 0; u < amountToCopy; u++)
                        {
                            if ((copyPosition + (u % copyFrom)) < uncompressedData.Count)
                            {
                                uncompressedData.Add(uncompressedData[copyPosition + (u % copyFrom)]);
                            }
                            else
                            {
                                return uncompressedData.ToArray();
                            }
                        }
                    }
                    else
                    {
                        uncompressedData.Add(compressedData[currentPosition + forward]);
                        forward++;
                    }
                }
                
                currentPosition += forward;
            }
            
            while (uncompressedData.Count < size)
            {
                uncompressedData.Add(0);
            }
            
            return uncompressedData.ToArray();
        }
        
        /// <summary>
        /// Decompresses an arm9 executable with a variant of LZSS encoding specific to binary files.
        /// </summary>
        /// <param name="compressedData">The compressed arm9 file data.</param>
        /// <param name="headerLength">The length of the uncompressed header (usually 0x4000).</param>
        /// <param name="footer">The arm9-specific footer that needs to be re-added when the file is
        /// compressed.
        /// </param>
        /// <returns>The decompressed arm9 file data.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
                                                         Justification = "Need to save variables for later")]
        public static byte[] DecompressArm9(byte[] compressedData, out int headerLength, out byte[] footer)
        {
            int arm9Length = compressedData.Length - Arm9FooterLength;
            int compressedLength = BitConverter.ToInt32(compressedData, arm9Length - 0x08);
            headerLength = arm9Length - (compressedLength & 0x00FFFFFF);
            
            footer = new byte[Arm9FooterLength];
            Array.Copy(compressedData, arm9Length, footer, 0, footer.Length);
            
            byte[] binaryData = new byte[compressedData.Length - Arm9FooterLength];
            Array.Copy(compressedData, 0, binaryData, 0, binaryData.Length);
            
            return DecompressBinary(binaryData);
        }
        
        /// <summary>
        /// Decompresses an overlay file with a variant of LZSS encoding specific to binary files.
        /// </summary>
        /// <param name="compressedData">The compressed overlay file data.</param>
        /// <returns>The decompressed overlay file data.</returns>
        public static byte[] DecompressOverlay(byte[] compressedData)
        {
            return DecompressBinary(compressedData);
        }
        
        /// <summary>
        /// Compresses an arm9 executable with a variant of LZSS encoding specific to binary files.
        /// </summary>
        /// <param name="uncompressedData">The uncompressed arm9 file data.</param>
        /// <param name="headerLength">The length of the uncompressed header (usually 0x4000).</param>
        /// <param name="footer">The footer stripped from the original arm9 file during decompression.</param>
        /// <returns>The compressed arm9 file data.</returns>
        public static byte[] CompressArm9(byte[] uncompressedData, int headerLength, byte[] footer)
        {
            // Perform the standard compression routine
            byte[] compressedData = CompressBinary(uncompressedData, headerLength);
            
            // Arm9 executables have an additional footer pointing to the location in the decompressed section that
            // holds some important pointers, which needs to be appended to the compressed data
            byte[] arm9Data = new byte[compressedData.Length + footer.Length];
            Array.Copy(compressedData, 0, arm9Data, 0, compressedData.Length);
            Array.Copy(footer, 0, arm9Data, compressedData.Length, footer.Length);
            
            // Update the pointer to the end of the compressed data
            int footerPointerOffset = BitConverter.ToInt32(footer, Constants.PointerLength) + 0x14;
            StreamHelper.WriteBytes(compressedData.Length | 0x02000000, arm9Data, footerPointerOffset);
            
            return arm9Data;
        }
        
        /// <summary>
        /// Compresses an overlay file with a variant of LZSS encoding specific to binary files.
        /// </summary>
        /// <param name="uncompressedData">The uncompressed overlay file data.</param>
        /// <returns>The compressed overlay file data.</returns>
        public static byte[] CompressOverlay(byte[] uncompressedData)
        {
            // Overlays are 100% compressed, so no need to deal with a header.
            return CompressBinary(uncompressedData, 0);
        }
        
        private static byte[] Compress(byte[] uncompressedData, int readAheadBufferSize, byte compressionType, bool addHeader, int distance)
        {
            List<byte> compressedData = new List<byte>();
            Queue<byte> readAheadBuffer = new Queue<byte>(readAheadBufferSize);
            List<byte> slidingWindow = new List<byte>(SlidingWindowSize);

            int position = 0;
            if (addHeader)
            {
                compressedData.Add(compressionType);
                compressedData.AddRange(BitConverter.GetBytes(uncompressedData.Length));
                compressedData.RemoveAt(4);
            }

            while (position < readAheadBufferSize)
            {
                readAheadBuffer.Enqueue(uncompressedData[position]);
                position++;
            }

            while (readAheadBuffer.Count > 0)
            {
                bool[] isCompressed = new bool[BlockSize];
                List<byte[]> data = new List<byte[]>();

                for (int i = BlockSize - 1; i >= 0; i--)
                {
                    int[] dataSeed = Search(slidingWindow, readAheadBuffer.ToArray(), distance);
                    byte[] datum;

                    if (dataSeed[1] > 2)
                    {
                        isCompressed[i] = true;
                        datum = new byte[2];
                        datum[0] = (byte)(((dataSeed[1] - (readAheadBufferSize - 0xF)) & 0xF) << 4);
                        datum[0] += (byte)(((dataSeed[0] - distance) >> 8) & 0xF);
                        datum[1] = (byte)((dataSeed[0] - distance) & 0xFF);
                        data.Add(datum);
                    }
                    else if (dataSeed[1] >= 0)
                    {
                        dataSeed[1] = 1;
                        datum = new byte[1] { readAheadBuffer.Peek() };
                        isCompressed[i] = false;
                        data.Add(datum);
                    }
                    else
                    {
                        isCompressed[i] = false;
                    }

                    for (int u = 0; u < dataSeed[1]; u++)
                    {
                        if (slidingWindow.Count >= SlidingWindowSize)
                        {
                            slidingWindow.RemoveAt(0);
                        }
                        
                        slidingWindow.Add(readAheadBuffer.Dequeue());
                    }

                    while ((readAheadBuffer.Count < readAheadBufferSize) && (position < uncompressedData.Length))
                    {
                        readAheadBuffer.Enqueue(uncompressedData[position]);
                        position++;
                    }
                }

                byte blockData = 0;
                for (int i = 0; i < BlockSize; i++)
                {
                    if (isCompressed[i])
                    {
                        blockData += (byte)(1 << i);
                    }
                }
                
                compressedData.Add(blockData);
                foreach (byte[] var in data)
                {
                    foreach (byte n in var)
                    {
                        compressedData.Add(n);
                    }
                }
            }

            return compressedData.ToArray();
        }
        
        private static int[] Search(List<byte> slidingWindow, byte[] readAheadBuffer, int distance)
        {
            if (readAheadBuffer.Length == 0)
            {
                return new int[2] { 0, -1 };
            }

            List<int> offsets = new List<int>();

            for (int i = 0; i < slidingWindow.Count - distance; i++)
            {
                if (slidingWindow[i] == readAheadBuffer[0])
                {
                    offsets.Add(i);
                }
            }

            if (offsets.Count == 0)
            {
                return new int[2] { 0, 0 };
            }

            for (int i = 1; i < readAheadBuffer.Length; i++)
            {
                for (int u = 0; u < offsets.Count; u++)
                {
                    if (slidingWindow[offsets[u] + (i % (slidingWindow.Count - offsets[u]))] != readAheadBuffer[i])
                    {
                        if (offsets.Count > 1)
                        {
                            offsets.Remove(offsets[u]);
                            u--;
                        }
                    }
                }
                
                if (offsets.Count < 2)
                {
                    i = readAheadBuffer.Length;
                }
            }
            
            int size = 1;
            bool keepGoing = true;
            while ((readAheadBuffer.Length > size) && keepGoing)
            {
                if (slidingWindow[offsets[0] + (size % (slidingWindow.Count - offsets[0]))] == readAheadBuffer[size])
                {
                    size++;
                }
                else
                {
                    keepGoing = false;
                }
            }

            return new int[2] { slidingWindow.Count - offsets[0], size };
        }
        
        private static byte[] Compress(byte[] uncompressedData, int readAheadBufferSize, byte compressionType)
        {
            return Compress(uncompressedData, readAheadBufferSize, compressionType, true, 1);
        }
        
        private static byte[] DecompressLzss(byte[] compressedData, int size, int currentPosition, int distance)
        {
            List<byte> uncompressedData = new List<byte>();
            while (uncompressedData.Count <= size && currentPosition < compressedData.Length)
            {
                int forward = 1;
                for (int i = 0; i < 8; i++)
                {
                    if (currentPosition + forward >= compressedData.Length)
                    {
                        break;
                    }
                    
                    if (uncompressedData.Count >= size)
                    {
                        break;
                    }
                    
                    if (((compressedData[currentPosition] >> (7 - i)) & 1) == 1)
                    {
                        int amountToCopy = 3 + ((compressedData[currentPosition + forward] >> 4) & 0xF);
                        int copyFrom = distance + ((compressedData[currentPosition + forward] & 0xF) << 8)
                            + compressedData[currentPosition + forward + 1];
                        int copyPosition = uncompressedData.Count - copyFrom;
                        for (int u = 0; u < amountToCopy; u++)
                        {
                            if ((copyPosition + (u % copyFrom)) < uncompressedData.Count)
                            {
                                uncompressedData.Add(uncompressedData[copyPosition + (u % copyFrom)]);
                            }
                            else
                            {
                                return uncompressedData.ToArray();
                            }
                        }
                        
                        forward += 2;
                    }
                    else
                    {
                        uncompressedData.Add(compressedData[currentPosition + forward]);
                        forward++;
                    }
                }
                
                currentPosition += forward;
            }
            
            while (uncompressedData.Count < size)
            {
                uncompressedData.Add(0);
            }
            
            return uncompressedData.ToArray();
        }
        
        /// <summary>
        /// Decompresses arm9/overlays with a variant of LZSS encoding specific to binary files. The files have an
        /// uncompressed header section followed by a compressed body, but they are actually decompressed backwards
        /// starting from the end of the file until reaching the header. This approach allows the file to be
        /// decompressed in-place once it is loaded into memory, eventually overwriting the entire compressed section
        /// with the decompressed file as it works backwards to the header.
        /// </summary>
        /// <param name="compressedData">The compressed binary file data.</param>
        /// <returns>The decompressed binary file data.</returns>
        private static byte[] DecompressBinary(byte[] compressedData)
        {
            // After the end of the compressed binary data there is a footer that contains information necessary to
            // decompress the data. The footer must be aligned to a 4-bye boundary, so the compressed data is padded
            // with FF if necessary
            int compressedLengthPointer = compressedData.Length - (2 * Constants.PointerLength);
            int compressedLength = BitConverter.ToInt32(compressedData, compressedLengthPointer);
            
            // First comes the length of the compressed data (not including the uncompressed header) which takes up 3
            // bytes
            int compressionStartOffset = compressedData.Length - (compressedLength & 0x00FFFFFF);
            
            // Then the 4th byte (5th from the end) holds the length of the footer (including any padding)
            int compressionEndOffset = compressedData.Length - compressedData[compressedData.Length - 5];
            
            // Next comes the difference between the file sizes of the compressed and uncompressed binaries, which
            // tells the processor how far out to put the start of the decompressed data
            int decompressedLengthPointer = compressedData.Length - Constants.PointerLength;
            int decompressedLength = BitConverter.ToInt32(compressedData, decompressedLengthPointer) + compressedData.Length;
            
            int headerLength = compressionStartOffset;
            
            // Make a copy of the data that needs to be compressed and reverse it so the standard LZ77 decompression
            // algorithm can handle it
            byte[] inputData = new byte[compressionEndOffset - compressionStartOffset];
            for (int destOffset = 0, sourceOffset = compressionEndOffset - 1; sourceOffset >= compressionStartOffset; sourceOffset--)
            {
                inputData[destOffset++] = compressedData[sourceOffset];
            }
            
            // Decompress the data now that it is in the correct order
            byte[] outputData = DecompressLzss(inputData, decompressedLength - headerLength, 0, BinaryMinDistance);
            
            // Create a new array to hold the entire file
            byte[] decompressedData = new byte[headerLength + outputData.Length];
            
            // Copy the uncompressed header directly from the original data
            Array.Copy(compressedData, 0, decompressedData, 0, headerLength);
            
            // Append the decompressed data, returning it to reverse order
            for (int destOffset = headerLength, sourceOffset = outputData.Length - 1; sourceOffset >= 0; sourceOffset--)
            {
                decompressedData[destOffset++] = outputData[sourceOffset];
            }
            
            return decompressedData;
        }
        
        /// <summary>
        /// Compresses arm9/overlays with a variant of LZSS encoding specific to binary files. The files have an
        /// uncompressed header section followed by a compressed body, but they are actually decompressed backwards
        /// starting from the end of the file until reaching the header. This approach allows the file to be
        /// decompressed in-place once it is loaded into memory, eventually overwriting the entire compressed section
        /// with the decompressed file as it works backwards to the header.
        /// </summary>
        /// <param name="uncompressedData">The raw binary file data.</param>
        /// <param name="headerLength">The length of the uncompressed header (usually 0x4000).</param>
        /// <returns>The compressed binary file data.</returns>
        private static byte[] CompressBinary(byte[] uncompressedData, int headerLength)
        {
            // Make a copy of the data that needs to be compressed and reverse it so the standard LZ77 compression
            // algorithm can handle it
            byte[] inputData = new byte[uncompressedData.Length - headerLength];
            for (int destOffset = 0, sourceOffset = uncompressedData.Length - 1; sourceOffset >= headerLength; sourceOffset--)
            {
                inputData[destOffset++] = uncompressedData[sourceOffset];
            }
            
            // Compress the data now that it is in the correct order
            byte[] outputData = Compress(inputData, LzssBufferSize, OnzCompressionType, false, BinaryMinDistance);
            
            int outputDataLength = headerLength + outputData.Length;
            int compressedDataLength = RoundUp(outputDataLength) + (2 * Constants.PointerLength);
            
            // Create a new array to hold the entire file
            byte[] compressedData = new byte[compressedDataLength];
            
            // Copy the uncompressed header directly from the original data
            Array.Copy(uncompressedData, 0, compressedData, 0, headerLength);
            
            // Append the compressed data, returning it to reverse order
            for (int destOffset = headerLength, sourceOffset = outputData.Length - 1; sourceOffset >= 0; sourceOffset--)
            {
                compressedData[destOffset++] = outputData[sourceOffset];
            }
            
            // Pad the compressed data to the next 4-byte boundary, so the length information in the footer will be
            // properly aligned
            for (int i = outputDataLength; i < RoundUp(outputDataLength); i++)
            {
                compressedData[i] = Padding;
            }
            
            // First comes the length of the compressed data (not including the uncompressed header) which takes up 3
            // bytes, with the 4th byte holding the length of the footer (including any padding)
            int footerLength = compressedDataLength - outputDataLength;
            int compressedLengthPointer = (compressedDataLength - headerLength) | (footerLength << 24);
            StreamHelper.WriteBytes(compressedLengthPointer, compressedData, compressedDataLength - (2 * Constants.PointerLength));
            
            // Next comes the difference between the file sizes of the compressed and uncompressed binaries, which
            // tells the processor how far out to put the start of the decompressed data
            int decompressedLengthPointer = uncompressedData.Length - compressedDataLength;
            StreamHelper.WriteBytes(decompressedLengthPointer, compressedData, compressedDataLength - Constants.PointerLength);
            
            return compressedData;
        }
        
        /// <summary>
        /// Rounds the given offset up to the nearest multiple of 4.
        /// Used for adding padding to keep pointers aligned to a valid memory address.
        /// </summary>
        /// <param name="offset">The offset to round.</param>
        /// <returns>The next offset aligned to a 4-byte boundary.</returns>
        private static int RoundUp(int offset)
        {
            return (int)(Math.Ceiling((double)offset / Constants.PointerLength) * Constants.PointerLength);
        }
    }
}
