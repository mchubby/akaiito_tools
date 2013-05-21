//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-22</date>
// <summary>Primary application form.</summary>
//-----------------------------------------------------------------------

namespace BatchLZ77
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using DarthNemesis;
    
    /// <summary>
    /// Primary application form.
    /// </summary>
    public partial class MainForm : Form
    {
        private AboutForm aboutForm;
        private List<string> errors;
        
        /// <summary>
        /// Initializes a new instance of the MainForm class.
        /// </summary>
        public MainForm()
        {
            this.InitializeComponent();
            this.errors = new List<string>();
            this.aboutForm = new AboutForm(this, "BatchLZ77", "v1.4", "A multi-file LZ77 (de)compression\nutility for Nintendo DS files.");
        }
        
        #region Event Handlers
        private void DecompressToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.ClearErrors();
                this.DecompressTree(this.openFileDialog1.FileNames);
                this.ShowErrors();
            }
        }
        
        private void PanelDecompressDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        
        private void PanelDecompressDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                this.ClearErrors();
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.DecompressTree(fileNames);
                this.ShowErrors();
            }
        }
        
        private void CompressToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.ClearErrors();
                this.CompressTree(this.openFileDialog1.FileNames);
                this.ShowErrors();
            }
        }
        
        private void PanelCompressDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        
        private void PanelCompressDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                this.ClearErrors();
                string[] fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                this.CompressTree(fileNames);
                this.ShowErrors();
            }
        }
        
        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        private void Type10ToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (this.type11ToolStripMenuItem.Checked)
            {
                this.type10ToolStripMenuItem.Checked = true;
                this.type11ToolStripMenuItem.Checked = false;
            }
        }
        
        private void Type11ToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (this.type10ToolStripMenuItem.Checked)
            {
                this.type10ToolStripMenuItem.Checked = false;
                this.type11ToolStripMenuItem.Checked = true;
            }
        }
        
        private void OverwriteToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.overwriteToolStripMenuItem.Checked = !this.overwriteToolStripMenuItem.Checked;
        }
        
        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            this.aboutForm.ShowDialog();
        }
        #endregion
        
        #region File Management
        private void DecompressTree(string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (Directory.Exists(fileName))
                {
                    this.DecompressTree(Directory.GetFiles(fileName));
                }
                else if (File.Exists(fileName))
                {
                    this.DecompressFile(fileName);
                }
            }
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private void DecompressFile(string fileName)
        {
            try
            {
                byte[] compressedData = StreamHelper.ReadFile(fileName);
                byte[] decompressedData;
                if (compressedData.Length > 0)
                {
                    this.type11ToolStripMenuItem.Checked = compressedData[0] == 0x11;
                    this.type10ToolStripMenuItem.Checked = !this.type11ToolStripMenuItem.Checked;
                }
                
                if (this.type11ToolStripMenuItem.Checked)
                {
                    decompressedData = CompressionManager.DecompressOnz(compressedData);
                }
                else
                {
                    decompressedData = CompressionManager.Decompress(compressedData);
                }
                
                if (decompressedData == null)
                {
                    throw new FormatException("Not a compressed file!");
                }
                
                string outFileName = fileName;
                if (!this.overwriteToolStripMenuItem.Checked)
                {
                    outFileName += ".decompressed";
                }
                
                StreamHelper.WriteFile(outFileName, decompressedData);
            }
            catch (SystemException exc)
            {
                this.errors.Add(string.Format(CultureInfo.CurrentCulture, "[{0}]: {1}", fileName, exc.Message));
            }
        }
        
        private void CompressTree(string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                if (Directory.Exists(fileName))
                {
                    this.CompressTree(Directory.GetFiles(fileName));
                }
                else if (File.Exists(fileName))
                {
                    this.CompressFile(fileName);
                }
            }
        }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Redirecting to log")]
        private void CompressFile(string fileName)
        {
            try
            {
                byte[] uncompressedData = StreamHelper.ReadFile(fileName);
                byte[] compressedData;
                if (this.type11ToolStripMenuItem.Checked)
                {
                    compressedData = CompressionManager.CompressOnz(uncompressedData);
                }
                else
                {
                    compressedData = CompressionManager.CompressLzss(uncompressedData);
                }
                
                string outFileName = fileName;
                if (!this.overwriteToolStripMenuItem.Checked)
                {
                    outFileName += ".compressed";
                }
                
                StreamHelper.WriteFile(outFileName, compressedData);
            }
            catch (SystemException exc)
            {
                this.errors.Add(string.Format(CultureInfo.CurrentCulture, "[{0}]: {1}", fileName, exc.Message));
            }
        }
        #endregion
        
        #region Error Handling
        private void ClearErrors()
        {
            this.errors.Clear();
        }
        
        private void ShowErrors()
        {
            if (this.errors.Count == 0)
            {
                return;
            }
            
            FileStream errorFile = new FileStream("errors.txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(errorFile);
            writer.WriteLine("Not all files were processed successfully.");
            writer.WriteLine();
            foreach (string error in this.errors)
            {
                writer.WriteLine(error);
            }
            
            writer.Close();
            System.Diagnostics.Process.Start(errorFile.Name);
        }
        #endregion
    }
}
