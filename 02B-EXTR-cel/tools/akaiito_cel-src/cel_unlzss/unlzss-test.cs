using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Reflection;
using System.Runtime.CompilerServices;

using Hanasaku;

[assembly: AssemblyTitle("unlzss-test.cs")]
[assembly: AssemblyDescription("Uncompresses LZSS data for Aoi Shiro.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Nekozakura")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("2011-*")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("1.0.0.0")]

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]




public class Entrypoint
{
    public static void Main(String[] args)
    {
        if ( args.Length == 0 )
            return;
        String filein = args[0];

        using( FileStream fs = File.OpenRead(filein) )
        {
            if (fs.Length > int.MaxValue)
                throw new Exception("Files larger than 2GB cannot be decompressed by this program.");
            using( BinaryReader br = new BinaryReader(fs) )
            {
                using (MemoryStream memsw = new MemoryStream())
                {
                    int width = 0x35;
                    int height = 0x38;

#if TEMP
                    Hanasaku.LZSS.Decompress(fs, fs.Length, memsw, 0x3EE, 0x4B404);
#else
                    Hanasaku.LZSS.Decompress(fs, fs.Length, memsw, 0x3EE, 0x404 + width * height);
#endif
                    using (FileStream fsw = new FileStream(filein + ".tga", FileMode.Create))
                    {
                        fsw.Write(new byte[0x12]{
                            0x00,	//idLength           // Length of optional identification sequence.
                            0x01,   //paletteType        // Is a palette present? (1=yes)
                            0x01,   //imageType          // Image data type (0=none, 1=indexed, 2=rgb, 3=grey, +8=rle packed).
                            0x00, 0x00, //firstPaletteEntry  // First palette index, if present.
                            0x00, 0x01, //numPaletteEntries  // Number of palette entries, if present.
                            0x20,   //paletteBits        // Number of bits per palette entry.
                            0x00, 0x00, //x                  // Horiz. pixel coord. of lower left of image.
                            0x00, 0x00, //y                  // Vert. pixel coord. of lower left of image.
#if TEMP
                        0x80, 0x02,     //width              // Image width in pixels.
                        0xE0, 0x01,     //height             // Image height in pixels.
#else
                        0x35, 0x00,     //width              // Image width in pixels.
                        0x38, 0x00,     //height             // Image height in pixels.
#endif
                            0x08,   //depth              // Image color depth (bits per pixel).
                            0x27	//descriptor         // Image attribute flags.
                        }, 0, 0x12);

                        byte[] memBuffer = memsw.ToArray();
                        string outfile = String.Format("out_{0:X}.bin", 0x404 + width * height);
                        using (Stream fw = File.OpenWrite(outfile))
                        {
                            fw.Write(memBuffer, 0, memBuffer.Length);
                        }
                        byte[] palette = new byte[0x400];
                        for (int index = 0; index < 0x400; index+=4)
                        {
                            palette[index    ] = memBuffer[index + 6];
                            palette[index + 1] = memBuffer[index + 5];
                            palette[index + 2] = memBuffer[index + 4];
                            palette[index + 3] = memBuffer[index + 7];
                        }
                        fsw.Write(palette, 0, 0x400);
#if TEMP
                        fsw.Write(memBuffer, 0x404, 0x0004B404 - 0x404);
#else
                        fsw.Write(memBuffer, 0x404, width * height);
#endif
                    }
                }
            }
        }
    }
}

