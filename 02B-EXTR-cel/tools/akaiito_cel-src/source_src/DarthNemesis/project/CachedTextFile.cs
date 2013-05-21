//-----------------------------------------------------------------------
// <copyright file="CachedTextFile.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-06</date>
// <summary>A text file that keeps track of modifications and only updates when changed.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.IO;
    
    /// <summary>
    /// A text file that keeps track of modifications and only updates when changed.
    /// </summary>
    public abstract class CachedTextFile : ITextFile
    {
        #region Variables
        private string fileName;
        private IGame game;
        private long timestamp;
        private bool isModified;
        #endregion
        
        /// <summary>
        /// Initializes a new instance of the CachedTextFile class.
        /// </summary>
        /// <param name="fileName">The file path relative to the directory where the game was extracted.</param>
        /// <param name="game">Common properties and methods for the entire game.</param>
        protected CachedTextFile(string fileName, IGame game)
        {
            this.fileName = fileName;
            this.game = game;
        }
        
        private CachedTextFile()
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
        
        /// <summary>
        /// Gets the name of the text file relative to the directory where the game was extracated.
        /// </summary>
        /// <value>The name of the text file.</value>
        protected abstract string TextFileName
        {
            get;
        }
        
        /// <summary>
        /// Gets a value indicating whether the text file has been modified since it was last saved.
        /// </summary>
        /// <value>Whether the text file has been modified since it was last saved.</value>
        protected bool IsModified
        {
            get
            {
                return this.isModified;
            }
        }
        #endregion
        
        /// <summary>
        /// Extracts the text strings from the game file and handles caching.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully loaded.</returns>
        public bool Load()
        {
            if (!this.LoadText())
            {
                return false;
            }
            
            this.isModified = false;
            this.timestamp = this.game.GetTimestamp(this.fileName);
            
            return true;
        }
        
        /// <summary>
        /// Inserts the text strings back into the game file and handles caching.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully saved.</returns>
        public bool Save()
        {
            if (!this.isModified)
            {
                return false;
            }
            
            if (!this.SaveText())
            {
                return false;
            }
            
            this.isModified = false;
            this.game.SetTimestamp(this.fileName, this.timestamp);
            
            return true;
        }
        
        /// <summary>
        /// Writes the text strings into an easily editable text file and handles caching.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully exported.</returns>
        public bool Export()
        {
            if (!this.ExportText())
            {
                return false;
            }
            
            this.timestamp = StreamHelper.GetTimestamp(this.TextFileName);
            if (!this.isModified)
            {
                this.game.SetTimestamp(this.fileName, this.timestamp);
            }
            
            return true;
        }
        
        /// <summary>
        /// Reads in the replacement text strings from the text file and handles caching.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully imported.</returns>
        public bool Import()
        {
            long newTimestamp = StreamHelper.GetTimestamp(this.TextFileName);
            if (newTimestamp == this.timestamp)
            {
                return false;
            }
            
            if (!this.ImportText())
            {
                return false;
            }
            
            this.timestamp = newTimestamp;
            this.isModified = true;
            return true;
        }
        
        /// <summary>
        /// Extracts the text strings from the game file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully loaded.</returns>
        protected abstract bool LoadText();
        
        /// <summary>
        /// Inserts the text strings back into the game file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully saved.</returns>
        protected abstract bool SaveText();
        
        /// <summary>
        /// Writes the text strings into an easily editable text file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully exported.</returns>
        protected abstract bool ExportText();
        
        /// <summary>
        /// Reads in the replacement text strings from the text file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully imported.</returns>
        protected abstract bool ImportText();
    }
}
