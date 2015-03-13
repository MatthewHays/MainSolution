using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.Threading;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Data;
using System.ComponentModel;
using Newtonsoft.Json;

namespace WPFMovieViewer
{
    class MovieViewModel : ViewModelBase
    {
        public static string server = ConfigurationManager.AppSettings["MovieDir"];
        public static string xmlFile = ConfigurationManager.AppSettings["XMLFile"];
        public static string htmlFile = ConfigurationManager.AppSettings["HTMLFile"];
        public static string moviedataDir = ConfigurationManager.AppSettings["MovieDataDir"];
        private const string regex = @"^(?<moviename>[a-zA-Z0-9\.\,\'\-\&\!\s]+?)(?<year>\(\d{4}\))?(?<HD>\sHD)?(\sAC3)?(?<part>\s\d-\d)?((.avi$)|(.mkv$)|(.mp4$)|(.m4v$))";


        public ThreadedObservableCollection<Movie> Movies 
        {
            get { return _movies; }
            set { _movies = value; }
        }

        private ThreadedObservableCollection<Movie> _movies = new ThreadedObservableCollection<Movie>();
        private Thread _getFiles;

        public MovieViewModel()
        {
            
            if (isDesignMode)
            {
                Movie movie = new Movie();
                movie.MovieName = "TestMovie";
                movie.Year = 2010;
                movie.JenWantsToWatch = true;
                movie.Watched = true;
                movie.IMDBRating = 1.2;
                movie.NetflixRating = 1.1;
                movie.Description = "Test Description";

                Movies.Add(movie);

                Movie movie2 = new Movie();
                movie2.MovieName = "TestMovie2";
                movie2.Year = 2011;
                movie2.JenWantsToWatch = true;
                movie2.Watched = true;
                movie2.IMDBRating = 1.3;
                movie2.NetflixRating = 1.2;
                movie2.Description = "Test Description2";

                Movies.Add(movie2);
            }
            else
            {
                LoadMovieFile(xmlFile);
            }
        }

        private string _searchString = string.Empty;
        public string SearchString
        {
            get { return _searchString; }
            set 
            { 
                _searchString = value;

                ICollectionView view = CollectionViewSource.GetDefaultView(Movies);
                if (view == null)
                    return;

                view.Filter = i =>
                {
                    return ( ((Movie)i).MovieName.ToUpper().Contains(_searchString.ToUpper()) ||
                             ((Movie)i).GenresFormatted.ToUpper().Contains(_searchString.ToUpper()) ||
                             ((Movie)i).CastFormatted.ToUpper().Contains(_searchString.ToUpper()));
                };
            }
        }

        /// <summary>
        /// Load the movie database file and populate the movie datasource 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool LoadMovieFile(string file)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;

            try
            {
                string jsonFile = file + ".json";
                if (File.Exists(jsonFile))
                {
                    FileStream movieFile = File.OpenRead(jsonFile);
                    StreamReader sr = new StreamReader(movieFile);

                    var movies = JsonConvert.DeserializeObject<List<Movie>>(sr.ReadToEnd());

                    foreach (Movie movie in movies)
                    {
                        _movies.Add(movie);
                    }

                    movieFile.Close();
                    movieFile = null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return false;
        }

    } //class
} //namespace
