using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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
            name = name.Replace("[eng]", "");
            name = name.Replace("-axxo", "");
            name = name.Replace("-lol", "");
            name = name.Replace("-killers", "");
            name = name.Replace("ettv", "");
            name = name.Replace("webrip", "");
            name = name.Replace("dvdrip", "");
            name = name.Replace("xvid", "");
            name = name.Replace("bdrip", "");
            name = name.Replace("720p", "");
            name = name.Replace("hdtv", "");
            name = name.Replace("x264", "");
            name = name.Replace("h264", "");
            name = name.Replace("blueray", "");
            name = name.Replace("series", "S");
            name = name.Replace("episode", "E");

            name = name.Replace(".", " ");
            name = name.Replace("[", "(");
            name = name.Replace("]", ")");
            name = name.Replace("cd1", "1-2");
            name = name.Replace("cd2", "2-2");

            name = name.Replace("  ", " ");

            name = name.Replace("(", "");
            name = name.Replace(")", "");

            name = name.Replace("2000", "(2000)");
            name = name.Replace("2001", "(2001)");
            name = name.Replace("2002", "(2002)");
            name = name.Replace("2003", "(2003)");
            name = name.Replace("2004", "(2004)");
            name = name.Replace("2005", "(2005)");
            name = name.Replace("2006", "(2006)");
            name = name.Replace("2007", "(2007)");
            name = name.Replace("2008", "(2008)");
            name = name.Replace("2009", "(2009)");
            name = name.Replace("2010", "(2010)");
            name = name.Replace("2011", "(2011)");
            name = name.Replace("2012", "(2012)");
            name = name.Replace("2013", "(2013)");
            name = name.Replace("2014", "(2014)");
            name = name.Replace("2015", "(2015)");
            
            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);

            return name.Trim();
        }
    }
}
