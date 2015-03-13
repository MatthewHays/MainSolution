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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using MahApps.Metro.Controls;

namespace WpfMovieDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MovieViewModel _viewModel = null;
        private SortAdorner _CurAdorner = null;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = this.DataContext as MovieViewModel;

            _listView.Items.SortDescriptions.Add(new SortDescription("FileDate", ListSortDirection.Descending));
            _listView.SelectedIndex = 0; 
        }

        
        private void SortClick(object sender, RoutedEventArgs e)
        {
            ListBox item = sender as ListBox;

            GridViewColumnHeader column =
              e.OriginalSource as GridViewColumnHeader;

            if (column == null)
                return;

            
            ListSortDirection newDir = ListSortDirection.Ascending;

            if (column.Tag != null && ((ListSortDirection)column.Tag) == newDir)
                newDir = ListSortDirection.Descending;
            column.Tag = newDir;

            item.Items.SortDescriptions.Clear();
            var a = column.Column.DisplayMemberBinding;
            var c = (a as Binding).Path.Path;
            item.Items.SortDescriptions.Add(new SortDescription(c, newDir));

            if (_CurAdorner != null)
                AdornerLayer.GetAdornerLayer(column).Remove(_CurAdorner);
            _CurAdorner = new SortAdorner(column, newDir);
            AdornerLayer.GetAdornerLayer(column).Add(_CurAdorner);
        }


        /// <summary>
        /// User has right clicked on a movie, create a custom context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ListBox lv = ((ListBox)e.Source);

            FileMovie movie = ((FileMovie)lv.SelectedItem);

            foreach (MenuItem mi in lv.ContextMenu.Items)
            {
                mi.Tag = movie;
            }
        }


        /// <summary>
        /// A movie was double clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lv = ((ListBox)e.Source);
            FileMovie movie = ((FileMovie)lv.SelectedItem);

            Process proc = new Process();
            proc.StartInfo.FileName = movie.Directory + "\\" + movie.FileName;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
        }


        private void UpdateAllRatings_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.UpdateAllRatings();
           
            foreach (FileMovie movie in _viewModel.FileMovies)
            {
                movie.Refresh();
            }
        }


        private void LookupItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            FileMovie fileMovie = (item.Tag as FileMovie);

            Movie movie = Helper.GetMovie(fileMovie.MovieName, fileMovie.Year);
            if (movie == null)
            {
                this.Cursor = Cursors.Arrow;
                MessageBox.Show("No movie data found");
                return;
            }
            //force the movie names to be the same
            movie.MovieName = fileMovie.MovieName;
            movie.DateDownloaded = DateTime.Now;
            
            _viewModel.AddMovie(movie);

            if (movie.Year != fileMovie.FileYear)
                MessageBox.Show("File year different, rename to " + movie.Year);

            fileMovie.Refresh();
        }

        private void LookupAllItem_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            _viewModel.LookupAllMovies();

            foreach (FileMovie movie in _viewModel.FileMovies)
            {
                movie.Refresh();
            }

            this.Cursor = Cursors.Arrow;
        }


        private void AlternateLookupItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            FileMovie tableMovie = (item.Tag as FileMovie);

            EditWindow editFileName = new EditWindow(tableMovie.MovieName);
            if (editFileName.ShowDialog() != true)
                return;

            this.Cursor = Cursors.Wait;
            //search using the new name
            Movie movie = Helper.GetMovie(editFileName.Text, tableMovie.Year);
            if (movie == null)
            {
                this.Cursor = Cursors.Arrow;
                MessageBox.Show("No movie data found");
                return;
            }
            //force the movie names to be the same
            movie.MovieName = tableMovie.MovieName;
            movie.DateDownloaded = DateTime.Now;

            _viewModel.AddMovie(movie);

            tableMovie.Refresh();

            this.Cursor = Cursors.Arrow;
        }


        private void RemoveDataItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            FileMovie tableMovie = item.Tag as FileMovie;

            _viewModel.RemoveMovie(tableMovie.Movie);

            tableMovie.Refresh();
        }


        private void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            FileMovie tableMovie = item.Tag as FileMovie;

            EditWindow editFileName = new EditWindow(tableMovie.FileName);
            if (editFileName.ShowDialog() != true)
                return;

            _viewModel.UpdateMovieName(tableMovie, editFileName.Text);

            tableMovie.Refresh();
        }

        private void ChangeFileDate_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            FileMovie tableMovie = item.Tag as FileMovie;

            _viewModel.UpdateFileDate(tableMovie);
        }
        

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            FileMovie tableMovie = item.Tag as FileMovie;

            Movie movie = FileMovie.Movies[tableMovie.MovieName];
            EditMovieWindow window = new EditMovieWindow(movie.MovieName, movie.Year);
            if (window.ShowDialog() == true)
            {
                movie.Year = window.Year;
                tableMovie.Refresh();
            }

            tableMovie.Refresh();
        }


        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            FileMovie tableMovie = item.Tag as FileMovie;

            if (MessageBox.Show(string.Format("Are you sure you want to delete {0}", tableMovie.FileName), "File deletion") == MessageBoxResult.OK)
            {
                File.Delete(tableMovie.Directory + @"\" + tableMovie.FileName);
                _viewModel.RemoveFileMovie(tableMovie);
            }

            tableMovie.Refresh();
        }


        /// <summary>
        /// Save the in mempory movie database to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            _viewModel.SaveMovieFile();
            this.Cursor = Cursors.Arrow;
        }

        

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(_viewModel.FileMovies);
            if (view == null)
                return;

            string filterString = ((TextBox)e.Source).Text.ToUpper();

            view.Filter = i =>
            {
                return (((FileMovie)i).MovieName.ToUpper().Contains(filterString) ||
                        ((FileMovie)i).Genres.ToUpper().Contains(filterString)    ||
                        ((FileMovie)i).Cast.ToUpper().Contains(filterString));
            };
            
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = new MovieViewModel();
            _viewModel = this.DataContext as MovieViewModel;
        }


    }
}
