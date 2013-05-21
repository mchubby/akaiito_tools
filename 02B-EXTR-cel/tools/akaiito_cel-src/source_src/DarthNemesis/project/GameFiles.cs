//-----------------------------------------------------------------------
// <copyright file="GameFiles.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-06</date>
// <summary>A wrapper for all the text and pack files in the game.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    
    /// <summary>
    /// A wrapper for all the text and pack files in the game.
    /// </summary>
    public class GameFiles : IPackFile
    {
        private List<ITextFile> textFiles;
        private List<IPackFile> packFiles;
        
        /// <summary>
        /// Initializes a new instance of the GameFiles class.
        /// </summary>
        public GameFiles()
        {
            this.textFiles = new List<ITextFile>();
            this.packFiles = new List<IPackFile>();
        }
        
        /// <summary>
        /// Gets the name of the file relative to the directory where the game was extracted.
        /// </summary>
        /// <value>The file path.</value>
        public string FileName
        {
            get
            {
                return "Game";
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
                return this.textFiles;
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
                return this.packFiles;
            }
        }
        
        /// <summary>
        /// Extracts the game files from the pack file.
        /// </summary>
        public void Unpack()
        {
            // TODO unpack ROM?
        }
        
        /// <summary>
        /// Re-inserts the game files into the pack file.
        /// </summary>
        public void Pack()
        {
            // TODO pack ROM?
        }
    }
}
