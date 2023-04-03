using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Management;
using System.Diagnostics;

namespace AviFileRename
{
    public partial class MainForm : Form
    {
        private const string regex = @"^([a-zA-Z0-9\.\,\'\-\&\!\s]+?)(?:\(\d{4}\))?(?:\sHD)?(?:\sAC3)?(?:\s\d-\d)?(:?(:?.avi$)|(:?.mkv$)|(?:.mp4$))";

        public MainForm()
        {
            InitializeComponent();
        }

        private void rename_Click(object sender, EventArgs e)
        {
            FindReplace("avi");
            FindReplace("mkv");
            FindReplace("mp4");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = _sourceTextBox.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _sourceTextBox.Text = dialog.SelectedPath;
            }
        }

        private void _destButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = _destTextBox.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _destTextBox.Text = dialog.SelectedPath;
            }
        }

        private void FindReplace(string extension)
        {
            DirectoryInfo info = new DirectoryInfo(_sourceTextBox.Text);
            foreach (FileInfo file in info.GetFiles("*." + extension, SearchOption.AllDirectories))
            {
                try
                {
                    /*Regex reg = new Regex(regex);
                    Match match = reg.Match(file.Name);*/

                    string fileName = Path.GetFileNameWithoutExtension(file.Name);
                    if (fileName.ToLower().Contains("sample"))
                        continue;

                    EditFileName editFileName = new EditFileName(fileName, file.DirectoryName, _sourceTextBox.Text);

                    if (editFileName.ShowDialog() == DialogResult.OK)
                    {
                        string subTitles = file.DirectoryName + "\\" + fileName + ".srt";

                        if (File.Exists(subTitles))
                        {
                            FileInfo subs = new FileInfo(subTitles);

                            string subtemp = file.DirectoryName + "\\" + editFileName.FileName + ".srt";
                            subs.MoveTo(subtemp);
                        }

                        string temp = file.DirectoryName + "\\" + editFileName.FileName + "." + extension;
                        file.MoveTo(temp);
                    }
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    {
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Copy all the files from the source and all its child directories, to the source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void collapseFlatten_Click(object sender, EventArgs e)
        {
            Collapse("avi", _sourceTextBox.Text, _sourceTextBox.Text, SearchOption.AllDirectories);
            Collapse("mkv", _sourceTextBox.Text, _sourceTextBox.Text, SearchOption.AllDirectories);
            Collapse("mp4", _sourceTextBox.Text, _sourceTextBox.Text, SearchOption.AllDirectories);
            Collapse("srt", _sourceTextBox.Text, _sourceTextBox.Text, SearchOption.AllDirectories);
            
        }

        /// <summary>
        /// Move all the files to a different location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void collapseToDest_Click(object sender, EventArgs e)
        {
            Collapse("avi", _sourceTextBox.Text, _destTextBox.Text, SearchOption.TopDirectoryOnly);
            Collapse("mkv", _sourceTextBox.Text, _destTextBox.Text, SearchOption.TopDirectoryOnly);
            Collapse("mp4", _sourceTextBox.Text, _destTextBox.Text, SearchOption.TopDirectoryOnly);
            Collapse("srt", _sourceTextBox.Text, _destTextBox.Text, SearchOption.TopDirectoryOnly);
        }

        private void Collapse(string extension, string source, string destination, SearchOption searchOption)
        {
            this.Cursor = Cursors.WaitCursor;
            DirectoryInfo info = new DirectoryInfo(source);
            foreach (FileInfo file in info.GetFiles("*." + extension, searchOption))
            {
                try
                {
                    File.Move(file.FullName, destination + @"\" + file.Name);
                }
                catch (Exception )
                {
                }
            }

            this.Cursor = Cursors.Default;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var psi = new ProcessStartInfo() { FileName = _sourceTextBox.Text, UseShellExecute = true };
            Process.Start(psi);
        }
    } //class
} //namespace
