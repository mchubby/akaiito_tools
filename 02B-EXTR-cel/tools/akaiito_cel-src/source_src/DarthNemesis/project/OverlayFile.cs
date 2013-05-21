//-----------------------------------------------------------------------
// <copyright file="OverlayFile.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-09</date>
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
    public class OverlayFile : CachedTextFile
    {
        #region Variables
        private const string OverlayTableFileName = @"NDS_UNPACK\y9.bin";
        private const string TextDirectoryPrefix = @"text\";
        private const string PointerDirectoryPrefix = @"pointers\";
        private const string GameFileExtension = ".bin";
        private const string TextFileExtension = ".sjs";
        private const string PointerFileExtension = ".ptr";
        private const int OverlayTableEntryLength = 0x20;
        private int overlayNumber;
        private int overlayOffset;
        private int[] overlayOffsetPointers;
        private string[] strings;
        private int[] pointers;
        private List<TextRange> validTextRanges;
        private bool isCompressed;
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the OverlayFile class.
        /// </summary>
        /// <param name="overlayNumber">The index of the overlay to load.</param>
        /// <param name="game">Common properties and methods for the entire game.</param>
        /// <param name="isCompressed">A value indicating whether the overlay file was originally compressed.</param>
        public OverlayFile(int overlayNumber, IGame game, bool isCompressed)
            : base(string.Format(CultureInfo.InvariantCulture, @"overlay\overlay_{0:0000}.bin", overlayNumber), game)
        {
            this.overlayNumber = overlayNumber;
            this.isCompressed = isCompressed;
            this.GenerateOffsetPointers();
        }
        
        /// <summary>
        /// Initializes a new instance of the OverlayFile class.
        /// </summary>
        /// <param name="overlayNumber">The index of the overlay to load.</param>
        /// <param name="game">Common properties and methods for the entire game.</param>
        public OverlayFile(int overlayNumber, IGame game) : this(overlayNumber, game, false)
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
            byte[] overlayTableData = StreamHelper.ReadFile(OverlayTableFileName);
            this.overlayOffset = BitConverter.ToInt32(overlayTableData, this.overlayOffsetPointers[this.overlayNumber]);
            
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
            
            this.validTextRanges = new List<TextRange>(numRanges);
            for (int i = 0; i < numRanges; i++)
            {
                TextRange textRange = new TextRange(reader.ReadInt32(), reader.ReadInt32());
                this.validTextRanges.Add(textRange);
            }
            
            this.strings = new string[this.pointers.Length];
            for (int i = 0; i < this.pointers.Length; i++)
            {
                int textOffset = BitConverter.ToInt32(fileData, this.pointers[i]) - this.overlayOffset;
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
            byte[] fileData = StreamHelper.ReadFile(this.GameFileName);
            int linesAdded = 0;
            
            Dictionary<string, int> dupes = new Dictionary<string, int>();
            
            List<ScriptPointer> scriptPointers = new List<ScriptPointer>(this.strings.Length);
            int i;
            for (i = 0; i < this.strings.Length; i++)
            {
                ScriptPointer scriptPointer = new ScriptPointer(this.Game, this.pointers[i], this.strings[i]);
                scriptPointers.Add(scriptPointer);
            }
            
            List<TextRange> textRanges = new List<TextRange>(this.validTextRanges);
            
            scriptPointers.Sort();
            textRanges.Sort();
            
            /*
            System.Diagnostics.Debug.WriteLine("Phase 1");
            for (int a = 0; a < scriptPointers.Count; a++)
            {
                if (scriptPointers[a] == null) continue;
                System.Diagnostics.Debug.WriteLine(a + ". " + scriptPointers[a].Length + " - " + scriptPointers[a].Text);
            }
            for (int b = 0; b < textRanges.Count; b++)
            {
                if (textRanges[b] == null) continue;
                System.Diagnostics.Debug.WriteLine(b + ". " + textRanges[b].Length);
            }
             */
            
            i = 0;
            int k = 0;
            byte[] pointerBytes;
            while (i < scriptPointers.Count)
            {
                if (dupes.ContainsKey(scriptPointers[i].Text))
                {
                    ////System.Diagnostics.Debug.WriteLine("Skipping duplicate line " + i + ".");
                    pointerBytes = BitConverter.GetBytes(dupes[scriptPointers[i].Text]);
                    Array.Copy(pointerBytes, 0, fileData, scriptPointers[i].Pointer, Constants.PointerLength);
                    scriptPointers[i] = null;
                    linesAdded++;
                    i++;
                }
                else if (k < textRanges.Count)
                {
                    if (scriptPointers[i].Length > textRanges[k].Length)
                    {
                        ////System.Diagnostics.Debug.WriteLine("Line " + i + " is larger than current range, skipping.");
                        i++;
                    }
                    else if (scriptPointers[i].Length < textRanges[k].Length)
                    {
                        ////System.Diagnostics.Debug.WriteLine("Range " + k + " is larger than current line, skipping.");
                        k++;
                    }
                    else
                    {
                        ////System.Diagnostics.Debug.WriteLine("Writing line " + i + " to range " + k + ". New length = 0");
                        int textOffset = textRanges[k].Start;
                        textRanges[k] = null;
                        k++;
                        byte[] encodedText = scriptPointers[i].GetEncodedText();
                        Array.Copy(encodedText, 0, fileData, textOffset, encodedText.Length);
                        for (int pad = encodedText.Length; pad < scriptPointers[i].Length; pad++)
                        {
                            fileData[textOffset + pad] = 0x00;
                        }
                        
                        pointerBytes = BitConverter.GetBytes(textOffset + this.overlayOffset);
                        Array.Copy(pointerBytes, 0, fileData, scriptPointers[i].Pointer, Constants.PointerLength);
                        dupes.Add(scriptPointers[i].Text, textOffset + this.overlayOffset);
                        scriptPointers[i] = null;
                        linesAdded++;
                        i++;
                    }
                }
                else
                {
                    ////System.Diagnostics.Debug.WriteLine("Line " + i + " is larger than current range, skipping.");
                    i++;
                }
            }
            
            /*
            System.Diagnostics.Debug.WriteLine("Phase 2");
            for (int a = 0; a < scriptPointers.Count; a++)
            {
                if (scriptPointers[a] == null) continue;
                System.Diagnostics.Debug.WriteLine(a + ". " + scriptPointers[a].Length + " - " + scriptPointers[a].Text);
            }
            for (int b = 0; b < textRanges.Count; b++)
            {
                if (textRanges[b] == null) continue;
                System.Diagnostics.Debug.WriteLine(b + ". " + textRanges[b].Length);
            }
             */
            
            for (i = 0; i < scriptPointers.Count; i++)
            {
                if (scriptPointers[i] == null)
                {
                    continue;
                }
                
                if (dupes.ContainsKey(scriptPointers[i].Text))
                {
                    ////System.Diagnostics.Debug.WriteLine("Skipping duplicate line " + i + ".");
                    pointerBytes = BitConverter.GetBytes(dupes[scriptPointers[i].Text]);
                }
                else
                {
                    int textOffset = -1;
                    for (k = 0; k < textRanges.Count; k++)
                    {
                        if (textRanges[k] == null)
                        {
                            ////System.Diagnostics.Debug.WriteLine("No room for line " + i + ".");
                            continue;
                        }
                        
                        if (scriptPointers[i].Length <= textRanges[k].Length)
                        {
                            textOffset = textRanges[k].Start;
                            textRanges[k].Start = textOffset + scriptPointers[i].Length;
                            ////System.Diagnostics.Debug.WriteLine("Writing line " + i + " to range " + k + ". New start = " + textRanges[k].Start + ", new length = " + textRanges[k].Length);
                            if (textRanges[k].Length == 0)
                            {
                                textRanges[k] = null;
                            }
                            
                            break;
                        }
                    }
                    
                    if (textOffset == -1)
                    {
                        continue;
                    }
                    
                    byte[] encodedText = scriptPointers[i].GetEncodedText();
                    Array.Copy(encodedText, 0, fileData, textOffset, encodedText.Length);
                    for (int pad = encodedText.Length; pad < scriptPointers[i].Length; pad++)
                    {
                        fileData[textOffset + pad] = 0x00;
                    }
                    
                    pointerBytes = BitConverter.GetBytes(textOffset + this.overlayOffset);
                    dupes.Add(scriptPointers[i].Text, textOffset + this.overlayOffset);
                }
                
                Array.Copy(pointerBytes, 0, fileData, scriptPointers[i].Pointer, Constants.PointerLength);
                linesAdded++;
            }
            
            if (linesAdded < scriptPointers.Count)
            {
                string message = string.Format(CultureInfo.CurrentCulture, "Ran out of room for translated text! (lines fit {0}/{1})", linesAdded, scriptPointers.Count);
                throw new FormatException(message);
            }
            
            StreamHelper.WriteFile(this.GameFileName, fileData);
            return true;
        }
        
        /*
        /// <summary>
        /// Inserts the text strings back into the game file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully saved.</returns>
        protected override bool SaveText()
        {
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
                    
                    pointerBytes = BitConverter.GetBytes(textOffset + this.overlayOffset);
                    dupes.Add(this.strings[i], textOffset + this.overlayOffset);
                }
                
                Array.Copy(pointerBytes, 0, fileData, this.pointers[i], Constants.PointerLength);
            }
            
            StreamHelper.WriteFile(this.GameFileName, fileData);
            return true;
        }
         */
        
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
        
        private void GenerateOffsetPointers()
        {
            byte[] overlayTableData = StreamHelper.ReadFile(OverlayTableFileName);
            int overlayCount = overlayTableData.Length / OverlayTableEntryLength;
            
            this.overlayOffsetPointers = new int[overlayCount];
            for (int i = 0; i < overlayCount; i++)
            {
                int overlayNumberPointer = (OverlayTableEntryLength * i) + 0x18;
                int overlayOffsetPointer = (OverlayTableEntryLength * i) + 0x04;
                int overlayNumber = BitConverter.ToInt32(overlayTableData, overlayNumberPointer);
                this.overlayOffsetPointers[overlayNumber] = overlayOffsetPointer;
            }
        }
    }
}
