using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShellExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".pdf")]
    //[COMServerAssociation(AssociationType.AllFiles)]
    public class ServerMoviesExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            //  Create a 'count lines' item.
            var rename = new ToolStripMenuItem
            {
                Text = "Rename movie"
            };

            //  When we click, we'll count the lines.
            rename.Click += (sender, args) => RenameMethod();

            //  Add the item to the context menu.
            menu.Items.Add(rename);

            //  Return the menu.
            return menu;
        }

        /// <summary>
        /// Counts the lines in the selected files.
        /// </summary>
        private void RenameMethod()
        {
            if (SelectedItemPaths == null)
                return;

            //  Builder for the output.
            var builder = new StringBuilder();

            //  Go through each file.
            foreach (var filePath in SelectedItemPaths)
            {
                //  Count the lines.
                MessageBox.Show(filePath);
            }
        }
    }
}
