//-----------------------------------------------------------------------
// <copyright file="XmlConfig.cs" company="VAkos">
// Copyright (c) 2007 All Right Reserved
// </copyright>
// <author>Vandra Akos</author>
// <date>2007-01-14</date>
// <summary>The class which represents a configuration xml file.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// The class which represents a configuration xml file.
    /// </summary>
    public class XmlConfig : IDisposable
    {
        private XmlDocument xmldoc;
        private string originalFile;
        private bool commitOnUnload = true;
        private bool cleanupOnSave;

        /// <summary>
        /// Initializes a new instance of the XmlConfig class.
        /// Create an XmlConfig from an empty xml file
        /// containing only the rootelement named as 'xml'.
        /// </summary>
        public XmlConfig()
        {
            this.xmldoc = new XmlDocument();
            this.LoadXmlFromString("<xml></xml>");
        }

        /// <summary>
        /// Initializes a new instance of the XmlConfig class.
        /// Create an XmlConfig from an existing file, or create a new one.
        /// </summary>
        /// <param name="loadFromFile">
        /// Path and filename from where to load the xml file.
        /// </param>
        /// <param name="create">
        /// If file does not exist, controls whether we create it or throw an exception.
        /// </param>
        public XmlConfig(string loadFromFile, bool create)
        {
            this.xmldoc = new XmlDocument();
            this.LoadXmlFromFile(loadFromFile, create);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to clean the tree by
        /// stripping out empty nodes before saving the XML config file.
        /// </summary>
        /// <value>Whether to clean the tree before saving.</value>
        public bool CleanupOnSave
        {
            get { return this.cleanupOnSave; }
            set { this.cleanupOnSave = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether changes should be saved
        /// back to the current XML config file when unloading it.
        /// </summary>
        /// <value>Whether changes should be saved before unloading the file.</value>
        /// <remarks>
        /// <list type="bullet">
        /// <item>Only applies if it was loaded from a local file</item>
        /// <item>True by default</item>
        /// </list>
        /// </remarks>
        public bool CommitOnUnload
        {
            get
            {
                return this.commitOnUnload;
            }
            
            set
            {
                this.commitOnUnload = value;
            }
        }
        
        /// <summary>
        /// Gets the root ConfigSetting.
        /// </summary>
        /// <value>The root ConfigSetting.</value>
        public ConfigSetting Settings
        {
            get { return new ConfigSetting(this.xmldoc.DocumentElement); }
        }
        
        /// <summary>
        /// Check XML file if it conforms the config xml restrictions.
        /// 1. No nodes with two children of the same name.
        /// 2. Only alphanumerical names.
        /// </summary>
        /// <param name="silent">
        /// Whether to return a true/false value, or throw an exception on failure.
        /// </param>
        /// <returns>
        /// True on success and in case of silent mode false on failure.
        /// </returns>
        public bool ValidateXml(bool silent)
        {
            if (!this.Settings.Validate())
            {
                if (silent)
                {
                    return false;
                }
                else
                {
                    throw new FormatException("This is not a valid configuration xml file! Probably duplicate children with the same names, or non-alphanumerical tagnames!");
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Strip empty nodes from the whole tree.
        /// </summary>
        public void Clean()
        {
            this.Settings.Clean();
        }

        /// <summary>
        /// Load a new XmlDocument from a file.
        /// </summary>
        /// <param name="fileName">
        /// Path and filename from where to load the xml file.
        /// </param>
        /// <param name="create">
        /// If file does not exist, determines whether we should create it or throw an exception.
        /// </param>
        public void LoadXmlFromFile(string fileName, bool create)
        {
            if (this.CommitOnUnload)
            {
                this.Commit();
            }
            
            try
            {
                this.xmldoc.Load(fileName);
            }
            catch
            {
                if (!create)
                {
                    throw new ArgumentException("xmldoc.Load() failed! Probably file does NOT exist!");
                }
                else
                {
                    this.xmldoc.LoadXml("<xml></xml>");
                    this.Save(fileName);
                }
            }
            
            this.ValidateXml(false);
            this.originalFile = fileName;
        }

        /// <summary>
        /// Load a new XmlDocument from a file.
        /// </summary>
        /// <param name="fileName">
        /// Path and filename from where to load the xml file.
        /// </param>
        /// <remarks>
        /// Throws an exception if file does not exist
        /// </remarks>
        public void LoadXmlFromFile(string fileName)
        {
            this.LoadXmlFromFile(fileName, false);
        }

        /// <summary>
        /// Load a new XmlDocument from a string.
        /// </summary>
        /// <param name="xml">
        /// XML string.
        /// </param>
        public void LoadXmlFromString(string xml)
        {
            if (this.CommitOnUnload)
            {
                this.Commit();
            }
            
            this.xmldoc.LoadXml(xml);
            this.originalFile = null;
            this.ValidateXml(false);
        }

        /// <summary>
        /// Load an empty XmlDocument.
        /// </summary>
        /// <param name="rootElement">
        /// Name of root element.
        /// </param>
        public void NewXml(string rootElement)
        {
            if (this.CommitOnUnload)
            {
                this.Commit();
            }
            
            this.LoadXmlFromString(String.Format(CultureInfo.InvariantCulture, "<{0}></{0}>", rootElement));
        }

        /// <summary>
        /// Save configuration to an xml file.
        /// </summary>
        /// <param name="fileName">
        /// Path and filname where to save.
        /// </param>
        public void Save(string fileName)
        {
            this.ValidateXml(false);
            if (this.CleanupOnSave)
            {
                this.Clean();
            }
            
            this.xmldoc.Save(fileName);
            this.originalFile = fileName;
        }

        /// <summary>
        /// Save configuration to a stream.
        /// </summary>
        /// <param name="stream">
        /// Stream where to save.
        /// </param>
        public void Save(System.IO.Stream stream)
        {
            this.ValidateXml(false);
            if (this.CleanupOnSave)
            {
                this.Clean();
            }
            
            this.xmldoc.Save(stream);
        }

        /// <summary>
        /// If loaded from a file, commit any changes, by overwriting the file.
        /// </summary>
        /// <returns>
        /// True on success.
        /// False on failure, probably due to the file was not loaded from a file.
        /// </returns>
        public bool Commit()
        {
            if (this.originalFile != null)
            {
                this.Save(this.originalFile);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// If loaded from a file, trash any changes, and reload the file.
        /// </summary>
        /// <returns>
        /// True on success.
        /// False on failure, probably due to file was not loaded from a file.
        /// </returns>
        public bool Reload()
        {
            if (this.originalFile != null)
            {
                this.LoadXmlFromFile(this.originalFile);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        /// <summary>
        /// Save any modifications to the XML file before destruction
        /// if CommitOnUnload is true.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Save any modifications to the XML file before destruction
        /// if CommitOnUnload is true.
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.CommitOnUnload)
                {
                    this.Commit();
                }
            }
        }
    }
}
