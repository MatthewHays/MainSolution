using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfMovieDB
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SelectionForm : MetroWindow
    {
        public Movie SelectedMovie { get; set; }

        public List<Movie> Movies { get; set; }

        public SelectionForm(string movieName, List<Movie> movies)
        {
            Movies = new List<Movie>(movies);
            
            InitializeComponent();

            this.Title = movieName;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedMovie = e.AddedItems.Count > 0 ? e.AddedItems[0] as Movie : null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
