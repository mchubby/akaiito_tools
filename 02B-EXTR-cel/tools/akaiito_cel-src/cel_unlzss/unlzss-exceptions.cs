using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hanasaku
{
    public class InputTooLargeException : Exception
    {
        public InputTooLargeException()
            : base("The compression ratio is not high enough to fit the input "
            + "in a single compressed file.") { }
    }

    /// <summary>
    /// An exception that is thrown by the decompression functions when there
    /// is not enough data available in order to properly decompress the input.
    /// </summary>
    public class NotEnoughDataException : IOException
    {
        private long currentOutSize;
        private long totalOutSize;
        /// <summary>
        /// Gets the actual number of written bytes.
        /// </summary>
        public long WrittenLength { get { return this.currentOutSize; } }
        /// <summary>
        /// Gets the number of bytes that was supposed to be written.
        /// </summary>
        public long DesiredLength { get { return this.totalOutSize; } }


        /// <summary>
        /// Creates a new NotEnoughDataException.
        /// </summary>
        /// <param name="currentOutSize">The actual number of written bytes.</param>
        /// <param name="totalOutSize">The desired number of written bytes.</param>
        public NotEnoughDataException(long currentOutSize, long totalOutSize)
            : base("Not enough data available; 0x" + currentOutSize.ToString("X")
                + " of " + (totalOutSize < 0 ? "???" : ("0x" + totalOutSize.ToString("X")))
                + " bytes written.")
        {
            this.currentOutSize = currentOutSize;
            this.totalOutSize = totalOutSize;
        }
    }
    
    /// <summary>
    /// An exception thrown by the compression or decompression function, indicating that the
    /// given input length was too large for the given input stream.
    /// </summary>
    public class StreamTooShortException : EndOfStreamException
    {
        public StreamTooShortException()
            : base("The end of the stream was reached "
                 + "before the given amount of data was read.")
        { }
    }
    
    public class UnsupportedDataInput : ApplicationException
    {
        public UnsupportedDataInput()
            : base("Input contained invalid or unsupported data.")
        { }
        public UnsupportedDataInput(string details)
            : base("Input contained invalid or unsupported data: "
                 + details)
        { }
    }
    
    public class TooMuchInputException : Exception
    {
        /// <summary>
        /// Gets the number of bytes read by the decompressed to decompress the stream.
        /// </summary>
        public long ReadBytes { get; private set; }


        /// <summary>
        /// Creates a new exception indicating that the input has more data than necessary for
        /// decompressing th stream. It may indicate that other data is present after the compressed
        /// stream.
        /// </summary>
        /// <param name="readBytes">The number of bytes read by the decompressor.</param>
        /// <param name="totLength">The indicated length of the input stream.</param>
        public TooMuchInputException(long readBytes, long totLength)
            : base("The input contains more data than necessary. Only used 0x" 
            + readBytes.ToString("X") + " of 0x" + totLength.ToString("X") + " bytes")
        {
            this.ReadBytes = readBytes;
        }
        public TooMuchInputException(long readBytes, long totLength, string details)
            : base("The input contains more data than necessary. Only used 0x"
                 + readBytes.ToString("X") + " of 0x" + totLength.ToString("X") + " bytes. Details: "
                 + details)
        {
            this.ReadBytes = readBytes;
        }
    }    
    
}
