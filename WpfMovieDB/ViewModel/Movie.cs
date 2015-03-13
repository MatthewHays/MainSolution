using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WpfMovieDB
{
    public class Movie
    {
        private List<string> _genres;

        public string MovieName        { get; set; }
        public int    Year             { get; set; }
        public double OMDBRating       { get; set; }
        public double IMDBRating       { get; set; }
        public double MetacriticRating { get; set; }
        public double NetflixRating    { get; set; }
        public string Description      { get; set; }
        public List<string> Genres
        {
            get { return _genres; }
            set { _genres = Helper.NormaliseGenres(value); }
        }
        public List<string> Cast            { get; set; }
        public bool         Watched         { get; set; }
        public bool         JenWantsToWatch { get; set; }
        public string       CoverArt        { get; set; }
        public DateTime     DateDownloaded  { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string ThumbNail { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string CoverArtConverted
        {
            get 
            { 
                return Helper.GetImage(MovieName + "." + Year, CoverArt); 
            }
            set { }
        }
    }
}
