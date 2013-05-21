//-----------------------------------------------------------------------
// <copyright file="TextRange.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-17</date>
// <summary>Represents a range of addresses where text can be safely inserted.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    
    /// <summary>
    /// Represents a range of addresses where text can be safely inserted.
    /// </summary>
    public class TextRange : IComparable
    {
        #region Variables
        private int start;
        private int end;
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the TextRange class.
        /// </summary>
        /// <param name="start">The starting address of the range (inclusive).</param>
        /// <param name="end">The ending address of the range (inclusive).</param>
        public TextRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
        
        #region Properties
        
        /// <summary>
        /// Gets or sets the starting address of the range.
        /// </summary>
        /// <value>The starting address of the range.</value>
        public int Start
        {
            get
            {
                return this.start;
            }
            
            set
            {
                this.start = value;
            }
        }
        
        /// <summary>
        /// Gets the ending address of the range.
        /// </summary>
        /// <value>The ending address of the range.</value>
        public int End
        {
            get
            {
                return this.end;
            }
        }
        
        /// <summary>
        /// Gets the number of bytes encompassed by the range.
        /// </summary>
        /// <value>The number of bytes in the range.</value>
        public int Length
        {
            get
            {
                return 1 + this.end - this.start;
            }
        }
        #endregion
        
        /// <summary>
        /// Determines whether the ranges have the same length.
        /// </summary>
        /// <param name="range1">The left-hand operand.</param>
        /// <param name="range2">The right-hand operand.</param>
        /// <returns>True if the ranges have the same length, false otherwise.</returns>
        public static bool operator ==(TextRange range1, TextRange range2)
        {
            if ((object)range1 == null)
            {
                return (object)range2 == null;
            }
            
            return range1.Equals(range2);
        }
        
        /// <summary>
        /// Determines whether the ranges have different lengths.
        /// </summary>
        /// <param name="range1">The left-hand operand.</param>
        /// <param name="range2">The right-hand operand.</param>
        /// <returns>True if the ranges have different lengths, false otherwise.</returns>
        public static bool operator !=(TextRange range1, TextRange range2)
        {
            return !(range1 == range2);
        }
        
        /// <summary>
        /// Determines whether the left-hand operand is longer than the right-hand operand.
        /// </summary>
        /// <param name="range1">The left-hand operand.</param>
        /// <param name="range2">The right-hand operand.</param>
        /// <returns>True if the left-hand operand is longer than the right-hand operand, false otherwise.</returns>
        public static bool operator <(TextRange range1, TextRange range2)
        {
            if (range1 == null)
            {
                return true;
            }
            
            return range1.CompareTo(range2) < 0;
        }
        
        /// <summary>
        /// Determines whether the left-hand operand is shorter than the right-hand operand.
        /// </summary>
        /// <param name="range1">The left-hand operand.</param>
        /// <param name="range2">The right-hand operand.</param>
        /// <returns>True if the left-hand operand is shorter than the right-hand operand, false otherwise.</returns>
        public static bool operator >(TextRange range1, TextRange range2)
        {
            if (range1 == null)
            {
                return false;
            }
            
            return range1.CompareTo(range2) > 0;
        }
        
        /// <summary>
        /// Compares the lengths of the two ranges.
        /// </summary>
        /// <param name="obj">The range to compare to this.</param>
        /// <returns>Positive if this range is shorter than that one, negative if it is longer, or zero if they have equal length.</returns>
        public int CompareTo(object obj)
        {
            TextRange that = (TextRange)obj;
            int thisLength = this.end - this.start;
            int thatLength = that.end - that.start;
            return thatLength.CompareTo(thisLength);
        }
        
        /// <summary>
        /// Determines whether the ranges have the same length.
        /// </summary>
        /// <param name="obj">The range to compare to this.</param>
        /// <returns>True if the ranges have the same length, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is TextRange))
            {
                return false;
            }
            
            return this.CompareTo(obj) == 0;
        }
        
        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return this.Length;
        }
    }
}
