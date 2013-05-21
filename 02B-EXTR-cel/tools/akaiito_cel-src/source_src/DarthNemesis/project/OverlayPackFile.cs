//-----------------------------------------------------------------------
// <copyright file="OverlayPackFile.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-09</date>
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
    public class OverlayPackFile : IPackFile
    {
        #region Variables
        private const string OverlayTableFileName = @"NDS_UNPACK\y9.bin";
        private const string CompressedDirectoryPrefix = @"NDS_UNPACK\";
        private const string DecompressedDirectoryPrefix = @"cache\";
        private const string CompressedFileExtension = ".bin";
        private const string DecompressedFileExtension = ".bin";
        private const int OverlayTableEntryLength = 0x20;
        private IGame game;
        private string fileName;
        private int overlayNumber;
        private int[] overlayLengthPointers;
        private List<ITextFile> textFiles;
        private List<IPackFile> packFiles;
        
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the OverlayPackFile class.
        /// </summary>
        /// <param name="overlayNumber">The index of the overlay to load.</param>
        /// <param name="game">Common properties and methods for the entire game.</param>
        public OverlayPackFile(int overlayNumber, IGame game)
        {
            this.overlayNumber = overlayNumber;
            this.fileName = string.Format(CultureInfo.InvariantCulture, @"overlay\overlay_{0:0000}.bin", overlayNumber);
            this.game = game;
            this.textFiles = new List<ITextFile>();
            this.packFiles = new List<IPackFile>();
            if (this.game.IsUnpacked)
            {
                this.InitializeChildren();
            }
            
            this.GenerateLengthPointers();
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
        
        private int CompressedFileSize
        {
            set
            {
                byte[] overlayTableData = StreamHelper.ReadFile(OverlayTableFileName);
                byte[] sizeBytes = BitConverter.GetBytes(value);
                Array.Copy(sizeBytes, 0, overlayTableData, this.overlayLengthPointers[this.overlayNumber], 0x03);
                StreamHelper.WriteFile(OverlayTableFileName, overlayTableData);
            }
        }
        #endregion
        
        /// <summary>
        /// Unpacks and initializes all files from the pack.
        /// </summary>
        public void Unpack()
        {
            byte[] fileData = StreamHelper.ReadFile(this.CompressedFileName);
            fileData = CompressionManager.DecompressOverlay(fileData);
            StreamHelper.WriteFile(this.DecompressedFileName, fileData);
            
            this.InitializeChildren();
        }
        
        /// <summary>
        /// Repacks all files into the pack.
        /// </summary>
        public void Pack()
        {
            byte[] fileData = StreamHelper.ReadFile(this.DecompressedFileName);
            fileData = CompressionManager.CompressOverlay(fileData);
            StreamHelper.WriteFile(this.CompressedFileName, fileData);
            this.CompressedFileSize = fileData.Length;
        }
        
        private void InitializeChildren()
        {
            this.textFiles.Add(new OverlayFile(this.overlayNumber, this.game, true));
            ////this.textFiles.Add(new OverlayPointerFile(this.overlayNumber, this.game));
        }
        
        private void GenerateLengthPointers()
        {
            byte[] overlayTableData = StreamHelper.ReadFile(OverlayTableFileName);
            int overlayCount = overlayTableData.Length / OverlayTableEntryLength;
            
            this.overlayLengthPointers = new int[overlayCount];
            for (int i = 0; i < overlayCount; i++)
            {
                int overlayNumberPointer = (OverlayTableEntryLength * i) + 0x18;
                int overlayLengthPointer = overlayNumberPointer + 0x04;
                int overlayNumber = BitConverter.ToInt32(overlayTableData, overlayNumberPointer);
                this.overlayLengthPointers[overlayNumber] = overlayLengthPointer;
            }
        }
    }
}
