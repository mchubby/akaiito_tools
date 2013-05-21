//-----------------------------------------------------------------------
// <copyright file="MainForm.Designer.cs" company="DarthNemesis">
// Copyright (c) 2010 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2010-02-22</date>
// <summary>Primary application form.</summary>
//-----------------------------------------------------------------------
namespace BatchLZ77
{
    partial class MainForm
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
        	this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
        	this.groupBoxDecompress = new System.Windows.Forms.GroupBox();
        	this.panelDecompress = new System.Windows.Forms.Panel();
        	this.groupBoxCompress = new System.Windows.Forms.GroupBox();
        	this.panelCompress = new System.Windows.Forms.Panel();
        	this.menuStrip1 = new System.Windows.Forms.MenuStrip();
        	this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.decompressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.compressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
        	this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.type10ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.type11ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
        	this.overwriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        	this.groupBoxDecompress.SuspendLayout();
        	this.groupBoxCompress.SuspendLayout();
        	this.menuStrip1.SuspendLayout();
        	this.SuspendLayout();
        	// 
        	// openFileDialog1
        	// 
        	this.openFileDialog1.Filter = "All files|*.*";
        	this.openFileDialog1.Multiselect = true;
        	this.openFileDialog1.RestoreDirectory = true;
        	// 
        	// groupBoxDecompress
        	// 
        	this.groupBoxDecompress.Controls.Add(this.panelDecompress);
        	this.groupBoxDecompress.Location = new System.Drawing.Point(12, 27);
        	this.groupBoxDecompress.Name = "groupBoxDecompress";
        	this.groupBoxDecompress.Size = new System.Drawing.Size(100, 100);
        	this.groupBoxDecompress.TabIndex = 3;
        	this.groupBoxDecompress.TabStop = false;
        	this.groupBoxDecompress.Text = "Decompress";
        	// 
        	// panelDecompress
        	// 
        	this.panelDecompress.AllowDrop = true;
        	this.panelDecompress.Location = new System.Drawing.Point(6, 12);
        	this.panelDecompress.Name = "panelDecompress";
        	this.panelDecompress.Size = new System.Drawing.Size(88, 82);
        	this.panelDecompress.TabIndex = 0;
        	this.panelDecompress.DragDrop += new System.Windows.Forms.DragEventHandler(this.PanelDecompressDragDrop);
        	this.panelDecompress.DragEnter += new System.Windows.Forms.DragEventHandler(this.PanelDecompressDragEnter);
        	// 
        	// groupBoxCompress
        	// 
        	this.groupBoxCompress.Controls.Add(this.panelCompress);
        	this.groupBoxCompress.Location = new System.Drawing.Point(118, 27);
        	this.groupBoxCompress.Name = "groupBoxCompress";
        	this.groupBoxCompress.Size = new System.Drawing.Size(100, 100);
        	this.groupBoxCompress.TabIndex = 4;
        	this.groupBoxCompress.TabStop = false;
        	this.groupBoxCompress.Text = "Compress";
        	// 
        	// panelCompress
        	// 
        	this.panelCompress.AllowDrop = true;
        	this.panelCompress.Location = new System.Drawing.Point(6, 12);
        	this.panelCompress.Name = "panelCompress";
        	this.panelCompress.Size = new System.Drawing.Size(88, 82);
        	this.panelCompress.TabIndex = 0;
        	this.panelCompress.DragDrop += new System.Windows.Forms.DragEventHandler(this.PanelCompressDragDrop);
        	this.panelCompress.DragEnter += new System.Windows.Forms.DragEventHandler(this.PanelCompressDragEnter);
        	// 
        	// menuStrip1
        	// 
        	this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.fileToolStripMenuItem,
        	        	        	this.optionsToolStripMenuItem,
        	        	        	this.helpToolStripMenuItem});
        	this.menuStrip1.Location = new System.Drawing.Point(0, 0);
        	this.menuStrip1.Name = "menuStrip1";
        	this.menuStrip1.Size = new System.Drawing.Size(230, 24);
        	this.menuStrip1.TabIndex = 5;
        	this.menuStrip1.Text = "menuStrip1";
        	// 
        	// fileToolStripMenuItem
        	// 
        	this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.decompressToolStripMenuItem,
        	        	        	this.compressToolStripMenuItem,
        	        	        	this.toolStripMenuItem1,
        	        	        	this.exitToolStripMenuItem});
        	this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        	this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
        	this.fileToolStripMenuItem.Text = "&File";
        	// 
        	// decompressToolStripMenuItem
        	// 
        	this.decompressToolStripMenuItem.Name = "decompressToolStripMenuItem";
        	this.decompressToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
        	this.decompressToolStripMenuItem.Text = "&Decompress Files...";
        	this.decompressToolStripMenuItem.Click += new System.EventHandler(this.DecompressToolStripMenuItemClick);
        	// 
        	// compressToolStripMenuItem
        	// 
        	this.compressToolStripMenuItem.Name = "compressToolStripMenuItem";
        	this.compressToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
        	this.compressToolStripMenuItem.Text = "&Compress Files...";
        	this.compressToolStripMenuItem.Click += new System.EventHandler(this.CompressToolStripMenuItemClick);
        	// 
        	// toolStripMenuItem1
        	// 
        	this.toolStripMenuItem1.Name = "toolStripMenuItem1";
        	this.toolStripMenuItem1.Size = new System.Drawing.Size(165, 6);
        	// 
        	// exitToolStripMenuItem
        	// 
        	this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        	this.exitToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
        	this.exitToolStripMenuItem.Text = "E&xit";
        	this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItemClick);
        	// 
        	// optionsToolStripMenuItem
        	// 
        	this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.type10ToolStripMenuItem,
        	        	        	this.type11ToolStripMenuItem,
        	        	        	this.toolStripMenuItem2,
        	        	        	this.overwriteToolStripMenuItem});
        	this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
        	this.optionsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
        	this.optionsToolStripMenuItem.Text = "&Options";
        	// 
        	// type10ToolStripMenuItem
        	// 
        	this.type10ToolStripMenuItem.Checked = true;
        	this.type10ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
        	this.type10ToolStripMenuItem.Name = "type10ToolStripMenuItem";
        	this.type10ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
        	this.type10ToolStripMenuItem.Text = "LZ77 Type 10";
        	this.type10ToolStripMenuItem.Click += new System.EventHandler(this.Type10ToolStripMenuItemClick);
        	// 
        	// type11ToolStripMenuItem
        	// 
        	this.type11ToolStripMenuItem.Name = "type11ToolStripMenuItem";
        	this.type11ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
        	this.type11ToolStripMenuItem.Text = "LZ77 Type 11";
        	this.type11ToolStripMenuItem.Click += new System.EventHandler(this.Type11ToolStripMenuItemClick);
        	// 
        	// toolStripMenuItem2
        	// 
        	this.toolStripMenuItem2.Name = "toolStripMenuItem2";
        	this.toolStripMenuItem2.Size = new System.Drawing.Size(149, 6);
        	// 
        	// overwriteToolStripMenuItem
        	// 
        	this.overwriteToolStripMenuItem.Name = "overwriteToolStripMenuItem";
        	this.overwriteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
        	this.overwriteToolStripMenuItem.Text = "&Overwrite files";
        	this.overwriteToolStripMenuItem.Click += new System.EventHandler(this.OverwriteToolStripMenuItemClick);
        	// 
        	// helpToolStripMenuItem
        	// 
        	this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
        	        	        	this.aboutToolStripMenuItem});
        	this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
        	this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
        	this.helpToolStripMenuItem.Text = "&Help";
        	// 
        	// aboutToolStripMenuItem
        	// 
        	this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
        	this.aboutToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
        	this.aboutToolStripMenuItem.Text = "&About";
        	this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
        	// 
        	// MainForm
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(230, 133);
        	this.Controls.Add(this.groupBoxCompress);
        	this.Controls.Add(this.groupBoxDecompress);
        	this.Controls.Add(this.menuStrip1);
        	this.MainMenuStrip = this.menuStrip1;
        	this.Name = "MainForm";
        	this.Text = "BatchLZ77";
        	this.groupBoxDecompress.ResumeLayout(false);
        	this.groupBoxCompress.ResumeLayout(false);
        	this.menuStrip1.ResumeLayout(false);
        	this.menuStrip1.PerformLayout();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem type11ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem type10ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem overwriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decompressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Panel panelCompress;
        private System.Windows.Forms.GroupBox groupBoxCompress;
        private System.Windows.Forms.Panel panelDecompress;
        private System.Windows.Forms.GroupBox groupBoxDecompress;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}
