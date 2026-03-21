using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using AviFileRename.Core;

namespace AviFileRename
{
    public partial class MainForm : Form
    {
        private readonly FileRenameService _svc = new();

        public MainForm()
        {
            InitializeComponent();
        }

        // Rename button — per-file interactive approval using Core's Clean
        private void rename_Click(object sender, EventArgs e)
        {
            FindReplace("avi");
            FindReplace("mkv");
            FindReplace("mp4");
        }

        private void FindReplace(string extension)
        {
            var info = new DirectoryInfo(_sourceTextBox.Text);
            foreach (var file in info.GetFiles("*." + extension, SearchOption.AllDirectories))
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file.Name);
                    if (fileName.ToLower().Contains("sample"))
                        continue;

                    var suggested = _svc.Clean(fileName);

                    // Skip if nothing changed
                    if (suggested == fileName)
                        continue;

                    var dlg = new EditFileName(fileName, suggested, file.DirectoryName!, _sourceTextBox.Text, _svc.Clean);
                    if (dlg.ShowDialog() != DialogResult.OK)
                        continue;

                    var newName = dlg.FileName;

                    // Co-rename subtitle
                    var oldSub = Path.Combine(file.DirectoryName!, fileName + ".srt");
                    if (File.Exists(oldSub))
                        File.Move(oldSub, Path.Combine(file.DirectoryName!, newName + ".srt"));

                    file.MoveTo(Path.Combine(file.DirectoryName!, newName + "." + extension));
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OKCancel) != DialogResult.OK)
                        continue;
                }
            }
        }

        // Source browse
        private void button2_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = _sourceTextBox.Text };
            if (dialog.ShowDialog() == DialogResult.OK)
                _sourceTextBox.Text = dialog.SelectedPath;
        }

        // Dest browse
        private void _destButton_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = _destTextBox.Text };
            if (dialog.ShowDialog() == DialogResult.OK)
                _destTextBox.Text = dialog.SelectedPath;
        }

        // Flatten — move all video/subtitle files from subdirs into source root
        private void collapseFlatten_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                _svc.CollapseAsync(_sourceTextBox.Text, _sourceTextBox.Text,
                    SearchOption.AllDirectories,
                    new[] { "avi", "mkv", "mp4", "srt" }).GetAwaiter().GetResult();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // Collapse to dest — move top-level files to destination folder
        private void collapseToDest_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                _svc.CollapseAsync(_sourceTextBox.Text, _destTextBox.Text,
                    SearchOption.TopDirectoryOnly,
                    new[] { "avi", "mkv", "mp4", "srt" }).GetAwaiter().GetResult();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // Open source folder in Explorer
        private void button1_Click(object sender, EventArgs e)
        {
            var psi = new ProcessStartInfo { FileName = _sourceTextBox.Text, UseShellExecute = true };
            Process.Start(psi);
        }
    }
}
