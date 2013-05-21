using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

using MiscUtil.IO;
using MiscUtil.Conversion;

using fastJSON;

using ProjProperties = global::cel2tiff.Properties;

namespace Hanasaku
{
    public class cel2tiff
    {
        #region .cel File Structures

        /// <summary>
        /// Header for a .cel file
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CelHeader
        {
            public UInt32 sigCel;
            public Int16 nReserved1;
            public Int16 nUnkSubType;
            public Int16 nEntrChunks;
            public Int16 nAnimChunks;
            public Int16 nImagChunks;
            public Int16 nReserved2;
            public UInt32 nDataLength;
            public UInt32 nUnknown1;
        }

        /// <summary>
        /// Those file structures represent info for .cel IMAG chunks
        /// </summary>
        internal class ImagInfo
        {
            public enum PixelFormat
            {
                //
                // Summary:
                //     Specifies that the format is 32 bits per pixel; 8 bits each are used for
                //     the alpha, red, green, and blue components.
                Format32bppAbgr = 0x07,
                //
                // Summary:
                //     Specifies that the format is 8 bits per pixel, indexed. The color table therefore
                //     has 256 color entries, using the Format32bppAbgr pixel format.
                Format8bppIndexed = 0x05
            }
            public static PixelFormat PixelFormatFromValue(long value)
            {
                return (PixelFormat)value;
            }
            public static System.Drawing.Imaging.PixelFormat MapToSystemPixelFormat(PixelFormat format)
            {
                switch (format)
                {
                    case PixelFormat.Format8bppIndexed:
                        return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
                    case PixelFormat.Format32bppAbgr:
                        // needs swapping B <--> R
                        return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                }
                return System.Drawing.Imaging.PixelFormat.Undefined;
            }

            public enum Compression
            {
                //
                // Summary:
                //     uncompressed
                CompressionNone = 0x00,
                //
                // Summary:
                //     LZSS-variant compression
                CompressionLzss = 0x01
            }

            public Compression CompressionAlgorithm { get; set; }
            public PixelFormat Format { get; set; }
            public long Height { get; set; }
            public long Size_c { get; set; }
            public long Size_d { get; set; }
            public long StreamOffset { get; set; }
            public long Width { get; set; }

        }

        /// <summary>
        /// Those are serialized info in the ExifMakerNote property field.
        /// Includes header and chunks info.
        /// </summary>
        [Serializable()]
        public class CelInfoDescriptor
        {
            public string Header { get; set; }
            public List<string> EntrColl { get; set; }
            public List<string> AnimColl { get; set; }
            public List<string> ImagColl { get; set; }

            public CelInfoDescriptor()
            {
                EntrColl = new List<string>();
                AnimColl = new List<string>();
                ImagColl = new List<string>();
            }

            public string ToJson()
            {
                fastJSON.JSON.Instance.UseSerializerExtension = false;
                return fastJSON.JSON.Instance.ToJSON(this);
            }

            public static CelInfoDescriptor FromJson(string json)
            {
                fastJSON.JSON.Instance.UseSerializerExtension = false;
                return fastJSON.JSON.Instance.ToObject<CelInfoDescriptor>(json);
            }
        }
        #endregion

        #region .cel Constants
        internal static readonly UInt32 CP10_SIG_CONST = new LittleEndianBitConverter().ToUInt32(new byte[] { 0x43, 0x50, 0x31, 0x30 }, 0);
        internal static readonly UInt32 ENTR_CHUNK_CONST = new LittleEndianBitConverter().ToUInt32(new byte[] { 0x45, 0x4E, 0x54, 0x52 }, 0);
        /// <summary>
        /// Size of an ENTR chunk. Fixed.
        /// </summary>
        internal static readonly UInt32 ENTR_CHUNK_SIZE = 44;  // may be padded
        internal static readonly UInt32 ANIM_CHUNK_CONST = new LittleEndianBitConverter().ToUInt32(new byte[] { 0x41, 0x4E, 0x49, 0x4D }, 0);
        /// <summary>
        /// Size of an ANIM chunk. Variable.
        /// </summary>
        internal static readonly UInt32 ANIM_CHUNK_SIZE = 20;  // variable size, may be padded
        internal static readonly UInt32 IMAG_CHUNK_CONST = new LittleEndianBitConverter().ToUInt32(new byte[] { 0x49, 0x4D, 0x41, 0x47 }, 0);
        /// <summary>
        /// Size of an IMAG chunk. Variable.
        /// </summary>
        internal static readonly UInt32 IMAG_CHUNK_SIZE = 32;  // variable size, may be padded

        internal static readonly UInt32 IMAG_LZ_HEADER_SIZE = 10;
        internal static readonly UInt32 ENDC_CHUNK_CONST = new LittleEndianBitConverter().ToUInt32(new byte[] { 0x45, 0x4E, 0x44, 0x43 }, 0);
        #endregion

        internal class CelChunk
        {
            /// <summary>
            /// Obtains the offset for next chunk based on end of current one.
            /// </summary>
            /// <param name="currentendpos"></param>
            /// <returns></returns>
            public static UInt32 GetAlignedChunkPosition(UInt32 currentendpos)
            {
                return (currentendpos & 0x0F) != 0 ? (currentendpos + 0x10) & 0xFFFFFFF0 : currentendpos;
            }
        }

