//-----------------------------------------------------------------------
// <copyright file="ScriptPointer.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-17</date>
// <summary>Holds a line of text and its associated pointer.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    
    /// <summary>
    /// Holds a line of text and its associated pointer.
    /// </summary>
    public class ScriptPointer : IComparable
    {
        #region Variables
        private readonly int pointer;
        private readonly string text;
        private readonly byte[] encodedText;
        private readonly int length;
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the ScriptPointer class.
        /// </summary>
        /// <param name="game">Common properties and methods for the entire game.</param>
        /// <param name="pointer">The address of the pointer to the location of the text in the file.</param>
        /// <param name="text">A line of text.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "pointer", Justification = "Variable represents a pointer to an address.")]
        public ScriptPointer(IGame game, int pointer, string text)
        {
            this.pointer = pointer;
            this.text = text;
            
            this.encodedText = game.GetBytes(text);
            int textLength = this.encodedText.Length + 1;
            while (textLength % 4 != 0)
            {
                textLength++;
            }
            
            this.length = textLength;
        }
        
        #region Properties
        
        /// <summary>
        /// Gets the address of the pointer to the location of the text in the file.
        /// </summary>
        /// <value>The address of the pointer to the location of the text in the file.</value>
        public int Pointer
        {
            get
            {
                return this.pointer;
            }
        }
        
        /// <summary>
        /// Gets the line of text.
        /// </summary>
        /// <value>A line of text.</value>
        public string Text
        {
            get
            {
                return this.text;
            }
        }
        
        /// <summary>
        /// Gets the number of bytes required for the text once it has been encoded and padded to a 4-byte boundary.
        /// </summary>
        /// <value>The number of bytes requried for the encoded text.</value>
        public int Length
        {
            get
            {
                return this.length;
            }
        }
        #endregion
        
        /// <summary>
        /// Determines whether the lines have the same encoded length.
        /// </summary>
        /// <param name="sp1">The left-hand operand.</param>
        /// <param name="sp2">The right-hand operand.</param>
        /// <returns>True if the lines have the same encoded length, false otherwise.</returns>
        public static bool operator ==(ScriptPointer sp1, ScriptPointer sp2)
        {
            if ((object)sp1 == null)
            {
                return (object)sp2 == null;
            }
            
            return sp1.Equals(sp2);
        }
        
        /// <summary>
        /// Determines whether the lines have different encoded lengths.
        /// </summary>
        /// <param name="sp1">The left-hand operand.</param>
        /// <param name="sp2">The right-hand operand.</param>
        /// <returns>True if the lines have different encoded lengths, false otherwise.</returns>
        public static bool operator !=(ScriptPointer sp1, ScriptPointer sp2)
        {
            return !(sp1 == sp2);
        }
        
        /// <summary>
        /// Determines whether the left-hand operand is longer than the right-hand operand.
        /// </summary>
        /// <param name="sp1">The left-hand operand.</param>
        /// <param name="sp2">The right-hand operand.</param>
        /// <returns>True if the left-hand operand is longer than the right-hand operand, false otherwise.</returns>
        public static bool operator <(ScriptPointer sp1, ScriptPointer sp2)
        {
            if (sp1 == null)
            {
                return true;
            }
            
            return sp1.CompareTo(sp2) < 0;
        }
        
        /// <summary>
        /// Determines whether the left-hand operand is shorter than the right-hand operand.
        /// </summary>
        /// <param name="sp1">The left-hand operand.</param>
        /// <param name="sp2">The right-hand operand.</param>
        /// <returns>True if the left-hand operand is shorter than the right-hand operand, false otherwise.</returns>
        public static bool operator >(ScriptPointer sp1, ScriptPointer sp2)
        {
            if (sp1 == null)
            {
                return false;
            }
            
            return sp1.CompareTo(sp2) > 0;
        }
        
        /// <summary>
        /// Gets the encoded byte representation of the line.
        /// </summary>
        /// <returns>The encoded text.</returns>
        public byte[] GetEncodedText()
        {
            return this.encodedText;
        }
        
        /// <summary>
        /// Compares the lengths of the two lines once they have been encoded.
        /// </summary>
        /// <param name="obj">The line to compare to this.</param>
        /// <returns>Positive if this line is shorter than that one, negative if it is longer, or zero if they have equal length.</returns>
        public int CompareTo(object obj)
        {
            ScriptPointer that = (ScriptPointer)obj;
            return that.length.CompareTo(this.length);
        }
        
        /// <summary>
        /// Determines whether the lines have the same encoded length.
        /// </summary>
        /// <param name="obj">The line to compare to this.</param>
        /// <returns>True if the lines have the same encoded length, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ScriptPointer))
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
