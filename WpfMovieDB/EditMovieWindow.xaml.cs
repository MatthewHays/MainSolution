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
using System.ComponentModel;
using MahApps.Metro.Controls;

namespace WpfMovieDB
{
    /// <summary>
    /// Interaction logic for EditMovieWindow.xaml
    /// </summary>
    public partial class EditMovieWindow : MetroWindow, INotifyPropertyChanged
    {
        string _movieName;
        public String MovieName 
        {
            get { return _movieName; }
            set { _movieName = value; OnPropertyChanged("MovieName"); }
        }

        int _year;
        public int Year 
        {
            get { return _year; }
            set { _year = value; OnPropertyChanged("Year"); }
        }

        public EditMovieWindow(string movieName, int year)
        {
            InitializeComponent();
            MovieName = movieName;
            Year = year;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
