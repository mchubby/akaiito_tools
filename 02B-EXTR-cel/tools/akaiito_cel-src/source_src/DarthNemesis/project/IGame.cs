//-----------------------------------------------------------------------
// <copyright file="IGame.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-06</date>
// <summary>Interface for common properties and methods for the entire game.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text;
    
    /// <summary>
    /// Interface for common properties and methods for the entire game.
    /// </summary>
    public interface IGame : IDisposable
    {
        /// <summary>
        /// Gets the application name.
        /// </summary>
        /// <value>The application name.</value>
        string Name
        {
            get;
        }
        
        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <value>The application version.</value>
        string Version
        {
            get;
        }
        
        /// <summary>
        /// Gets the application description.
        /// </summary>
        /// <value>The application description.</value>
        string Description
        {
            get;
        }
        
        /// <summary>
        /// Gets the application icon.
        /// </summary>
        /// <value>The application icon.</value>
        Icon Icon
        {
            get;
        }
        
        /// <summary>
        /// Gets the persistent data cache.
        /// </summary>
        /// <value>The persistent data cache.</value>
        XmlConfig Cache
        {
            get;
        }
        
        /// <summary>
        /// Gets the text encoding used by script files in the project.
        /// </summary>
        /// <value>The text encoding used by script files in the project.</value>
        Encoding Encoding
        {
            get;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the game has been successfully unpacked.
        /// </summary>
        /// <value>Whether the game has been successfully unpacked.</value>
        bool IsUnpacked
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets a wrapper for all the text and pack files in the game.
        /// </summary>
        /// <value>A wrapper for all the text and pack files in the game.</value>
        IPackFile Files
        {
            get;
        }
        
        /// <summary>
        /// Populates the list of files in the game.
        /// </summary>
        void InitializeFiles();
        
        /// <summary>
        /// Retrieves the cached file modification time.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The file modification time.</returns>
        long GetTimestamp(string fileName);
        
        /// <summary>
        /// Stores the cached file modification time.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="value">The file modification time.</param>
        void SetTimestamp(string fileName, long value);
        
        /// <summary>
        /// Converts a range of bytes into a text string using the project's Encoding.
        /// </summary>
        /// <param name="data">A byte array containing the data to be converted.</param>
        /// <param name="offset">The starting offset of the data in the array.</param>
        /// <returns>The converted text string.</returns>
        string GetText(byte[] data, int offset);
        
        /// <summary>
        /// Converts a text string into an array of bytes using the project's Encoding.
        /// </summary>
        /// <param name="text">The text string to be converted.</param>
        /// <returns>The converted byte array.</returns>
        byte[] GetBytes(string text);
        
        /// <summary>
        /// Adds any custom menu options for the game.
        /// </summary>
        /// <param name="menu">The menu strip to update.</param>
        void LoadMenu(System.Windows.Forms.MenuStrip menu);
    }
}