        internal static void WriteCelBitmap(Image image, ImagInfo.PixelFormat pxFormat, Stream outputStream, SortedDictionary<UInt32, byte> forcedPalette, string contextDescription)
        {
            using (NonClosingStreamWrapper noCloseOut = new NonClosingStreamWrapper(outputStream))
            {
                if (!(image is Bitmap))
                {
                    throw new UnsupportedDataInput(String.Format("Input image for {0} should be a Bitmap instance",
                        contextDescription
                    ));
                }
                Bitmap bmp = (Bitmap)image;
#if test
                {
                    byte[] bmpBytes = new byte[bmp.Width * bmp.Height * 4];
                    BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                    // Copy the bytes into a byte array
                    for (int r = 0; r < bmp.Height; r++)
                    {
                        IntPtr rowaddr = new IntPtr((long)data.Scan0 + r * data.Stride);
                        Marshal.Copy(rowaddr, bmpBytes, r * bmp.Width * 4, bmp.Width * 4);
                    }
                    bmp.UnlockBits(data);
                }
#endif
                EndianBinaryWriter memwriter = new EndianBinaryWriter(EndianBitConverter.Little, noCloseOut);
                int inputBpp = 4;

//                bmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                switch(pxFormat)
                {
                    case ImagInfo.PixelFormat.Format32bppAbgr:
                        if (image.PixelFormat != ImagInfo.MapToSystemPixelFormat(pxFormat))
                        {
                            // casting up from 8bpp input to 32bpp, makes no sense
                            throw new UnsupportedDataInput(String.Format("Input pixelformat for {0} does not match: expected {1}, found {2} - it is a waste!?",
                                contextDescription,
                                Enum.GetName(typeof(System.Drawing.Imaging.PixelFormat), ImagInfo.MapToSystemPixelFormat(pxFormat)),
                                Enum.GetName(typeof(System.Drawing.Imaging.PixelFormat), image.PixelFormat)
                            ));
                        }

                        // at this point, image and output target match
                        inputBpp = 4;
                        {
                            BitmapData data = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                            byte[] bmpBytesRow = new byte[image.Width * inputBpp];
                            // Copy bytes from source (row by row) and write to stream
                            for (int r = 0; r < image.Height; r++)
                            {
                                IntPtr rowaddr = new IntPtr((long)data.Scan0 + r * data.Stride);
                                Marshal.Copy(rowaddr, bmpBytesRow, 0, image.Width * inputBpp);
                                memwriter.Write(bmpBytesRow);
                            }
                            bmp.UnlockBits(data);
                        }
                        break;

                    case ImagInfo.PixelFormat.Format8bppIndexed:

                    // First, determine if a conversion is needed at all
                    if (image.PixelFormat == ImagInfo.MapToSystemPixelFormat(pxFormat))
                    {
                        inputBpp = 1;
                        // # of palette entries
                        memwriter.WriteUint(0x100);
                        ColorPalette pal = image.Palette;
                        for (int index = 0, i = 0; index < 0x400; index += 4, ++i)
                        {
                            memwriter.Write(Color.FromArgb(pal.Entries[i].A, pal.Entries[i].B, pal.Entries[i].G, pal.Entries[i].R).ToArgb());
                        }
                        bmp.Palette = image.Palette;
                        BitmapData data = bmp.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

                        byte[] ddata = new byte[image.Width * image.Height * inputBpp];

                        int ddataoffset = 0;
                        // Copy the bytes from the image (row by row) into a byte array
                        for (int r = 0; r < image.Height; r++)
                        {
                            IntPtr rowaddr = new IntPtr((long)data.Scan0 + r * data.Stride);
                            Marshal.Copy(rowaddr, ddata, ddataoffset, image.Width * inputBpp);
                            ddataoffset += image.Width * inputBpp;
                        }
#if test
                        // dump ddata.ToArray() if wanted
#endif
                        bmp.UnlockBits(data);

                        memwriter.Write(ddata);
                    }
                    else
                    {
                        inputBpp = 4;
                        // else we need to 32bpp -> indexed 8bpp + swapping B <--> R
                        SortedDictionary<UInt32, byte> binPalette = forcedPalette;

                        // # of palette entries
                        memwriter.WriteUint(0x100);
                        UInt32[] sortedKeys;

                        if (binPalette == null)
                        {
                            binPalette = new SortedDictionary<UInt32, byte>();

                            if (image is Bitmap)
                            {
                                Bitmap asBmp = (Bitmap)image;
                                byte[] bmpBytesRow = new byte[asBmp.Width * 4];
                                BitmapData data = asBmp.LockBits(new Rectangle(0, 0, asBmp.Width, asBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                                // Copy the bytes from a byte array into the image
                                for (int r = 0; r < asBmp.Height; r++)
                                {
                                    IntPtr rowaddr = new IntPtr((long)data.Scan0 + r * data.Stride);
                                    Marshal.Copy(rowaddr, bmpBytesRow, 0, asBmp.Width * 4);
                                    // Peek pixel values to create a colormap
                                    for (int columnIndex = 0; columnIndex < asBmp.Width * 4; columnIndex += 4)
                                    {
                                        UInt32 toPal = (UInt32)Color.FromArgb(
                                            bmpBytesRow[columnIndex + 3], // original A
                                            bmpBytesRow[columnIndex], // original B
                                            bmpBytesRow[columnIndex + 1], // original G
                                            bmpBytesRow[columnIndex + 2] // original R
                                        ).ToArgb();
                                        if (!binPalette.ContainsKey(toPal))
                                        {
                                            binPalette[toPal] = 1;
                                        }
                                    }
                                }
                                asBmp.UnlockBits(data);
                            }

                            if (binPalette.Count > 256)
                            {
                                throw new UnsupportedDataInput(String.Format("Input unique color count for {0} exceeds 256: found {1} - apply color reduction processing first",
                                    contextDescription,
                                    binPalette.Count
                                ));
                            }
                        }

                        sortedKeys = new UInt32[binPalette.Keys.Count];
                        binPalette.Keys.CopyTo(sortedKeys, 0);
                        memwriter.WriteUint(sortedKeys);

                        int pali = 0;
                        foreach (UInt32 argbcolor in sortedKeys)
                        {
                            binPalette[argbcolor] = (byte)pali;  // Map (swapped, final) color to palette index
                            ++pali;
                        }

                        for (int palri = binPalette.Count; palri < 256; ++palri )
                        {
                            memwriter.WriteUint(0);
                        }

                        Queue<byte> dataqueue = new Queue<byte>(image.Width * image.Height);

//                        try
                        {
                            // yes, the bitmap is read twice
                            byte[] bmpBytesRow = new byte[bmp.Width * 4];
                            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                            // Copy the bytes from a byte array into the image
                            for (int r = 0; r < bmp.Height; r++)
                            {
                                IntPtr rowaddr = new IntPtr((long)data.Scan0 + r * data.Stride);
                                Marshal.Copy(rowaddr, bmpBytesRow, 0, bmp.Width * 4);
                                // Peek pixel values to create a colormap
                                for (int columnIndex = 0; columnIndex < bmp.Width * 4; columnIndex += 4)
                                {
                                    UInt32 toPal = (UInt32)Color.FromArgb(
                                        bmpBytesRow[columnIndex + 3], // original A
                                        bmpBytesRow[columnIndex], // original B
                                        bmpBytesRow[columnIndex + 1], // original G
                                        bmpBytesRow[columnIndex + 2] // original R
                                    ).ToArgb();
                                    byte indexPal;
                                    if(!binPalette.TryGetValue(toPal, out indexPal))
                                    {
                                        throw new UnsupportedDataInput(String.Format("Color not found in palette for {0} - {1} - specified palette may not contain the appropriate values",
                                            contextDescription,
                                            toPal
                                        ));
                                    }
                                    dataqueue.Enqueue(binPalette[toPal]);  // color mapped to palette index
                                }
                            }
                            bmp.UnlockBits(data); 
                        }
//                        catch (System.Collections.Generic.KeyNotFoundException ex)
//                        {
//                            throw new UnsupportedDataInput(String.Format("Color not found in palette for {0} - {1} - specified palette may not contain the appropriate values",
//                                contextDescription,
//                                ex.Message
//                            ));
//                        }

                        byte[] ddata = dataqueue.ToArray();
#if test
                        // dump ddata.ToArray() if wanted
#endif
                        memwriter.Write(ddata);
                    }
                    break;
                    // end of case ImagInfo.PixelFormat.Format8bppIndexed
                }
            }
        }

        /// <summary>
        /// Reads a .cel, outputs a .tiff
        /// </summary>
        /// <param name="ifs">input, must not be <value>null</value></param>
        /// <param name="ofs">output, must not be <value>null</value></param>
        public static void ConvertCelToTiff(FileStream ifs, FileStream ofs, string outBase)
        {
            CelInfoDescriptor desc = new CelInfoDescriptor();
            List<ImagInfo> imagInfos = new List<ImagInfo>();

            using (NonClosingStreamWrapper noCloseOut = new NonClosingStreamWrapper(ofs))
            {
                BinaryWriter writer = new BinaryWriter(noCloseOut);

                CelHeader header = default(CelHeader);
                
                // Process header
                using (NonClosingStreamWrapper noCloseStream = new NonClosingStreamWrapper(ifs))
                {
                    ifs.Seek(0, SeekOrigin.Begin);
                    BinaryReader reader = new BinaryReader(noCloseStream);
                    try
                    {
                        header = BinaryIO.FromBinaryReader<CelHeader>(reader);
                    }
                    catch (StreamTooShortException ex)
                    {
                        throw new UnsupportedDataInput("Attempt to read .cel header - " + ex.Message);
                    }
                    // NonClosingStreamWrapper still ok even after reader disposal
                }

                long maxSeekOffset = Marshal.SizeOf(typeof(CelHeader)) + header.nDataLength;

                if (header.sigCel != CP10_SIG_CONST)
                {
                    throw new UnsupportedDataInput("Unsupported magic; 'CP10' wanted");
                }

                // Usually, header.nEntrChunks is equal to either header.nAnimChunks or header.nImagChunks
                // but it's not always the case
                if (header.nEntrChunks <= 0
                    || (header.nEntrChunks > header.nAnimChunks + header.nImagChunks)
                )
                {
                    throw new UnsupportedDataInput("input .cel declares unrealistic ENTR chunk count, or chunk count does not match");
                }
                if (header.nDataLength < 0 || header.nDataLength > 20 * 1024 * 1024)
                {
                    throw new UnsupportedDataInput("input .cel declares unrealistic datastream size, possible corruption?");
                }

                desc.Header = System.Convert.ToBase64String(BinaryIO.ToByteArray(header), Base64FormattingOptions.None);


                
                
                using (NonClosingStreamWrapper noCloseStream = new NonClosingStreamWrapper(ifs))
                {
                    // fine as long as both readers share the same scope (are marked for disposal at the same time)
                    EndianBinaryReader reader = new EndianBinaryReader(EndianBitConverter.Little, noCloseStream);
                    EndianBinaryReader bereader = new EndianBinaryReader(EndianBitConverter.Big, noCloseStream);

                    try
                    {
                        // Process ENTR chunks
                        long nextChunkOffset = 0x18;

                        for (int i = 0; i < header.nEntrChunks; ++i)
                        {
                            long chunkBaseOffset = nextChunkOffset;
                            ifs.Seek(chunkBaseOffset, SeekOrigin.Begin);
                            long minNextChunkOffset = nextChunkOffset + ENTR_CHUNK_SIZE;
                            if (reader.ReadUInt32() != ENTR_CHUNK_CONST)
                            {
                                throw new UnsupportedDataInput(String.Format("'ENTR' type field expected at chunk header (@<0x{0:X8}>)", chunkBaseOffset));
                            }
                            nextChunkOffset = reader.ReadUInt32();
                            if (nextChunkOffset >= maxSeekOffset || nextChunkOffset < minNextChunkOffset)
                            {
                                throw new UnsupportedDataInput(String.Format("'ENTR' declares next chunk at invalid offset = <0x{0:X8}> (@<0x{1:X8}>)", nextChunkOffset, chunkBaseOffset + 4));
                            }

                            ifs.Seek(chunkBaseOffset, SeekOrigin.Begin);
                            byte[] bytes = reader.ReadBytes((int)ENTR_CHUNK_SIZE);
                            desc.EntrColl.Add(System.Convert.ToBase64String(bytes, Base64FormattingOptions.None));
                        }

                        // Process ANIM chunks (retaining current value of nextChunkOffset)
                        for (int i = 0; i < header.nAnimChunks; ++i)
                        {
                            long chunkBaseOffset = nextChunkOffset;
                            ifs.Seek(chunkBaseOffset, SeekOrigin.Begin);
                            long minNextChunkOffset = nextChunkOffset + ANIM_CHUNK_SIZE + 4;
                            if (reader.ReadUInt32() != ANIM_CHUNK_CONST)
                            {
                                throw new UnsupportedDataInput(String.Format("'ANIM' type field expected at chunk header (@<0x{0:X8}>)", chunkBaseOffset));
                            }
                            nextChunkOffset = reader.ReadUInt32();
                            if (nextChunkOffset >= maxSeekOffset || nextChunkOffset < minNextChunkOffset)
                            {
                                throw new UnsupportedDataInput(String.Format("'ANIM' declares next chunk at invalid offset = <0x{0:X8}> (@<0x{1:X8}>)", nextChunkOffset, chunkBaseOffset + 4));
                            }
                            ifs.Seek(chunkBaseOffset + 16, SeekOrigin.Begin);
                            UInt32 numAnimCmd = reader.ReadUInt32();
                            if (numAnimCmd > 1000 || (chunkBaseOffset + ANIM_CHUNK_SIZE + 4 * numAnimCmd) >= maxSeekOffset)
                            {
                                throw new UnsupportedDataInput(String.Format("'ANIM' declares unrealistic number of animation commands, possible corruption? (@<0x{1:X8}>)", chunkBaseOffset + 16));
                            }
                            ifs.Seek(chunkBaseOffset, SeekOrigin.Begin);
                            byte[] bytes = reader.ReadBytes((int)(ANIM_CHUNK_SIZE + 4 * numAnimCmd));
                            desc.AnimColl.Add(System.Convert.ToBase64String(bytes, Base64FormattingOptions.None));
                        }

                        // Process IMAG chunks (retaining current value of nextChunkOffset)
                        for (int i = 0; i < header.nImagChunks; ++i)
                        {
                            long chunkBaseOffset = nextChunkOffset;
                            ifs.Seek(chunkBaseOffset, SeekOrigin.Begin);
                            long minNextChunkOffset = nextChunkOffset + IMAG_CHUNK_SIZE + 6;
                            if (reader.ReadUInt32() != IMAG_CHUNK_CONST)
                            {
                                throw new UnsupportedDataInput(String.Format("'IMAG' type field expected at chunk header (@<0x{0:X8}>)", chunkBaseOffset));
                            }
                            nextChunkOffset = reader.ReadUInt32();
                            if (nextChunkOffset >= maxSeekOffset || nextChunkOffset < minNextChunkOffset)
                            {
                                throw new UnsupportedDataInput(String.Format("'IMAG' declares next chunk at invalid offset = <0x{0:X8}> (@<0x{1:X8}>)", nextChunkOffset, chunkBaseOffset + 4));
                            }
                            ifs.Seek(chunkBaseOffset + 28, SeekOrigin.Begin);
                            UInt32 nDataLen = reader.ReadUInt32();
                            if ((chunkBaseOffset + IMAG_CHUNK_SIZE + nDataLen) >= maxSeekOffset)
                            {
                                throw new UnsupportedDataInput(String.Format("'IMAG' declares data size past end of file (@<0x{1:X8}>)", chunkBaseOffset + 28));
                            }

                            ifs.Seek(chunkBaseOffset + 0x0C, SeekOrigin.Begin);
                            UInt16 width = reader.ReadUInt16();
                            UInt16 height = reader.ReadUInt16();
                            UInt32 imagetype = reader.ReadUInt32();
                            ifs.Seek(chunkBaseOffset + 0x18, SeekOrigin.Begin);
                            UInt32 comptype = reader.ReadUInt32();
                            UInt32 datalen = reader.ReadUInt32();
                            UInt32 size_d = 0;
                            UInt32 size_c = 0;
                            switch ((ImagInfo.Compression)comptype)
                            {
                                case ImagInfo.Compression.CompressionNone:
                                    size_d = datalen;
                                    size_c = datalen;
                                    break;
                                case ImagInfo.Compression.CompressionLzss:

                                    ifs.Seek(chunkBaseOffset + cel2tiff.IMAG_CHUNK_SIZE, SeekOrigin.Begin);
                                    ifs.Seek(0x02, SeekOrigin.Current);
                                    size_d = bereader.ReadUInt32();
                                    size_c = bereader.ReadUInt32();
                                    break;
                            }
                            
                            imagInfos.Add(new ImagInfo()
                            {
                                CompressionAlgorithm = (ImagInfo.Compression)comptype,
                                Format = ImagInfo.PixelFormatFromValue(imagetype),
                                Height = height,
                                Size_c = size_c,
                                Size_d = size_d,
                                StreamOffset = chunkBaseOffset,
                                Width = width
                            });

                            ifs.Seek(chunkBaseOffset, SeekOrigin.Begin);
                            byte[] bytes = reader.ReadBytes((int)(IMAG_CHUNK_SIZE));
                            desc.ImagColl.Add(System.Convert.ToBase64String(bytes, Base64FormattingOptions.None));
                        }

                        //
                        {
                            long chunkBaseOffset = nextChunkOffset;
                            ifs.Seek(chunkBaseOffset, SeekOrigin.Begin);
                            if (reader.ReadUInt32() != ENDC_CHUNK_CONST)
                            {
                                throw new UnsupportedDataInput(String.Format("'ENDC' type field expected at chunk header (@<0x{0:X8}>)", chunkBaseOffset));
                            }
                            long supposedEofOffset = reader.ReadUInt32();
                            if (supposedEofOffset != ifs.Position)
                            {
                                throw new UnsupportedDataInput(String.Format("'ENDC' value, expected filesize = <0x{0:X8}>, found <0x{1:X8}>", ifs.Position, supposedEofOffset));
                            }
                        }

                    }
                    catch (EndOfStreamException ex)
                    {
                        throw new ApplicationException(String.Format("Error: Attempt to read field (@<0x{0:X8}>) - ", ifs.Position) + ex.Message, ex);
                    }

                    Bitmap bitmap = new Bitmap(ProjProperties.Resources.frame0);
                    Image imageref = bitmap;
                    ImageUtil.Image_SetPropertyItemAscii(ref imageref, ImageUtil.PropertyTagExifMakerNote, desc.ToJson());

                    MemoryStream byteStream = new MemoryStream();
                    // Write tagged image to memstream as .tiff, then reload it
                    bitmap.Save(byteStream, ImageFormat.Tiff);
                    Image tiff = Image.FromStream(byteStream);
                    EncoderParameter parameter;

                    // Write a placeholder image
                    {
                        EncoderParameters firstFrameEncoderParams = new EncoderParameters(2);
                        {
                            //parameter = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionCCITT4);
                            parameter = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);
                            firstFrameEncoderParams.Param[0] = parameter;

                            parameter = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
                            firstFrameEncoderParams.Param[1] = parameter;
                        }
                        ImageCodecInfo encoderInfo = ImageUtil.GetEncoderInfo("image/tiff");
                        tiff.Save(noCloseOut, encoderInfo, firstFrameEncoderParams);
                    }


                    EncoderParameters pageEncoderParams = new EncoderParameters(3);
                    {
                        //parameter = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionCCITT4);
                        parameter = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);
                        pageEncoderParams.Param[0] = parameter;

                        parameter = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                        pageEncoderParams.Param[1] = parameter;

                        parameter = new EncoderParameter(Encoder.ColorDepth, 32L);
                        pageEncoderParams.Param[2] = parameter;
                    }
                    // Write pages
                    int chunkCount = 0;
                    foreach (ImagInfo info in imagInfos)
                    {
                        ++chunkCount;
                        ifs.Seek(info.StreamOffset + cel2tiff.IMAG_CHUNK_SIZE, SeekOrigin.Begin);

                        System.Drawing.Imaging.PixelFormat pxfmt = ImagInfo.MapToSystemPixelFormat(info.Format);
                        Bitmap bmp = new Bitmap((int)info.Width, (int)info.Height, pxfmt);

                        MemoryStream decompressedStream = new MemoryStream();
                        switch (info.CompressionAlgorithm)
                        {
                            case ImagInfo.Compression.CompressionNone:
                                BinaryIO.CopyTo(ifs, decompressedStream, (int)info.Size_c);
                                break;
                            case ImagInfo.Compression.CompressionLzss:
                                ifs.Seek(cel2tiff.IMAG_LZ_HEADER_SIZE, SeekOrigin.Current);
                                Hanasaku.LZSS.Decompress(ifs, info.Size_c, decompressedStream, info.Size_d);

                                string rawDumpPath = Path.ChangeExtension(outBase, String.Format("raw_{0}.lz77", chunkCount));
                                ifs.Seek(info.StreamOffset + cel2tiff.IMAG_CHUNK_SIZE, SeekOrigin.Begin);
                                using (FileStream fs = File.OpenWrite(rawDumpPath))
                                {
                                    BinaryIO.CopyTo(ifs, fs, (int)(info.Size_c + cel2tiff.IMAG_LZ_HEADER_SIZE));
                                }
                                break;
                        }
                        byte[] ddata = decompressedStream.ToArray();
#if test
                        string outfile = String.Format("out_{0:X}.bin", info.StreamOffset);
                        using (Stream fw = File.OpenWrite(outfile))
                        {
                            fw.Write(ddata, 0, ddata.Length);
                        }
                        System.Environment.Exit(3);
#endif
                        // Set defaults for uncompressed, 32bpp blob
                        int ddataSkipBytes = 0;
                        int Bpp = 4;

                        if (info.Format == ImagInfo.PixelFormat.Format8bppIndexed)
                        {
                            string rawDumpPath = Path.ChangeExtension(outBase, String.Format("{0}.palette.bin", chunkCount));
                            using (FileStream fs = File.OpenWrite(rawDumpPath))
                            {
                                fs.Write(ddata, 4, 4 * 0x100);
                            }
                            ColorPalette pal = bmp.Palette;
                            for (int index = 0, i = 0; index < 0x400; index += 4, ++i)
                            {
                                pal.Entries[i] = Color.FromArgb(
                                    ddata[index + 7],
                                    ddata[index + 4],
                                    ddata[index + 5],
                                    ddata[index + 6]
                                );
                            }
                            bmp.Palette = pal;
                            ddataSkipBytes += 0x404;
                            Bpp = 1;
                        }

                        // MSDN communities:
                        // For changing pixels in an image with an indexed format, you need to change the bytes constituting the image.
                        // You cannot use SetPixel.
                        BitmapData data = bmp.LockBits(new Rectangle(0, 0, (int)info.Width, (int)info.Height), ImageLockMode.WriteOnly, pxfmt);

                        // Copy the bytes from a byte array into the image
                        for (int r = 0; r < info.Height; r++)
                        {
                            IntPtr rowaddr = new IntPtr((long)data.Scan0 + r * data.Stride);
                            Marshal.Copy(ddata, ddataSkipBytes + (int)(r * info.Width * Bpp), rowaddr, (int)(info.Width * Bpp));
                        }

                        bmp.UnlockBits(data);

                        using (MemoryStream byteStream2 = new MemoryStream())
                        {
                            {
                                ImageCodecInfo encoderInfo = ImageUtil.GetEncoderInfo("image/tiff");
                                EncoderParameters tempEncoderParams = new EncoderParameters(2);
                                tempEncoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);
                                tempEncoderParams.Param[1] = new EncoderParameter(Encoder.ColorDepth, 32L);

#if DEBUG
                                string outfile = String.Format("outimag_{0:X}.tiff", info.StreamOffset);
                                using (Stream fw = File.OpenWrite(outfile))
                                {
                                    bmp.Save(fw, encoderInfo, tempEncoderParams);
                                }
#endif
                                bmp.Save(byteStream2, encoderInfo, tempEncoderParams);
                            }
                            Image img2 = Image.FromStream(byteStream2);
                            tiff.SaveAdd(img2, pageEncoderParams);
                        }
                    }

                }
            }
        }


