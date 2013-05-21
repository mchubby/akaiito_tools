using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace Hanasaku
{
    public class LZSS
    {
        public const int THIS_IMPL_BUFFER_LENGTH = 0x400;
        public const int THIS_IMPL_BUFFER_INITIAL_OFFSET = 0x3EE;
        public const int THIS_IMPL_COMP_BLOCK_SIZE = 8; // 8 bits per control byte
        public const int THIS_IMPL_COMP_MIN_LENGTH = 3;
        public const int THIS_IMPL_COMP_MAX_LENGTH = 12;
        public const int THIS_IMPL_COMP_READ_AHEAD = 18;

        public static void Decompress(Stream instream, long inLength,
                                       Stream outstream, long decompressedSize)
        {
            /*
                Repeat below. Each Flag Byte followed by eight Blocks.
                Flag data (8bit)
                  Bit 0-7   Type Flags for next 8 Blocks, LSB first
                Block Type 0 - Uncompressed - Copy 1 Byte from Source to Dest
                  Bit 0-7   One data byte to be copied to dest
                Block Type 1 - Compressed - Copy N+3 Bytes from ringbuffer'offset to Dest
                  Bit 0-3   offset MSBs
                  Bit 4-7   Number of bytes to copy (minus 3)
                  Bit 8-15  offset LSBs
            */
            long readBytes = 0;

            // the maximum offset is 0x3FF.
            const int bufferLength = LZSS.THIS_IMPL_BUFFER_LENGTH;
            byte[] buffer = new byte[bufferLength];  // Note: CLR guarantees 0x00 cleared
            int bufferOffset = LZSS.THIS_IMPL_BUFFER_INITIAL_OFFSET;


            long currentOutSize = 0;
            int flags = 0, mask = 0x80;
            while (currentOutSize < decompressedSize)
            {
                // (throws when requested new flags byte is not available)
                #region Update the mask. If all flag bits have been read, get a new set.
                // the current mask is the mask used in the previous run. So if it masks the
                // last flag bit, get a new flags byte.
                if (mask == 0x80)
                {
                    if (readBytes >= inLength)
                        throw new NotEnoughDataException(currentOutSize, decompressedSize);
                    flags = instream.ReadByte(); readBytes++;
                    if (flags < 0)
                        throw new StreamTooShortException();
                    mask = 0x01;
                }
                else
                {
                    mask <<= 1; // This impl treats bits LSB to MSB
                }
                #endregion

                // bit = 0 <=> compressed.
                if ((flags & mask) == 0)
                {
                    // (throws when < 2 bytes are available)
                    #region Get length and offset values from next 2 bytes
                    // there are < 2 bytes available when the end is at most 1 byte away
                    if (readBytes + 1 >= inLength)
                    {
                        // make sure the stream is at the end
                        if (readBytes < inLength)
                        {
                            instream.ReadByte(); readBytes++;
                        }
                        throw new NotEnoughDataException(currentOutSize, decompressedSize);
                    }
                    int byte1 = instream.ReadByte(); readBytes++;
                    int byte2 = instream.ReadByte(); readBytes++;
                    if (byte2 < 0)
                        throw new StreamTooShortException();

                    // the number of bytes to copy
                    int length = byte2 & 0x0F;
                    length += 3;

                    #endregion

                    if (currentOutSize + length > decompressedSize)
                    {
                        throw new TooMuchInputException(readBytes, inLength, String.Format("Trying to write sliding window reference len={0} past EOF={1}", length, decompressedSize));
                    }
                    int bufIdx = ((byte2 >> 4) << 8) | byte1;
                    for (int i = 0; i < length; i++)
                    {
                        byte next = buffer[bufIdx % bufferLength];
                        bufIdx++;
                        outstream.WriteByte(next);
                        buffer[bufferOffset] = next;
                        bufferOffset = (bufferOffset + 1) % bufferLength;
                    }
                    currentOutSize += length;
                }
                else
                {
                    if (readBytes >= inLength)
                        throw new NotEnoughDataException(currentOutSize, decompressedSize);
                    int next = instream.ReadByte(); readBytes++;
                    if (next < 0)
                        throw new StreamTooShortException();
                        
                    currentOutSize++;
                    outstream.WriteByte((byte)next);
                    buffer[bufferOffset] = (byte)next;
                    bufferOffset = (bufferOffset + 1) % bufferLength;
                }
            }

//            if (readBytes < inLength)
//                throw new TooMuchInputException(readBytes, inLength);

        }

        public static int Compress(Stream instream, long inLength, Stream outstream)
        {
            BinaryWriter writer = new BinaryWriter(outstream);

            int readAheadBufferSize = LZSS.THIS_IMPL_COMP_READ_AHEAD;
            int distance = 1;

            // Erm, ok the following is... inefficient at best
            byte[] uncompressedData = new byte[inLength];
            {
                int offset = 0;
                int remaining = (int)inLength;
                while (remaining > 0)
                {
                    int read = instream.Read(uncompressedData, offset, remaining);
                    if (read <= 0)
                        throw new EndOfStreamException
                            (String.Format("End of stream reached with {0} bytes left to read", remaining));
                    remaining -= read;
                    offset += read;
                }
            }

            int bufferOffset = LZSS.THIS_IMPL_BUFFER_INITIAL_OFFSET;
            int position = 0;
            Queue<byte> readAheadBuffer = new Queue<byte>((int)inLength);
            List<byte> slidingWindow = new List<byte>(LZSS.THIS_IMPL_BUFFER_LENGTH);

            int writtenBytes = 0;


            while (position < readAheadBufferSize && position < inLength)
            {
                readAheadBuffer.Enqueue(uncompressedData[position]);
                position++;
            }

            while (readAheadBuffer.Count > 0)
            {
                bool[] isCompressed = new bool[LZSS.THIS_IMPL_COMP_BLOCK_SIZE];
                List<byte[]> data = new List<byte[]>();

                for (int i = LZSS.THIS_IMPL_COMP_BLOCK_SIZE - 1; i >= 0; i--)
                {
#if DEBUG
                    byte[] racont = readAheadBuffer.ToArray();
#endif
                    int[] dataSeed = Search(slidingWindow, readAheadBuffer.ToArray(), distance);
                    byte[] datum;

                    // Encoding a match
                    if (dataSeed[1] >= LZSS.THIS_IMPL_COMP_MIN_LENGTH)
                    {
                        isCompressed[i] = true;
                        datum = new byte[2];
                        int bufferMatchIndex = (bufferOffset + slidingWindow.Count - dataSeed[0]) % LZSS.THIS_IMPL_BUFFER_LENGTH;
                        datum[0] = (byte)(bufferMatchIndex & 0xFF);
                        datum[1] = (byte)((dataSeed[1] - LZSS.THIS_IMPL_COMP_MIN_LENGTH) & 0xF);
                        datum[1] += (byte)(((bufferMatchIndex >> 8) & 0xF) << 4);
                        data.Add(datum);
                    }
                    // Encoding a byte literal
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
                        if (slidingWindow.Count >= LZSS.THIS_IMPL_BUFFER_LENGTH)
                        {
                            // slide by one mark
                            bufferOffset = (bufferOffset + 1) % LZSS.THIS_IMPL_BUFFER_LENGTH;
                            slidingWindow.RemoveAt(0);
                        }

                        slidingWindow.Add(readAheadBuffer.Dequeue());
                    }


                    while ((readAheadBuffer.Count < readAheadBufferSize) && (position < uncompressedData.Length))
                    {
                        readAheadBuffer.Enqueue(uncompressedData[position]);
                        if ((position & 0x2fff) == 0x2000)
                        {
                            System.Console.Out.Write('.');
                        }
                        position++;
                    }
                }

                byte blockData = 0;
                // Control byte: LSB to MSB
                for (int i = LZSS.THIS_IMPL_COMP_BLOCK_SIZE - 1; i >= 0 ; i--)
                {
                    if (!isCompressed[i])
                    {
                        blockData += (byte)(1 << (LZSS.THIS_IMPL_COMP_BLOCK_SIZE - 1 - i));
                    }
                }

                writer.Write(blockData); ++writtenBytes;
                foreach (byte[] var in data)
                {
                    writer.Write(var); writtenBytes += var.Length;
                }
            }

            return writtenBytes;
        }


        internal static int[] Search(List<byte> slidingWindow, byte[] readAheadBuffer, int distance)
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
            while ((readAheadBuffer.Length > size) && size < LZSS.THIS_IMPL_COMP_MAX_LENGTH && keepGoing)
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

    }
}


