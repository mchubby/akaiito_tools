//-----------------------------------------------------------------------
// <copyright file="IPackFile.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-06</date>
// <summary>Standard interface for script files archives.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    
    /// <summary>
    /// Standard interface for script file archives.
    /// </summary>
    public interface IPackFile
    {
        /// <summary>
        /// Gets the name of the file relative to the directory where the game was extracted.
        /// </summary>
        /// <value>The file path.</value>
        string FileName
        {
            get;
        }
        
        /// <summary>
        /// Gets any text files in the archive.
        /// </summary>
        /// <value>A collection of text files.</value>
        ICollection<ITextFile> TextFiles
        {
            get;
        }
        
        /// <summary>
        /// Gets any pack files in the archive.
        /// </summary>
        /// <value>A collection of pack files.</value>
        ICollection<IPackFile> PackFiles
        {
            get;
        }
        
        /// <summary>
        /// Extracts the game files from the pack file.
        /// </summary>
        void Unpack();
        
        /// <summary>
        /// Re-inserts the game files into the pack file.
        /// </summary>
        void Pack();
    }
}
