//-----------------------------------------------------------------------
// <copyright file="Arm9PackFile.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-06</date>
// <summary>A compressed executable binary file.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using DarthNemesis;
    
    /// <summary>
    /// A compressed executable binary file.
    /// </summary>
    public class Arm9PackFile : IPackFile
    {
        #region Variables
        private const string CompressedDirectoryPrefix = @"NDS_UNPACK\";
        private const string DecompressedDirectoryPrefix = @"cache\";
        private const string CompressedFileExtension = ".bin";
        private const string DecompressedFileExtension = ".bin";
        private IGame game;
        private string fileName;
        private List<ITextFile> textFiles;
        private List<IPackFile> packFiles;
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the Arm9PackFile class.
        /// </summary>
        /// <param name="game">Common properties and methods for the entire game.</param>
        public Arm9PackFile(IGame game)
        {
            this.fileName = "arm9.bin";
            this.game = game;
            this.textFiles = new List<ITextFile>();
            this.packFiles = new List<IPackFile>();
            if (this.game.IsUnpacked)
            {
                this.InitializeChildren();
            }
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
        
        /// <summary>
        /// Gets any text files in the archive.
        /// </summary>
        /// <value>A collection of text files.</value>
        public ICollection<ITextFile> TextFiles
        {
            get
            {
                return new ReadOnlyCollection<ITextFile>(this.textFiles);
            }
        }
        
        /// <summary>
        /// Gets any pack files in the archive.
        /// </summary>
        /// <value>A collection of pack files.</value>
        public ICollection<IPackFile> PackFiles
        {
            get
            {
                return new ReadOnlyCollection<IPackFile>(this.packFiles);
            }
        }
        
        private string CompressedFileName
        {
            get
            {
                return CompressedDirectoryPrefix + Path.ChangeExtension(this.fileName, CompressedFileExtension);
            }
        }
        
        private string DecompressedFileName
        {
            get
            {
                return DecompressedDirectoryPrefix + Path.ChangeExtension(this.fileName, DecompressedFileExtension);
            }
        }
        #endregion
        
        /// <summary>
        /// Unpacks and initializes all files from the pack.
        /// </summary>
        public void Unpack()
        {
            int headerLength;
            byte[] footer;
            
            this.textFiles.Clear();
            
            byte[] fileData = StreamHelper.ReadFile(this.CompressedFileName);
            fileData = CompressionManager.DecompressArm9(fileData, out headerLength, out footer);
            StreamHelper.WriteFile(this.DecompressedFileName, fileData);
            
            this.game.Cache.Settings["arm9"]["headerLength"].Int32Value = headerLength;
            this.game.Cache.Settings["arm9"]["footer"].Value = EncodeFooter(footer);
            
            this.InitializeChildren();
        }
        
        /// <summary>
        /// Repacks all files into the pack.
        /// </summary>
        public void Pack()
        {
            int headerLength = this.game.Cache.Settings["arm9"]["headerLength"].Int32Value;
            byte[] footer = DecodeFooter(this.game.Cache.Settings["arm9"]["footer"].Value);
            
            byte[] fileData = StreamHelper.ReadFile(this.DecompressedFileName);
            fileData = CompressionManager.CompressArm9(fileData, headerLength, footer);
            StreamHelper.WriteFile(this.CompressedFileName, fileData);
        }
        
        private static string EncodeFooter(byte[] footer)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(footer.Length);
            for (int i = 0; i < footer.Length; i++)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", footer[i]));
            }
            
            return builder.ToString();
        }
        
        private static byte[] DecodeFooter(string encode)
        {
            byte[] footer = new byte[encode.Length / 2];
            for (int i = 0; i < footer.Length; i++)
            {
                footer[i] = Convert.ToByte(encode.Substring(2 * i, 2), 16);
            }
            
            return footer;
        }
        
        private void InitializeChildren()
        {
            this.textFiles.Add(new Arm9File(this.game, true));
        }
    }
}
