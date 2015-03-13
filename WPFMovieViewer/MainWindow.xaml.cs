using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFMovieViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ListBox.Items.SortDescriptions.Add(new SortDescription("DateDownloaded", ListSortDirection.Descending));
        }

        public void Sort_Click(object sender, EventArgs args)
        {
            var b = sender as Button;
            string property = b.Tag as String;

            if (ListBox.Items.SortDescriptions[0].PropertyName == property)
            {
                var s = new SortDescription(property, ListBox.Items.SortDescriptions[0].Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                ListBox.Items.SortDescriptions.Clear();
                ListBox.Items.SortDescriptions.Add(s);
            }
            else
            {
                ListBox.Items.SortDescriptions.Clear();
                ListBox.Items.SortDescriptions.Add(new SortDescription((b.Tag as string), ListSortDirection.Descending));
            }

        }
    }
}
