//-----------------------------------------------------------------------
// <copyright file="Arm9File.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-06</date>
// <summary>An executable binary file with embedded strings.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using DarthNemesis;
    
    /// <summary>
    /// An executable binary file with embedded strings.
    /// </summary>
    public class Arm9File : CachedTextFile
    {
        #region Variables
        private const string TextDirectoryPrefix = @"text\";
        private const string PointerDirectoryPrefix = @"pointers\";
        private const string GameFileExtension = ".bin";
        private const string TextFileExtension = ".sjs";
        private const string PointerFileExtension = ".ptr";
        private const int Arm9Offset = 0x02000000;
        private string[] strings;
        private int[] pointers;
        private KeyValuePair<int, int>[] validTextRanges;
        private bool isCompressed;
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the Arm9File class.
        /// </summary>
        /// <param name="game">Common properties and methods for the entire game.</param>
        /// <param name="isCompressed">A value indicating whether the arm9 file was originally compressed.</param>
        public Arm9File(IGame game, bool isCompressed) : base("arm9.bin", game)
        {
            this.isCompressed = isCompressed;
        }
        
        /// <summary>
        /// Initializes a new instance of the Arm9File class.
        /// </summary>
        /// <param name="game">Common properties and methods for the entire game.</param>
        public Arm9File(IGame game) : base("arm9.bin", game)
        {
        }
        
        #region Properties
        
        /// <summary>
        /// Gets the name of the text file relative to the directory where the game was extracated.
        /// </summary>
        /// <value>The name of the text file.</value>
        protected override string TextFileName
        {
            get
            {
                return TextDirectoryPrefix + Path.ChangeExtension(this.FileName, TextFileExtension);
            }
        }
        
        private string GameDirectoryPrefix
        {
            get
            {
                return this.isCompressed ? @"cache\" : @"NDS_UNPACK\";
            }
        }
        
        private string GameFileName
        {
            get
            {
                return this.GameDirectoryPrefix + Path.ChangeExtension(this.FileName, GameFileExtension);
            }
        }
        
        private string PointerFileName
        {
            get
            {
                return PointerDirectoryPrefix + Path.ChangeExtension(this.FileName, PointerFileExtension);
            }
        }
        #endregion
        
        /// <summary>
        /// Extracts the text strings from the game file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully loaded.</returns>
        protected override bool LoadText()
        {
            byte[] fileData = StreamHelper.ReadFile(this.GameFileName);
            
            FileStream pointerFile = new FileStream(this.PointerFileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(pointerFile);
            
            int numPointers = reader.ReadInt32();
            this.pointers = new int[numPointers];
            for (int i = 0; i < this.pointers.Length; i++)
            {
                this.pointers[i] = reader.ReadInt32();
            }
            
            int numRanges = reader.ReadInt32();
            
            this.validTextRanges = new KeyValuePair<int, int>[numRanges];
            for (int i = 0; i < this.validTextRanges.Length; i++)
            {
                this.validTextRanges[i] = new KeyValuePair<int, int>(reader.ReadInt32(), reader.ReadInt32());
            }
            
            this.strings = new string[this.pointers.Length];
            for (int i = 0; i < this.pointers.Length; i++)
            {
                int textOffset = BitConverter.ToInt32(fileData, this.pointers[i]) - Arm9Offset;
                this.strings[i] = this.Game.GetText(fileData, textOffset);
            }
            
            reader.Close();
            return true;
        }
        
        /// <summary>
        /// Inserts the text strings back into the game file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully saved.</returns>
        protected override bool SaveText()
        {
            if (!this.IsModified)
            {
                return false;
            }
            
            byte[] fileData = StreamHelper.ReadFile(this.GameFileName);
            
            Dictionary<string, int> dupes = new Dictionary<string, int>();
            
            KeyValuePair<int, int>[] textRanges = new KeyValuePair<int, int>[this.validTextRanges.Length];
            for (int i = 0; i < textRanges.Length; i++)
            {
                textRanges[i] = this.validTextRanges[i];
            }
            
            byte[] pointerBytes;
            for (int i = 0; i < this.pointers.Length; i++)
            {
                if (dupes.ContainsKey(this.strings[i]))
                {
                    pointerBytes = BitConverter.GetBytes(dupes[this.strings[i]]);
                }
                else
                {
                    byte[] encodedString = this.Game.GetBytes(this.strings[i]);
                    int textLength = encodedString.Length + 1;
                    while (textLength % 4 != 0)
                    {
                        textLength++;
                    }
                    
                    int textOffset = -1;
                    for (int k = 0; k < textRanges.Length; k++)
                    {
                        int rangeLength = textRanges[k].Value - textRanges[k].Key + 1;
                        if (textLength <= rangeLength)
                        {
                            textOffset = textRanges[k].Key;
                            textRanges[k] = new KeyValuePair<int, int>(textOffset + textLength, textRanges[k].Value);
                            break;
                        }
                    }
                    
                    if (textOffset == -1)
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, "Ran out of room for translated text! (line {0}/{1})", i + 1, this.pointers.Length);
                        throw new FormatException(message);
                    }
                    
                    Array.Copy(encodedString, 0, fileData, textOffset, encodedString.Length);
                    for (int pad = encodedString.Length; pad < textLength; pad++)
                    {
                        fileData[textOffset + pad] = 0x00;
                    }
                    
                    pointerBytes = BitConverter.GetBytes(textOffset + Arm9Offset);
                    dupes.Add(this.strings[i], textOffset + Arm9Offset);
                }
                
                Array.Copy(pointerBytes, 0, fileData, this.pointers[i], Constants.PointerLength);
            }
            
            StreamHelper.WriteFile(this.GameFileName, fileData);
            return true;
        }
        
        /// <summary>
        /// Writes the text strings into an easily editable text file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully exported.</returns>
        protected override bool ExportText()
        {
            StreamHelper.WriteLinesToFile(this.TextFileName, this.strings, this.Game.Encoding);
            return true;
        }
        
        /// <summary>
        /// Reads in the replacement text strings from the text file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully imported.</returns>
        protected override bool ImportText()
        {            
            StreamHelper.ReadLinesFromFile(this.TextFileName, this.strings, this.Game.Encoding);
            return true;
        }
    }
}
