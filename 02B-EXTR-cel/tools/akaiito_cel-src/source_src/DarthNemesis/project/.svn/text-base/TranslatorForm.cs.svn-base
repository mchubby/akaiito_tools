//-----------------------------------------------------------------------
// <copyright file="TranslatorForm.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-06</date>
// <summary>A UI template for a script dumper/inserter.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using DarthNemesis;

    /// <summary>
    /// A UI template for a script dumper/inserter.
    /// </summary>
    public partial class TranslatorForm : Form
    {
        private IGame game;
        private Log log;
        private AboutForm aboutForm;
        
        /// <summary>
        /// Initializes a new instance of the TranslatorForm class.
        /// </summary>
        /// <param name="game">Common properties and methods for the entire game.</param>
        public TranslatorForm(IGame game)
        {
            this.InitializeComponent();
            this.game = game;
            this.Text = game.Name;
            this.Icon = game.Icon;
            this.aboutForm = new AboutForm(this, game.Name, game.Version, game.Description);
            this.game.LoadMenu(this.menuStrip1);
            this.log = new Log(this.textBoxLog);
            
            int logLevel = this.game.Cache.Settings["logLevel"].Int32Value;
            this.SetLogLevel(logLevel);
        }
        
        private void SetLogLevel(int logLevel)
        {
            this.log.SetLogLevel(logLevel);
            this.debugToolStripMenuItem.Checked = 0 == logLevel;
            this.infoToolStripMenuItem.Checked  = 1 == logLevel;
            this.errorToolStripMenuItem.Checked = 3 == logLevel;
            this.game.Cache.Settings["logLevel"].Int32Value = logLevel;
        }
        
        #region File Management
        private bool UnpackFiles()
        {
            bool isUnpackSuccessful = true;
            if (this.game.Files.PackFiles.Count > 0)
            {
                this.log.Info("Unpacking archives...");
                isUnpackSuccessful &= this.UnpackFile(this.game.Files);
            }
            
            this.game.IsUnpacked = isUnpackSuccessful;
            return isUnpackSuccessful;
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private bool UnpackFile(IPackFile packFile)
        {
            bool isUnpackSuccessful = true;
            try
            {
                packFile.Unpack();
                if (!"Game".Equals(packFile.FileName))
                {
                    this.log.Debug(packFile.FileName, "Unpacked successfully.");
                }
                
                this.Refresh();
            }
            catch (SystemException exc)
            {
                this.log.Error(packFile.FileName, "Error: " + exc.Message);
                isUnpackSuccessful = false;
            }
            
            foreach (IPackFile file in packFile.PackFiles)
            {
                isUnpackSuccessful &= this.UnpackFile(file);
            }
            
            return isUnpackSuccessful;
        }
        
        private void LoadFiles()
        {
            this.log.Info("Loading...");
            this.Refresh();
            bool isLoadSuccessful = this.LoadPackFile(this.game.Files);
            this.saveToolStripMenuItem.Enabled = isLoadSuccessful;
            this.importToolStripMenuItem.Enabled = isLoadSuccessful;
            this.exportToolStripMenuItem.Enabled = isLoadSuccessful;
            this.log.Info("Done.");
        }
        
        private bool LoadPackFile(IPackFile packFile)
        {
            bool isLoadSuccessful = true;
            
            foreach (ITextFile file in packFile.TextFiles)
            {
                isLoadSuccessful &= this.LoadTextFile(file);
            }
            
            foreach (IPackFile file in packFile.PackFiles)
            {
                isLoadSuccessful &= this.LoadPackFile(file);
            }
            
            return isLoadSuccessful;
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private bool LoadTextFile(ITextFile textFile)
        {
            bool isLoadSuccessful = true;
            try
            {
                if (textFile.Load())
                {
                    this.log.Debug(textFile.FileName, "Loaded successfully.");
                }
                else
                {
                    this.log.Debug(textFile.FileName, "Needs to be analyzed first.");
                }
            }
            catch (SystemException exc)
            {
                this.log.Error(textFile.FileName, "Error: " + exc.Message);
                isLoadSuccessful = false;
            }
            
            return isLoadSuccessful;
        }
        
        private void SaveFiles()
        {
            this.log.Info("Saving...");
            this.Refresh();
            this.SavePackFile(this.game.Files);
            this.log.Info("Done.");
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private bool SavePackFile(IPackFile packFile)
        {
            bool isModified = false;
            
            foreach (ITextFile file in packFile.TextFiles)
            {
                isModified |= this.SaveTextFile(file);
            }
            
            foreach (IPackFile file in packFile.PackFiles)
            {
                isModified |= this.SavePackFile(file);
            }
            
            try
            {
                if (isModified)
                {
                    packFile.Pack();
                    if (!"Game".Equals(packFile.FileName))
                    {
                        this.log.Info(packFile.FileName, "Repacked successfully.");
                    }
                }
                else
                {
                    this.log.Debug(packFile.FileName, "Has not been modified.");
                }
            }
            catch (SystemException exc)
            {
                this.log.Error(packFile.FileName, "Error: " + exc.Message);
            }
            
            return isModified;
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private bool SaveTextFile(ITextFile textFile)
        {
            bool isModified = false;
            try
            {
                if (textFile.Save())
                {
                    this.log.Info(textFile.FileName, "Saved successfully.");
                    isModified = true;
                }
                else
                {
                    this.log.Debug(textFile.FileName, "Has not been modified.");
                }
            }
            catch (SystemException exc)
            {
                this.log.Error(textFile.FileName, "Error: " + exc.Message);
            }
            
            return isModified;
        }
        
        private void ImportFiles()
        {
            this.log.Info("Importing text...");
            this.Refresh();
            this.ImportPackFile(this.game.Files);
            this.log.Info("Done.");
        }
        
        private void ImportPackFile(IPackFile packFile)
        {
            foreach (ITextFile file in packFile.TextFiles)
            {
                this.ImportTextFile(file);
            }
            
            foreach (IPackFile file in packFile.PackFiles)
            {
                this.ImportPackFile(file);
            }
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private void ImportTextFile(ITextFile textFile)
        {
            try
            {
                if (textFile.Import())
                {
                    this.log.Info(textFile.FileName, "Imported successfully.");
                }
                else
                {
                    this.log.Debug(textFile.FileName, "Has not been modified.");
                }
            }
            catch (SystemException exc)
            {
                this.log.Error(textFile.FileName, "Error: " + exc.Message);
            }
        }
        
        private void ExportFiles()
        {
            this.log.Info("Exporting text...");
            this.Refresh();
            this.ExportPackFile(this.game.Files);
            this.log.Info("Done. Output is in the text folder.");
        }
        
        private void ExportPackFile(IPackFile packFile)
        {
            foreach (ITextFile file in packFile.TextFiles)
            {
                this.ExportTextFile(file);
            }
            
            foreach (IPackFile file in packFile.PackFiles)
            {
                this.ExportPackFile(file);
            }
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private void ExportTextFile(ITextFile textFile)
        {
            try
            {
                if (textFile.Export())
                {
                    this.log.Info(textFile.FileName, "Exported successfully.");
                }
                else
                {
                    this.log.Debug(textFile.FileName, "Needs to be analyzed first.");
                }
            }
            catch (SystemException exc)
            {
                this.log.Error(textFile.FileName, "Error: " + exc.Message);
            }
        }
        #endregion
        
        #region Event Handlers
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private void TranslatorFormShown(object sender, EventArgs e)
        {
            this.log.Clear();
            this.log.Info("[Message Log]");
            this.Refresh();
            
            try
            {
                this.game.InitializeFiles();
                
                if (this.game.IsUnpacked)
                {
                    this.log.Clear();
                    this.LoadFiles();
                }
            }
            catch (SystemException exc)
            {
                this.log.Error("Unable to initialize: " + exc.Message);
            }
        }
        
        private void TranslatorFormClosed(object sender, FormClosedEventArgs e)
        {
            this.game.Dispose();
        }
        
        private void LoadToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.log.Clear();
            if (this.game.IsUnpacked || this.UnpackFiles())
            {
                this.LoadFiles();
            }
            else
            {
                this.log.Info("Failed to unpack archives. Please ensure that you are running this program from the dslazy/dsbuff folder where you extracted the game files.");
            }
        }
        
        private void SaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.log.Clear();
            this.SaveFiles();
        }
        
        private void ImportToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.log.Clear();
            this.ImportFiles();
        }
        
        private void ExportToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.log.Clear();
            this.ExportFiles();
        }
        
        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        private void LogLevelToolStripMenuItemClick(object sender, EventArgs e)
        {
            int logLevel = Convert.ToInt32(((ToolStripItem)sender).Tag, CultureInfo.InvariantCulture);
            this.SetLogLevel(logLevel);
        }
        
        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.aboutForm.ShowDialog();
        }
        #endregion
    }
}
