using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            listView1.KeyDown += new KeyEventHandler(listView1_KeyDown);
            //listView1.ContextMenu = new ContextMenu();
            //listView1.ContextMenu.MenuItems.Add("Deleted", new EventHandler(Delete));
        }

        void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    DirectoryInfo info = new DirectoryInfo(item.Text);

                    if (MessageBox.Show("Delete " + info.FullName) == DialogResult.OK)
                    {
                        info.Delete(true);
                        listView1.Items.Remove(item);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                DirectoryInfo info = new DirectoryInfo(dialog.SelectedPath);

                DirectoryInfo[] children = info.GetDirectories();

                foreach (DirectoryInfo child in children)
                {
                    if (IsEmptyFolder(child))
                    {
                        listView1.Items.Add(child.FullName.ToString());
                    }
                }
            }
        }

        public void Delete(object sender, EventArgs eventArgs)
        {
            ListViewItem item = listView1.SelectedItems[0];

            DirectoryInfo info = new DirectoryInfo(item.Text);

            if (MessageBox.Show("Delete " + info.FullName) == DialogResult.OK) 
                info.Delete(true);
        }

        protected override void  OnKeyDown(KeyEventArgs e)
        {
            /*if (e.KeyCode == Keys.Delete)
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    DirectoryInfo info = new DirectoryInfo(item.Text);

                    if (MessageBox.Show("Delete " + info.FullName) == DialogResult.OK)
                    {
                        info.Delete(true);
                        listView1.Items.Remove(item);
                    }
                }
            }*/
            
        }
        public bool IsEmptyFolder(DirectoryInfo parent)
        {
            if (parent.GetFiles("*.mp3").Length > 0 || parent.GetFiles("*.m4p").Length > 0)
                return false;

            foreach (DirectoryInfo child in parent.GetDirectories())
            {
                if (!IsEmptyFolder(child))
                    return false;
            }

            return true ;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = listView1.SelectedItems[0];

            
            Process.Start("explorer.exe", item.Text); 

        }

    }
}
