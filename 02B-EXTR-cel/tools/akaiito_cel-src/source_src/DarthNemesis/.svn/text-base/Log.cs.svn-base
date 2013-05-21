//-----------------------------------------------------------------------
// <copyright file="Log.cs" company="DarthNemesis">
// Copyright (c) 2009 All Right Reserved
// </copyright>
// <author>DarthNemesis</author>
// <date>2009-12-21</date>
// <summary>Methods for writing messages to a RichTextBox.</summary>
//-----------------------------------------------------------------------

namespace DarthNemesis
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    
    /// <summary>
    /// Methods for writing messages to a RichTextBox.
    /// </summary>
    public class Log
    {
        private RichTextBox textBox;
        private Level logLevel;
        
        /// <summary>
        /// Initializes a new instance of the Log class.
        /// </summary>
        /// <param name="textBox">The RichTextBox where the messages should be written.</param>
        /// <param name="logLevel">The initial log level, from 0 (Debug) to 3 (Error).</param>
        public Log(RichTextBox textBox, int logLevel)
        {
            this.textBox = textBox;
            this.logLevel = (Level)logLevel;
        }
        
        /// <summary>
        /// Initializes a new instance of the Log class.
        /// Log level defaults to Debug.
        /// </summary>
        /// <param name="textBox">The RichTextBox where the messages should be written.</param>
        public Log(RichTextBox textBox) : this(textBox, 0)
        {
        }
        
        private enum Level
        {
            Debug = 0,
            Info = 1,
            Warn = 2,
            Error = 3
        }
        
        /// <summary>
        /// Changes the current log level.
        /// </summary>
        /// <param name="logLevel">The new log level, from 0 (Debug) to 3 (Error).</param>
        public void SetLogLevel(int logLevel)
        {
            this.logLevel = (Level)logLevel;
        }
        
        /// <summary>
        /// Erases the contents of the text box.
        /// </summary>
        public void Clear()
        {
            this.textBox.Text = string.Empty;
        }
        
        /// <summary>
        /// Writes the message to the text box if the log level is Debug.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Debug(string message)
        {
            if (Level.Debug >= this.logLevel)
            {
                this.Write(message, Color.Gray);
            }
        }
        
        /// <summary>
        /// Writes the filename-prefixed message to the text box if the log level is Debug.
        /// </summary>
        /// <param name="fileName">The filename to prepend to the message.</param>
        /// <param name="message">The message to be logged.</param>
        public void Debug(string fileName, string message)
        {
            this.Debug(string.Format(CultureInfo.CurrentCulture, "[{0}] {1}", fileName, message));
        }
        
        /// <summary>
        /// Writes the message to the text box if the log level is Info or lower.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Info(string message)
        {
            if (Level.Info >= this.logLevel)
            {
                this.Write(message, Color.Black);
            }
        }
        
        /// <summary>
        /// Writes the filename-prefixed message to the text box if the log level is Info or lower.
        /// </summary>
        /// <param name="fileName">The filename to prepend to the message.</param>
        /// <param name="message">The message to be logged.</param>
        public void Info(string fileName, string message)
        {
            this.Info(string.Format(CultureInfo.CurrentCulture, "[{0}] {1}", fileName, message));
        }
        
        /// <summary>
        /// Writes the message to the text box if the log level is Error or lower.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Error(string message)
        {
            if (Level.Error >= this.logLevel)
            {
                this.Write(message, Color.Red);
            }
        }
        
        /// <summary>
        /// Writes the filename-prefixed message to the text box if the log level is Error or lower.
        /// </summary>
        /// <param name="fileName">The filename to prepend to the message.</param>
        /// <param name="message">The message to be logged.</param>
        public void Error(string fileName, string message)
        {
            this.Error(string.Format(CultureInfo.CurrentCulture, "[{0}] {1}", fileName, message));
        }
        
        private void Write(string message, Color color)
        {
            message += System.Environment.NewLine;
            this.textBox.SelectionStart = this.textBox.Text.Length;
            this.textBox.SelectionColor = color;
            this.textBox.SelectedText = message;
            this.textBox.Select(this.textBox.Text.Length, 0);
            this.textBox.ScrollToCaret();
        }
    }
}
