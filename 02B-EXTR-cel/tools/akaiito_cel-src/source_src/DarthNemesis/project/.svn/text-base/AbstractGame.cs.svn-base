//-----------------------------------------------------------------------
// <copyright file="AbstractGame.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-07</date>
// <summary>Common properties and methods for the entire game.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using DarthNemesis;
    
    /// <summary>
    /// Common properties and methods for the entire game.
    /// </summary>
    public abstract class AbstractGame : IGame
    {
        private XmlConfig cache = new XmlConfig("cache.xml", true);
        private GameFiles files;
        
        /// <summary>
        /// Initializes a new instance of the AbstractGame class.
        /// </summary>
        protected AbstractGame()
        {
            this.files = new GameFiles();
        }
        
        /// <summary>
        /// Finalizes an instance of the AbstractGame class.
        /// </summary>
        ~AbstractGame()
        {
            this.Dispose(false);
        }
        
        /// <summary>
        /// Gets the application name.
        /// </summary>
        /// <value>The application name.</value>
        public abstract string Name
        {
            get;
        }
        
        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <value>The application version.</value>
        public abstract string Version
        {
            get;
        }
        
        /// <summary>
        /// Gets the application description.
        /// </summary>
        /// <value>The application description.</value>
        public abstract string Description
        {
            get;
        }
        
        /// <summary>
        /// Gets the application icon.
        /// </summary>
        /// <value>The application icon.</value>
        public Icon Icon
        {
            get
            {
                return new Icon(GetType(), "Game.ico");
            }
        }
        
        /// <summary>
        /// Gets the persistent data cache.
        /// </summary>
        /// <value>The persistent data cache.</value>
        public XmlConfig Cache
        {
            get
            {
                return this.cache;
            }
        }
        
        /// <summary>
        /// Gets the text encoding used by script files in the project.
        /// </summary>
        /// <value>The text encoding used by script files in the project.</value>
        public abstract Encoding Encoding
        {
            get;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the game has been successfully unpacked.
        /// </summary>
        /// <value>Whether the game has been successfully unpacked.</value>
        public bool IsUnpacked
        {
            get
            {
                return this.cache.Settings["unpacked"].BooleanValue;
            }
            
            set
            {
                this.cache.Settings["unpacked"].BooleanValue = value;
            }
        }
        
        /// <summary>
        /// Gets a wrapper for all the text and pack files in the game.
        /// </summary>
        /// <value>A wrapper for all the text and pack files in the game.</value>
        public IPackFile Files
        {
            get
            {
                return this.files;
            }
        }
        
        /// <summary>
        /// Populates the list of files in the game.
        /// </summary>
        public abstract void InitializeFiles();
        
        /// <summary>
        /// Retrieves the cached file modification time.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The file modification time.</returns>
        public long GetTimestamp(string fileName)
        {
            return this.cache.Settings["cache"][fileName].Int64Value;
        }
        
        /// <summary>
        /// Stores the cached file modification time.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="value">The file modification time.</param>
        public void SetTimestamp(string fileName, long value)
        {
            this.cache.Settings["cache"][fileName].Int64Value = value;
        }
        
        /// <summary>
        /// Converts a range of bytes into a text string using the project's Encoding.
        /// </summary>
        /// <param name="data">A byte array containing the data to be converted.</param>
        /// <param name="offset">The starting offset of the data in the array.</param>
        /// <returns>The converted text string.</returns>
        public abstract string GetText(byte[] data, int offset);
        
        /// <summary>
        /// Converts a text string into an array of bytes using the project's Encoding.
        /// </summary>
        /// <param name="text">The text string to be converted.</param>
        /// <returns>The converted byte array.</returns>
        public abstract byte[] GetBytes(string text);
        
        /// <summary>
        /// Adds any custom menu options for the game.
        /// </summary>
        /// <param name="menu">The menu strip to update.</param>
        public virtual void LoadMenu(System.Windows.Forms.MenuStrip menu)
        {
        }
        
        /// <summary>
        /// Implement IDisposable.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">A value indicating whether this method has been called by user code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.cache.Commit();
                this.cache.Dispose();
            }
            
            this.files = null;
        }
    }
}
