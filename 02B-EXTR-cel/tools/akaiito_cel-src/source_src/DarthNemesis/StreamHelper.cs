//-----------------------------------------------------------------------
// <copyright file="StreamHelper.cs" company="DarthNemesis">
// Copyright (c) 2009 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2009-12-18</date>
// <summary>Helper methods for stream I/O.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    
    /// <summary>
    /// Helper methods for stream I/O.
    /// </summary>
    public sealed class StreamHelper
    {
        private StreamHelper()
        {
        }
        
        /// <summary>
        /// A helper method for reading lines from a stream. Ignores comment lines and pads with blank lines if the
        /// application tries to read past the end of the stream.
        /// </summary>
        /// <param name="reader">The stream to be read.</param>
        /// <returns>The next non-comment line in the stream.</returns>
        public static string ReadNextLine(TextReader reader)
        {
            string line;
            do
            {
                line = reader.ReadLine();
            }
            while (line != null && line.StartsWith("//", StringComparison.Ordinal));
            
            return line == null ? string.Empty : line;
        }
        
        /// <summary>
        /// Reads as much of the stream as will fit into the provided buffer.
        /// </summary>
        /// <param name="stream">The stream to be read.</param>
        /// <param name="data">The buffer into which the stream is read.</param>
        public static void ReadStream(Stream stream, byte[] data)
        {
            int offset = 0;
            int remaining = data.Length;
            while (remaining > 0)
            {
                int read = stream.Read(data, offset, remaining);
                if (read <= 0)
                {
                    throw new EndOfStreamException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            "End of stream reached with {0} bytes left to read",
                            remaining));
                }
                
                remaining -= read;
                offset += read;
            }
        }
        
        /// <summary>
        /// Reads the specified number of bytes from the stream into memory.
        /// </summary>
        /// <param name="stream">The stream to be read.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>A buffer containing the contents read from the stream.</returns>
        public static byte[] ReadStream(Stream stream, int length)
        {
            byte[] streamData = new byte[length];
            ReadStream(stream, streamData);
            return streamData;
        }
        
        /// <summary>
        /// Reads the entire stream into memory.
        /// </summary>
        /// <param name="stream">The stream to be read.</param>
        /// <returns>A buffer containing the contents of the entire stream.</returns>
        public static byte[] ReadStream(Stream stream)
        {
            return ReadStream(stream, (int)stream.Length);
        }
        
        /// <summary>
        /// Returns the entire contents of the given file.
        /// </summary>
        /// <param name="fileName">The path of the file to be read from.</param>
        /// <returns>A byte array containing the file contents.</returns>
        public static byte[] ReadFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] fileData = ReadStream(file);
            file.Close();
            return fileData;
        }
        
        /// <summary>
        /// Returns the entire contents of the given file.
        /// </summary>
        /// <param name="fileName">The path of the file to be read from.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>A byte array containing the file contents.</returns>
        public static byte[] ReadFile(string fileName, int length)
        {
            FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] fileData = ReadStream(file, length);
            file.Close();
            return fileData;
        }
        
        /// <summary>
        /// Writes the data to the given file, creating or overwriting it.
        /// </summary>
        /// <param name="fileName">The path of the file to be written to.</param>
        /// <param name="data">A byte array containing the new contents of the file.</param>
        /// <param name="offset">The starting offset in the array of the data to write.</param>
        /// <param name="length">The number of bytes to write.</param>
        public static void WriteFile(string fileName, byte[] data, int offset, int length)
        {
            string directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            FileStream file = null;
            try
            {
                file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                file.Write(data, offset, length);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
        }
        
        /// <summary>
        /// Writes the data to the given file, creating or overwriting it.
        /// </summary>
        /// <param name="fileName">The path of the file to be written to.</param>
        /// <param name="data">A byte array containing the new contents of the file.</param>
        public static void WriteFile(string fileName, byte[] data)
        {
            WriteFile(fileName, data, 0, data.Length);
        }
        
        /// <summary>
        /// Reads the lines sequentially from the given file.
        /// </summary>
        /// <param name="fileName">The path of the file to be read from.</param>
        /// <param name="lines">The 2-dimensional string array to be populated from the file.</param>
        /// <param name="encoding">The encoding to use when reading from the file.</param>
        public static void ReadLinesFromFile(string fileName, string[] lines, Encoding encoding)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                StreamReader reader = new StreamReader(file, encoding);
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = ReadNextLine(reader);
                }
            }
        }
        
        /// <summary>
        /// Reads the lines sequentially from the given file.
        /// </summary>
        /// <param name="fileName">The path of the file to be read from.</param>
        /// <param name="lines">The 2-dimensional string array to be populated from the file.</param>
        /// <param name="encoding">The encoding to use when reading from the file.</param>
        public static void ReadLinesFromFile2D(string fileName, string[][] lines, Encoding encoding)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                StreamReader reader = new StreamReader(file, encoding);
                for (int i = 0; i < lines.Length; i++)
                {
                    for (int j = 0; j < lines[i].Length; j++)
                    {
                        lines[i][j] = ReadNextLine(reader);
                    }
                }
            }
        }
        
        /// <summary>
        /// Reads the lines sequentially from the given file.
        /// </summary>
        /// <param name="fileName">The path of the file to be read from.</param>
        /// <param name="lines">The 3-dimensional string array to be populated from the file.</param>
        /// <param name="encoding">The encoding to use when reading from the file.</param>
        public static void ReadLinesFromFile3D(string fileName, string[][][] lines, Encoding encoding)
        {
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                StreamReader reader = new StreamReader(file, encoding);
                for (int i = 0; i < lines.Length; i++)
                {
                    for (int j = 0; j < lines[i].Length; j++)
                    {
                        for (int k = 0; k < lines[j].Length; k++)
                        {
                            lines[i][j][k] = ReadNextLine(reader);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Writes the lines sequentially to the given file, creating or overwriting it.
        /// </summary>
        /// <param name="fileName">The path of the file to be written to.</param>
        /// <param name="lines">A string array containing the new contents of the file.</param>
        /// <param name="encoding">The encoding to use when writing to the file.</param>
        public static void WriteLinesToFile(string fileName, string[] lines, Encoding encoding)
        {
            string directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            MemoryStream contents = new MemoryStream();
            StreamWriter writer = new StreamWriter(contents, encoding);
            
            for (int i = 0; i < lines.Length; i++)
            {
                writer.WriteLine(lines[i]);
            }
            
            writer.Flush();
            WriteFile(fileName, contents.ToArray());
        }
        
        /// <summary>
        /// Writes the lines sequentially to the given file, creating or overwriting it.
        /// </summary>
        /// <param name="fileName">The path of the file to be written to.</param>
        /// <param name="lines">A 2-dimensional string array containing the new contents of the file.</param>
        /// <param name="encoding">The encoding to use when writing to the file.</param>
        public static void WriteLinesToFile2D(string fileName, string[][] lines, Encoding encoding)
        {
            string directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            MemoryStream contents = new MemoryStream();
            StreamWriter writer = new StreamWriter(contents, encoding);
            
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    writer.WriteLine(lines[i][j]);
                }
            }
            
            writer.Flush();
            WriteFile(fileName, contents.ToArray());
        }
        
        /// <summary>
        /// Writes the lines sequentially to the given file, creating or overwriting it.
        /// </summary>
        /// <param name="fileName">The path of the file to be written to.</param>
        /// <param name="lines">A 3-dimensional string array containing the new contents of the file.</param>
        /// <param name="encoding">The encoding to use when writing to the file.</param>
        public static void WriteLinesToFile3D(string fileName, string[][][] lines, Encoding encoding)
        {
            string directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            
            MemoryStream contents = new MemoryStream();
            StreamWriter writer = new StreamWriter(contents, encoding);
            
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    for (int k = 0; k < lines[j].Length; k++)
                    {
                        writer.WriteLine(lines[i][j][k]);
                    }
                }
            }
            
            writer.Flush();
            WriteFile(fileName, contents.ToArray());
        }
        
        /// <summary>
        /// Writes a 4-byte integer into a byte array at the specified offset.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="array">The array to be written to.</param>
        /// <param name="offset">The offset at which the value will be written.</param>
        /// <returns>True if the value was written, false otherwise.</returns>
        public static bool WriteBytes(int value, byte[] array, int offset)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (offset < 0 || offset + bytes.Length > array.Length)
            {
                return false;
            }
            
            Array.Copy(bytes, 0, array, offset, bytes.Length);
            return true;
        }
        
        /// <summary>
        /// Retrieves the last modification time for the file with the given path.
        /// </summary>
        /// <param name="fileName">The path of the file to check.</param>
        /// <returns>The file's last modification time.</returns>
        public static long GetTimestamp(string fileName)
        {
            DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(fileName);
            return lastWriteTimeUtc.Ticks;
        }
    }
}
