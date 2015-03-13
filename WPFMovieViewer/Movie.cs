using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WPFMovieViewer
{
    public class Movie
    {
        private List<string> _genres;

        public string MovieName { get; set; }
        public int Year { get; set; }
        public double OMDBRating { get; set; }
        public double IMDBRating { get; set; }
        public double MetacriticRating { get; set; }
        public double NetflixRating { get; set; }
        public string Description { get; set; }
        public List<string> Genres
        {
            get { return _genres; }
            set { _genres = Helper.NormaliseGenres(value); }
        }
        public List<string> Cast { get; set; }
        public bool Watched { get; set; }
        public bool JenWantsToWatch { get; set; }
        public string CoverArt { get; set; }
        public DateTime DateDownloaded { get; set; }
        [XmlIgnore]
        public string ThumbNail { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public string CoverArtConverted
        {
            get
            {
                return Helper.GetImage(MovieName + "." + Year, CoverArt);
            }
            set { }
        }
        
        [XmlIgnore]
        [JsonIgnore]
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

        [XmlIgnore]
        public string GenresFormatted
        {
            set { }
            get { return Genres.Count == 0 ? "<N/A>" : string.Join(", ", Genres.Take(2)); }
        }

        [XmlIgnore]
        public string CastFormatted
        {
            set { }
            get { return Cast.Count == 0 ? "<N/A>" : string.Join(", ", Cast.Take(5)); }
        }
    }
}
