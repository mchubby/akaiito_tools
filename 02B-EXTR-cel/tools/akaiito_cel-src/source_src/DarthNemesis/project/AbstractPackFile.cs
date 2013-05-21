//-----------------------------------------------------------------------
// <copyright file="AbstractPackFile.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-16</date>
// <summary>Common properties and methods for a pack file.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// Common properties and methods for a pack file.
    /// </summary>
    public abstract class AbstractPackFile : IPackFile
    {
        #region Variables
        private string fileName;
        private IGame game;
        private List<ITextFile> textFiles;
        private List<IPackFile> packFiles;
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the AbstractPackFile class.
        /// </summary>
        /// <param name="fileName">The file path relative to the directory where the game was extracted.</param>
        /// <param name="game">Common properties and methods for the entire game.</param>
        protected AbstractPackFile(string fileName, IGame game)
        {
            this.fileName = fileName;
            this.game = game;
            this.textFiles = new List<ITextFile>();
            this.packFiles = new List<IPackFile>();
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
                return (ICollection<ITextFile>)this.textFiles;
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
                return (ICollection<IPackFile>)this.packFiles;
            }
        }
        
        /// <summary>
        /// Gets common properties and methods for the entire game.
        /// </summary>
        /// <value>Common properties and methods for the entire game.</value>
        protected IGame Game
        {
            get
            {
                return this.game;
            }
        }
        #endregion
        
        /// <summary>
        /// Unpacks and initializes all files from the pack.
        /// </summary>
        public abstract void Unpack();
        
        /// <summary>
        /// Repacks all files into the pack.
        /// </summary>
        public abstract void Pack();
    }
}
