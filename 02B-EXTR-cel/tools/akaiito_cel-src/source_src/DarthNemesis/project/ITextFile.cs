//-----------------------------------------------------------------------
// <copyright file="ITextFile.cs" company="DarthNemesis">
// Copyright (c) 2009 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2009-09-17</date>
// <summary>Standard interface for script files.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    
    /// <summary>
    /// Standard interface for script files.
    /// </summary>
    public interface ITextFile
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
        /// Extracts the text strings from the game file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully loaded.</returns>
        bool Load();
        
        /// <summary>
        /// Inserts the text strings back into the game file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully saved.</returns>
        bool Save();
        
        /// <summary>
        /// Writes the text strings into an easily editable text file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully exported.</returns>
        bool Export();
        
        /// <summary>
        /// Reads in the replacement text strings from the text file.
        /// </summary>
        /// <returns>A value indicating whether the file was successfully imported.</returns>
        bool Import();
    }
}