        /// <summary>
        /// Reads a multipage .tiff, outputs a .cel
        /// </summary>
        /// <param name="ifs">input, must not be <value>null</value></param>
        /// <param name="ofs">output, must be seekable</param>
        public static void ConvertTiffToCel(FileStream ifs, FileStream ofs, string inBase)
        {
            CelInfoDescriptor desc = null;
            CelHeader header;
            List<ImagInfo> imagInfos = new List<ImagInfo>();

            Image inImg = null;

            // Read input
            using (NonClosingStreamWrapper noCloseStream = new NonClosingStreamWrapper(ifs))
            {
                try
                {
                    inImg = Image.FromStream(noCloseStream, false, true);
                }
                catch (ArgumentException ex)
                {
                    throw new UnsupportedDataInput("Input is not a valid image - " + ex.Message);
                }

                string serializedInfo = ImageUtil.Image_GetPropertyItemAscii(inImg, ImageUtil.PropertyTagExifMakerNote);
                if (serializedInfo == null)
                {
                    throw new UnsupportedDataInput("Input is lacking serialized data, maybe chopped out after it was edited?");
                }
                desc = CelInfoDescriptor.FromJson(serializedInfo);

                int numPage = inImg.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page);
                if (desc.ImagColl.Count + 1 != numPage)
                {
                    throw new UnsupportedDataInput(String.Format("Input TIFF does not have the appropriate page count (trimmed by image editor?). Expected: {0} - Found: {1}", desc.ImagColl.Count + 1, numPage));
                }

                //System.Console.Out.WriteLine(inImg.PixelFormat.ToString());
                using (NonClosingStreamWrapper noCloseOut = new NonClosingStreamWrapper(ofs))
                {
                    // fine as long as both readers share the same scope (are marked for disposal at the same time)
                    EndianBinaryWriter writer = new EndianBinaryWriter(EndianBitConverter.Little, noCloseOut);
                    EndianBinaryWriter bewriter = new EndianBinaryWriter(EndianBitConverter.Big, noCloseOut);
                    LittleEndianBitConverter lebc = new LittleEndianBitConverter();
                    UInt32 currentOffset = 0;

                    byte[] headerbytes = System.Convert.FromBase64String(desc.Header);
                    using (MemoryStream ms = new MemoryStream(headerbytes))
                    {
                        BinaryReader memreader = new BinaryReader(ms);
                        header = BinaryIO.FromBinaryReader<CelHeader>(memreader);
                    }
                    writer.Write(headerbytes);
                    currentOffset += (UInt32)headerbytes.Length;

                    int chunkCounter;

                    #region ENTR write
                    // There is always at least one ENTR chunk
                    chunkCounter = 0;
                    foreach (string s in desc.EntrColl)
                    {
                        byte[] bindata = System.Convert.FromBase64String(s);
                        UInt32 nextOffset = currentOffset + cel2tiff.ENTR_CHUNK_SIZE;
                        UInt32 origNextOffset = nextOffset;

                        // The chunk following the last ENTR chunk must be aligned
                        if (++chunkCounter == desc.EntrColl.Count)
                        {
                            nextOffset = CelChunk.GetAlignedChunkPosition(nextOffset);
                        }
                        // Replace at head of chunk
                        lebc.CopyBytes(nextOffset, bindata, 4);
                        writer.Write(bindata);

                        if (chunkCounter == desc.EntrColl.Count && nextOffset > origNextOffset)
                        {
                            writer.Seek((int)(nextOffset - origNextOffset), SeekOrigin.Current);
                        }

                        currentOffset = nextOffset;
                    }
                    #endregion

                    #region ANIM write
                    chunkCounter = 0;
                    foreach (string s in desc.AnimColl)
                    {
                        byte[] bindata = System.Convert.FromBase64String(s);
                        UInt32 nextOffset = currentOffset + (UInt32)bindata.Length;
                        UInt32 origNextOffset = nextOffset;

                        // The chunk following the last ANIM chunk must be aligned
                        if (++chunkCounter == desc.AnimColl.Count)
                        {
                            nextOffset = CelChunk.GetAlignedChunkPosition(nextOffset);
                        }
                        // Replace at head of chunk
                        lebc.CopyBytes(nextOffset, bindata, 4);
                        writer.Write(bindata);

                        if (chunkCounter == desc.AnimColl.Count && nextOffset > origNextOffset)
                        {
                            writer.Seek((int)(nextOffset - origNextOffset), SeekOrigin.Current);
                        }

                        currentOffset = nextOffset;
                    }
                    #endregion

                    #region IMAG write
                    // Note: Every IMAG chunk must be aligned
                    chunkCounter = 0;
                    foreach (string s in desc.ImagColl)
                    {
                        ++chunkCounter;
                        byte[] bindata = System.Convert.FromBase64String(s);

                        UInt16 metawidth = lebc.ToUInt16(bindata, 0x0C);
                        UInt16 metaheight = lebc.ToUInt16(bindata, 0x0E);
                        UInt32 metapxFormat = lebc.ToUInt32(bindata, 0x10);

                        inImg.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, chunkCounter);

                        if (metawidth != inImg.Width || metaheight != inImg.Height)
                        {
                            throw new UnsupportedDataInput(String.Format("Input dimensions for page #{0}/{1} do not match: expected {2}x{3}, found {4}x{5}", chunkCounter + 1, numPage, metawidth, metaheight, inImg.Width, inImg.Height)); // zero-base page numbering
                        }

                        byte[] compressedImage = null;
                        int writtenUncompressed = 0;

                        string rawDumpPath = Path.ChangeExtension(inBase, String.Format("raw_{0}.lz77", chunkCounter));

                        if (File.Exists(rawDumpPath))
                        {
                            // keep current metapxFormat
                            byte[] precompressed = File.ReadAllBytes(rawDumpPath);

                            if (precompressed.Length < cel2tiff.IMAG_LZ_HEADER_SIZE
                              || precompressed[0] != 0x4C || precompressed[1] != 0x5A)  // LZ
                            {
                                throw new UnsupportedDataInput(String.Format("Input for page #{0}/{1} is precompressed block (\"{2}\") but does not look like valid LZ77 data", chunkCounter + 1, numPage, rawDumpPath));
                            }
                            BigEndianBitConverter bebc = new BigEndianBitConverter();

                            writtenUncompressed = bebc.ToInt32(precompressed, 2);
                            int compressedDeclared = bebc.ToInt32(precompressed, 6);

                            if (precompressed.Length < cel2tiff.IMAG_LZ_HEADER_SIZE + compressedDeclared)
                            {
                                throw new UnsupportedDataInput(String.Format("Input for page #{0}/{1} is precompressed block (\"{2}\") but insufficient input: expected {3}, found {4}", chunkCounter + 1, numPage, rawDumpPath, cel2tiff.IMAG_LZ_HEADER_SIZE + compressedDeclared, precompressed.Length));
                            }
                            compressedImage = new byte[compressedDeclared];
                            Buffer.BlockCopy(precompressed, (int)cel2tiff.IMAG_LZ_HEADER_SIZE, compressedImage, 0, compressedImage.Length);
                        }
                        else
                        {
                            switch (inImg.PixelFormat)
                            {
                                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: metapxFormat = (uint)ImagInfo.PixelFormat.Format8bppIndexed; break;
                                case System.Drawing.Imaging.PixelFormat.Format32bppArgb: metapxFormat = (uint)ImagInfo.PixelFormat.Format32bppAbgr; break;
                                default: throw new UnsupportedDataInput(String.Format("Input pixelformat for page #{0}/{1} must be either Format8bppIndexed or Format32bppArgb", chunkCounter + 1, numPage)); // zero-base page numbering
                            }

                            string paletteIndicationPath = Path.ChangeExtension(inBase, String.Format("colormap_{0}.txt", chunkCounter));
                            SortedDictionary<UInt32, byte> forcedPalette = null;

                            if (File.Exists(paletteIndicationPath))
                            {
                                metapxFormat = (uint)ImagInfo.PixelFormat.Format8bppIndexed;
                                forcedPalette = new SortedDictionary<UInt32, byte>();

                                string[] paletteFilename = File.ReadAllLines(paletteIndicationPath);
                                string inDir = Path.GetDirectoryName(inBase);
                                inDir = inDir.Length == 0 ? "." : inDir;
                                string palettePath;
                                if (paletteFilename.Length == 0 || !File.Exists((palettePath = inDir + Path.DirectorySeparatorChar + Path.GetFileName(paletteFilename[0]))))
                                {
                                    throw new UnsupportedDataInput(String.Format("Specified colormap {0}:[{1}] does not exist or is not valid",
                                        paletteIndicationPath,
                                        paletteFilename[0]
                                    ));
                                }

                                using (Image paletteImage = Image.FromFile(palettePath))
                                {
                                    if (paletteImage.PixelFormat != PixelFormat.Format32bppArgb || !(paletteImage is Bitmap))
                                    {
                                        throw new UnsupportedDataInput(String.Format("Specified colormap {0}:[{1}] must be 32bpp ARGB",
                                            paletteIndicationPath,
                                            paletteFilename[0]
                                        ));
                                    }
                                    Bitmap asBmp = (Bitmap)paletteImage;
                                    byte[] bmpBytesRow = new byte[asBmp.Width * 4];
                                    BitmapData data = asBmp.LockBits(new Rectangle(0, 0, asBmp.Width, asBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                                    // Copy the bytes from a byte array into the image
                                    for (int r = 0; r < asBmp.Height; r++)
                                    {
                                        IntPtr rowaddr = new IntPtr((long)data.Scan0 + r * data.Stride);
                                        Marshal.Copy(rowaddr, bmpBytesRow, 0, asBmp.Width * 4);
                                        // Peek pixel values to create a colormap
                                        for (int columnIndex = 0; columnIndex < asBmp.Width * 4; columnIndex += 4)
                                        {
                                            UInt32 toPal = (UInt32)Color.FromArgb(
                                                bmpBytesRow[columnIndex + 3], // original A
                                                bmpBytesRow[columnIndex], // original B
                                                bmpBytesRow[columnIndex + 1], // original G
                                                bmpBytesRow[columnIndex + 2] // original R
                                            ).ToArgb();
                                            if (!forcedPalette.ContainsKey(toPal))
                                            {
                                                forcedPalette[toPal] = 1;
                                            }
                                        }
                                    }
                                    asBmp.UnlockBits(data);
                                }

                                if (forcedPalette.Count > 256)
                                {
                                    throw new UnsupportedDataInput(String.Format("Specified colormap:[{0}] unique color count exceeds 256: found {1} - apply color reduction processing first",
                                        paletteIndicationPath,
                                        forcedPalette.Count
                                    ));
                                }
                            }



                            using (MemoryStream ms = new MemoryStream())
                            {
                                Image imageRef = inImg;
                                string imageReplacementPath = Path.Combine(Path.GetDirectoryName(inBase), String.Format("edited_{0:D2}.png", chunkCounter));

                                try
                                {
                                    if (File.Exists(imageReplacementPath))
                                    {
                                        imageRef = Image.FromFile(imageReplacementPath, false);
                                        if (!(imageRef is Bitmap))
                                        {
                                            throw new UnsupportedDataInput(String.Format("Specified frame:[{0}] failed to load from file, not bitmap",
                                                imageReplacementPath
                                            ));
                                        }
                                        switch (imageRef.PixelFormat)
                                        {
                                            case System.Drawing.Imaging.PixelFormat.Format8bppIndexed: metapxFormat = (uint)ImagInfo.PixelFormat.Format8bppIndexed; break;
                                            case System.Drawing.Imaging.PixelFormat.Format32bppArgb: metapxFormat = (uint)ImagInfo.PixelFormat.Format32bppAbgr; break;
                                            default: throw new UnsupportedDataInput(String.Format("Specified frame:[{0}] failed to load from file, unsupported color format",
                                                imageReplacementPath
                                            ));
                                        }
                                        if (metawidth != imageRef.Width || metaheight != imageRef.Height)
                                        {
                                            throw new UnsupportedDataInput(String.Format("Input dimensions for page #{0}/{1} from File={2} do not match: expected {3}x{4}, found {5}x{6}", chunkCounter + 1, numPage, imageReplacementPath, metawidth, metaheight, imageRef.Width, imageRef.Height)); // zero-base page numbering
                                        }
                                    }
                                }
                                catch (System.OutOfMemoryException)
                                {
                                    throw new UnsupportedDataInput(String.Format("Specified frame:[{0}] failed to load from file",
                                        imageReplacementPath
                                    ));
                                }

                                WriteCelBitmap(imageRef, ImagInfo.PixelFormatFromValue(metapxFormat), ms, forcedPalette, String.Format("page #{0}/{1}", chunkCounter + 1, numPage));
                                byte[] palette_and_image = ms.ToArray();
                                writtenUncompressed = palette_and_image.Length;
#if test
                                using (FileStream fs = File.OpenWrite(@"E:\work\_TL_\akaiito_cel\cel_tiff2cel\debug_palimag.bin"))
                                {
                                    fs.Write(palette_and_image, 0, palette_and_image.Length);
                                }
#endif

                                using (MemoryStream compressedDataStream = new MemoryStream())
                                {
                                    ms.Seek(0, SeekOrigin.Begin);
                                    LZSS.Compress(ms, writtenUncompressed, compressedDataStream);
                                    compressedImage = compressedDataStream.ToArray();
                                    compressedDataStream.Seek(0, SeekOrigin.Begin);
                                    MemoryStream dms = new MemoryStream();
                                    //Hanasaku.LZSS.Decompress(compressedDataStream, compressedImage.Length, dms, writtenUncompressed);
                                    //using (FileStream fs = File.OpenWrite(@"E:\work\_TL_\akaiito_cel\cel_tiff2cel\debugdec.bin"))
                                    //{
                                        //byte[] outb = dms.ToArray();
                                        //fs.Write(outb, 0, outb.Length);
                                    //}
                                }
                            }
                        }
                        UInt32 compressedLZdatasize = cel2tiff.IMAG_LZ_HEADER_SIZE + (UInt32)compressedImage.Length;

                        UInt32 nextOffset = currentOffset + (UInt32)bindata.Length + compressedLZdatasize;
                        UInt32 origNextOffset = nextOffset;
                        nextOffset = CelChunk.GetAlignedChunkPosition(nextOffset);

                        // Replace into bindata chunk
                        lebc.CopyBytes(nextOffset, bindata, 4);
                        lebc.CopyBytes(metapxFormat, bindata, 0x10);
                        lebc.CopyBytes(compressedLZdatasize, bindata, (int)cel2tiff.IMAG_CHUNK_SIZE - 4);

                        writer.Write(bindata);  // header

                        // LZ block: header + bindata
                        writer.Write(new byte[] { 0x4C, 0x5A });  // LZ
                        bewriter.WriteUint((uint)writtenUncompressed);
                        bewriter.WriteUint((uint)compressedImage.Length);
                        writer.Write(compressedImage);

                        if (nextOffset > origNextOffset)
                        {
                            writer.Seek((int)(nextOffset - origNextOffset), SeekOrigin.Current);
                        }
                        currentOffset = nextOffset;
                    }
                    #endregion

                    #region ENDC
                    writer.WriteUint(cel2tiff.ENDC_CHUNK_CONST);
                    currentOffset += 8;
                    writer.WriteUint(currentOffset);
                    #endregion

                    writer.Seek(Marshal.SizeOf(typeof(CelHeader)) - 8, SeekOrigin.Begin);
                    writer.WriteUint(currentOffset - (uint)Marshal.SizeOf(typeof(CelHeader)));
                    
                }

            }
        }
    }
}
