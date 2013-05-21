//-----------------------------------------------------------------------
// <copyright file="AboutForm.cs" company="DarthNemesis">
// Copyright (c) 2009 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2009-09-18</date>
// <summary>A dialog box containing author information.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// A dialog box containing author information.
    /// </summary>
    public partial class AboutForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the AboutForm class.
        /// Populates the form with the given game information.
        /// </summary>
        /// <param name="parent">The calling form.</param>
        /// <param name="name">The name of the program.</param>
        /// <param name="version">The program version number.</param>
        /// <param name="description">A short description of the program.</param>
        public AboutForm(Form parent, string name, string version, string description)
        {
            this.InitializeComponent();
            this.Text = "About " + name;
            this.labelTitle.Text = name;
            this.labelVersion.Text = version;
            this.labelDescription.Text = description;
            this.Icon = parent.Icon;
        }
        
        private void ButtonOKClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
