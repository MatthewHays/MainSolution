using System;
using System.Windows.Forms;

namespace AviFileRename
{
    /// <summary>
    /// Shows the Core-cleaned filename suggestion and lets the user edit it before confirming.
    /// button3 ("Copy") re-applies the clean function so the user can reset to the suggestion.
    /// </summary>
    public partial class EditFileName : Form
    {
        private readonly Func<string, string> _clean;

        public EditFileName(string originalName, string suggestedName,
                            string directory, string baseDirectory,
                            Func<string, string> clean)
        {
            InitializeComponent();

            _clean = clean;

            textBox1.Text = suggestedName;
            textBox1.SelectAll();

            var relativeDir = directory.Length > baseDirectory.Length
                ? directory.Remove(0, baseDirectory.Length)
                : string.Empty;

            label1.Text = string.IsNullOrEmpty(relativeDir)
                ? originalName
                : relativeDir + @"\" + originalName;
        }

        public string FileName => textBox1.Text.Trim();

        // "Copy" button — re-applies the clean pipeline to the label text
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = _clean(label1.Text);
        }
    }
}
