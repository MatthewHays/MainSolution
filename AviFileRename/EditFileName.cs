using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Globalization;

namespace AviFileRename
{
    public partial class EditFileName : Form
    {
        public EditFileName(string fileName, string directory, string baseDirectory)
        {
            InitializeComponent();

            textBox1.Text = Clean(fileName);
            if (textBox1.Text == fileName)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                 Load += (s, e) => Close();
                return;
            }
            
            
            textBox1.SelectAll();

            directory = directory.Remove(0, baseDirectory.Length);
            if (string.IsNullOrEmpty(directory))
                label1.Text = fileName;
            else
                label1.Text = directory + @"\" + fileName;
            
        }

        

        public string FileName
        {
            get { return textBox1.Text.Trim(); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = Clean(label1.Text);
        }

        public string Clean(string name)
        {
            name = name.ToLower();

            name = name.Replace("1080p", "");
            name = name.Replace("720p", "");
            name = name.Replace("x264", "");
            name = name.Replace("h264", "");

            name = name.Replace("[eng]", "");
            name = name.Replace("-axxo", "");
            name = name.Replace("-lol", "");
            name = name.Replace("-2hd", "");
            name = name.Replace("-fov", "");
            name = name.Replace("-bia", "");
            name = name.Replace("-fqm", "");
            name = name.Replace("-notv", "");
            name = name.Replace("-pow4", "");
            name = name.Replace("-killers", "");
            name = name.Replace("-evolve", "");
            name = name.Replace("yify", "");
            name = name.Replace("ettv", "");
            name = name.Replace("webrip", "");
            name = name.Replace("dvdrip", "");
            name = name.Replace("xvid", "");
            name = name.Replace("bdrip", "");
            name = name.Replace("hdtv", "");
            name = name.Replace("blueray", "");
            name = name.Replace("bluray", "");
            name = name.Replace("series", "S");
            name = name.Replace("season", "S");
            name = name.Replace("episode", "E");
            name = name.Replace(".", " ");
            name = name.Replace("[", "(");
            name = name.Replace("]", ")");
            name = name.Replace("cd1", "1-2");
            name = name.Replace("cd2", "2-2");
            name = name.Replace("  ", " ");

            name = Regex.Replace(name, "([0-9]{4})", @"($1)");
            name = Regex.Replace(name, " ([0-9])([0-9][0-9])", " S0$1E$2");
            name = Regex.Replace(name, " ([0-9][0-9])x([0-9][0-9])", " S$1E$2", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, " ([0-9])x([0-9][0-9])", " S0$1E$2", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, " s([0-9][0-9])e([0-9][0-9])", " S$1E$2", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, " S ([0-9])", " S0$1", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, " E ([0-9][0-9])", "E$1", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, " s([0-9][0-9])e([0-9][0-9])-", " S$1E$2 - ", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, "- s([0-9][0-9])e([0-9][0-9])", "S$1E$2", RegexOptions.IgnoreCase);
            name = name.Replace("((", "(");
            name = name.Replace("))", ")");
            name = name.Replace("()", "");
            name = name.Replace("  ", " ");

            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);

            return name.Trim();
        }
    }
}
