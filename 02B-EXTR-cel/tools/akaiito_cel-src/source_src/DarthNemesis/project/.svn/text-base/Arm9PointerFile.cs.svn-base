//-----------------------------------------------------------------------
// <copyright file="Arm9PointerFile.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-09</date>
// <summary>A pointer analyzer for arm9 files.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using DarthNemesis;

    /// <summary>
    /// A pointer analyzer for arm9 files.
    /// </summary>
    public class Arm9PointerFile : ITextFile
    {
        #region Variables
        private const int Arm9Offset = 0x02000000;
        private const string GameDirectoryPrefix = @"NDS_UNPACK\";
        private const string TextDirectoryPrefix = @"text\";
        private const string PointerDirectoryPrefix = @"pointers\";
        private const string GameFileExtension = ".bin";
        private const string TextFileExtension = ".sjs";
        private const string PointerFileExtension = ".ptr";
        private IGame game;
        private string fileName;
        private string[] strings;
        private int[] pointerOffsets;
        private int[] pointerTable;
        private SortedDictionary<int, int> validTextRanges;
        private bool useValid;
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the Arm9PointerFile class.
        /// </summary>
        /// <param name="game">Common properties and methods for the entire game.</param>
        /// <param name="useValid">Whether or not to check lines against a list of valid strings.</param>
        public Arm9PointerFile(IGame game, bool useValid)
        {
            this.fileName = "arm9.bin";
            this.game = game;
            this.useValid = useValid;
        }
        
        /// <summary>
        /// Initializes a new instance of the Arm9PointerFile class.
        /// </summary>
        /// <param name="game">Common properties and methods for the entire game.</param>
        public Arm9PointerFile(IGame game) : this(game, false)
        {
        }
        
        #region Properties
        
        /// <summary>
        /// Gets the name of the file relative to the directory where the game was extracted.
        /// </summary>
        /// <value>The file path.</value>
        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }
        
        private string GameFileName
        {
            get
            {
                return GameDirectoryPrefix + Path.ChangeExtension(this.fileName, GameFileExtension);
            }
        }
        
        private string PointerFileDir
        {
            get
            {
                return Path.GetDirectoryName(PointerDirectoryPrefix + this.fileName);
            }
        }
        
        private string TextFileDir
        {
            get
            {
                return Path.GetDirectoryName(TextDirectoryPrefix + this.fileName);
            }
        }
        
        private string PointerFileName
        {
            get
            {
                return PointerDirectoryPrefix + Path.ChangeExtension(this.fileName, PointerFileExtension);
            }
        }
        
        private string TextFileName
        {
            get
            {
                return TextDirectoryPrefix + Path.ChangeExtension(this.fileName, TextFileExtension);
            }
        }
        #endregion
        
        /// <summary>
        /// Extracts the text strings from the game file and handles caching.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully loaded.</returns>
        public bool Load()
        {
            this.SearchForPointers();
            return true;
        }
        
        /// <summary>
        /// Inserts the text strings back into the game file and handles caching.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully saved.</returns>
        public bool Save()
        {
            this.ExportPointerSpecificationFile();
            return true;
        }
        
        /// <summary>
        /// Writes the text strings into an easily editable text file and handles caching.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully exported.</returns>
        public bool Export()
        {
            this.ExportListOfAllPointersAndLines();
            return true;
        }
        
        /// <summary>
        /// Reads in the replacement text strings from the text file and handles caching.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully imported.</returns>
        public bool Import()
        {
            this.ImportListOfValidPointers();
            return true;
        }
        
        /*
        private void SearchForPointers()
        {
            byte[] fileData = StreamHelper.ReadFile(this.GameFileName);
            
            List<int> offsets = new List<int>();
            List<int> pointers = new List<int>();
            for (int i = 0; i + Constants.PointerLength - 1 < fileData.Length; i += Constants.PointerLength)
            {
                int pointer = BitConverter.ToInt32(fileData, i);
                if (pointer >= Arm9Offset && pointer <= fileData.Length + Arm9Offset)
                {
                    offsets.Add(i);
                    pointers.Add(pointer - Arm9Offset);
                }
            }
            
            this.pointerOffsets = offsets.ToArray();
            this.pointerTable = pointers.ToArray();
            this.strings = new string[pointers.Count];
            for (int i = 0; i < pointers.Count; i++)
            {
                this.strings[i] = this.game.GetText(fileData, pointers[i]);
            }
        }
         */
        
        private void SearchForPointers()
        {
            List<string> validList = new List<string>();
            
            if (this.useValid)
            {
                FileStream validFile = new FileStream(TextDirectoryPrefix + @"\valid.sjs", FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(validFile, this.game.Encoding);
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    validList.Add(line);
                }
            }
        
            byte[] fileData = StreamHelper.ReadFile(this.GameFileName);
            
            List<int> offsets = new List<int>();
            List<int> pointers = new List<int>();
            for (int i = 0; i + Constants.PointerLength - 1 < fileData.Length; i += Constants.PointerLength)
            {
                int pointer = BitConverter.ToInt32(fileData, i);
                if (pointer >= Arm9Offset && pointer <= fileData.Length + Arm9Offset)
                {
                    string line = this.game.GetText(fileData, pointer - Arm9Offset);
                    if (!this.useValid || validList.Contains(line))
                    {
                        offsets.Add(i);
                        pointers.Add(pointer - Arm9Offset);
                    }
                }
            }
            
            this.pointerOffsets = offsets.ToArray();
            this.pointerTable = pointers.ToArray();
            this.strings = new string[pointers.Count];
            for (int i = 0; i < pointers.Count; i++)
            {
                this.strings[i] = this.game.GetText(fileData, pointers[i]);
            }
        }
        
        private void ExportListOfAllPointersAndLines()
        {
            if (!Directory.Exists(this.TextFileDir))
            {
                Directory.CreateDirectory(this.TextFileDir);
            }
            
            FileStream file = new FileStream(this.TextFileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(file, this.game.Encoding);
            
            for (int i = 0; i < this.strings.Length; i++)
            {
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:x8} {1:x8}: {2}", this.pointerOffsets[i], this.pointerTable[i], this.strings[i]));
            }
            
            writer.Close();
            
            /*
            List<string> sortedStrings = new List<string>(this.strings.Length);
            Dictionary<string, byte> dupes = new Dictionary<string, byte>();
            for (int i = 0; i < this.strings.Length; i++)
            {
                if (!dupes.ContainsKey(this.strings[i]))
                {
                    sortedStrings.Add(this.strings[i]);
                    dupes.Add(this.strings[i], 0);
                }
            }
            
            sortedStrings.Sort();
            StreamHelper.WriteLinesToFile(TextDirectoryPrefix + @"\valid.sjs", sortedStrings.ToArray(), this.game.Encoding);
            */
        }
        
        private void ImportListOfValidPointers()
        {
            FileStream file = new FileStream(this.TextFileName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file, this.game.Encoding);
            
            List<int> offsets = new List<int>();
            string line = StreamHelper.ReadNextLine(reader);
            int i = 0;
            while (line != null && line.Length > 0)
            {
                i++;
                int offset = Convert.ToInt32(line.Substring(0, 8), 16);
                offsets.Add(offset);
                line = StreamHelper.ReadNextLine(reader);
            }
            
            reader.Close();
            
            ////========================
            
            byte[] fileData = StreamHelper.ReadFile(this.GameFileName);
            
            this.pointerOffsets = offsets.ToArray();
            SortedDictionary<int, int> textRanges = new SortedDictionary<int, int>();
            for (i = 0; i < this.pointerOffsets.Length; i++)
            {
                int key = BitConverter.ToInt32(fileData, this.pointerOffsets[i]) - Arm9Offset;
                int val = key;
                for (val = key; fileData[val] != 0x00 || (val % 4) != 3; val++)
                {
                }
                
                if (!textRanges.ContainsKey(key))
                {
                    textRanges.Add(key, val);
                }
            }
            
            this.validTextRanges = new SortedDictionary<int, int>();
            int startOffset = -1;
            int endOffset = -1;
            foreach (KeyValuePair<int, int> kvp in textRanges)
            {
                if (kvp.Key == endOffset + 1)
                {
                    endOffset = kvp.Value;
                }
                else
                {
                    if (startOffset != -1)
                    {
                        this.validTextRanges.Add(startOffset, endOffset);
                    }
                    
                    startOffset = kvp.Key;
                    endOffset = kvp.Value;
                }
            }
            
            this.validTextRanges.Add(startOffset, endOffset);
        }
        
        private void ExportPointerSpecificationFile()
        {
            if (!Directory.Exists(this.PointerFileDir))
            {
                Directory.CreateDirectory(this.PointerFileDir);
            }
            
            FileStream file = new FileStream(this.PointerFileName, FileMode.Create, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(file);
            
            writer.Write(this.pointerOffsets.Length);
            for (int i = 0; i < this.pointerOffsets.Length; i++)
            {
                writer.Write(this.pointerOffsets[i]);
            }
            
            writer.Write(this.validTextRanges.Count);
            
            foreach (KeyValuePair<int, int> kvp in this.validTextRanges)
            {
                writer.Write(kvp.Key);
                writer.Write(kvp.Value);
            }
            
            writer.Close();
        }
    }
}
