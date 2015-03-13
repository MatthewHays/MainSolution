using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace WpfMovieDB
{
    
    
    public class FileMovie : ViewModelBase
    {
        public static Dictionary<String, Movie> Movies;

        public bool     DataDownloaded { set { } get { return Movies.ContainsKey(MovieName + "." + FileYear); } }
        public string   FileName       { get; set; }
        public string   Directory      { get; set; }
        public string   MovieName      { get; set; }
        public int      FileYear       { get; set; }
        public string   Part           { set; get; }
        public string   HD             { set; get; }
        public DateTime FileDate       { get; set; }

        private Movie _movie;
        
        public Movie Movie
        {
            get 
            { 
                if (_movie == null)
                    if (DataDownloaded)
                        _movie = Movies[MovieName + "." + FileYear];
                return _movie;
            }
            set { _movie = value; }
        }

        public int Year
        {
            set { }
            get { return DataDownloaded ? Movie.Year : FileYear; }
        }

        public double OMDBRating 
        {
            set { }
            get { return DataDownloaded ? Movie.OMDBRating : 0; }
        }
        
        public double IMDBRating 
        {
            set { }
            get { return DataDownloaded ? Movie.IMDBRating : 0; }
        }

        public double MetacriticRating
        {
            set { }
            get { return DataDownloaded ? Movie.MetacriticRating : 0; }
        }

        public double NetflixRating
        {
            set { }
            get { return DataDownloaded ? Movie.NetflixRating : 0; }
        }

        public double AverageRating
        {
            set { }
            get 
            { 
                double count = 0;
                double total = 0;
                if (OMDBRating > 0)
                {
                    total += OMDBRating;
                    count++;
                }
                if (IMDBRating > 0)
                {
                    total += IMDBRating;
                    count++;
                }
                if (MetacriticRating > 0)
                {
                    total += MetacriticRating;
                    count++;
                }
                if (NetflixRating > 0)
                {
                    total += NetflixRating;
                    count++;
                }
                return Math.Round(count > 0 ? total / count : 0, 1);
            }
        }

        public string Genres
        {
            set { }
            get { return DataDownloaded ? String.Join(", ", Movie.Genres.ToArray()) : string.Empty; }
        }

        public string Cast
        {
            set { }
            get { return DataDownloaded ? String.Join(", ", Movie.Cast.ToArray()) : string.Empty; }
        }

        public bool Watched
        {
            set { if (DataDownloaded) Movie.Watched = value; }
            get { return DataDownloaded ? Movie.Watched : false; }
        }

        public bool JenWantsToWatch
        {
            set { if (DataDownloaded) Movie.JenWantsToWatch = value; }
            get { return DataDownloaded ? Movie.JenWantsToWatch : false; }
        }

        public string CoverArtConverted
        {
            set { }
            get { return DataDownloaded ? Movie.CoverArtConverted : string.Empty; }
        }

        public string Description
        {
            set { }
            get { return DataDownloaded ? Movie.Description : string.Empty; }
        }

        public void Refresh()
        {
            OnPropertyChanged(string.Empty);
        }
    }
}
