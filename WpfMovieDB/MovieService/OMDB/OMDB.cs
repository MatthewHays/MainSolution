using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.IO;

namespace WpfMovieDB
{
    class OMDB 
    {
        //private static string search = @"http://api.themoviedb.org/2.0/Movie.search?title={0}&api_key={1}";
        //private static string getInfo = @"http://api.themoviedb.org/2.0/Movie.getInfo?id={0}&api_key={1}";
        private static readonly string ApiKey = "f6bd687ffa63cd282b6ff2c6877f2669";

        private static string search = @"http://api.themoviedb.org/2.1/Movie.search/en/xml/" + ApiKey + @"/{0}";
        private static string getInfo = @"http://api.themoviedb.org/2.1/Movie.getInfo/en/xml/" + ApiKey + @"/{0}";

        /// <summary>
        /// Return a given movie from TMDB
        /// </summary>
        /// <param name="movieId"></param>
        /// <returns></returns>
        private static Movie GetMovie(string movieId)
        {
            string url = string.Format(getInfo, movieId/*, ApiKey*/);
            XmlDocument doc = Helper.FetchXML(url);
            if (doc != null)
            {
                Movie movie = new Movie();

                XmlNode movieNode = doc.SelectSingleNode("//OpenSearchDescription/movies/movie");

                movie.MovieName = movieNode.SelectSingleNode("//name").InnerText;
                movie.Description = movieNode.SelectSingleNode("//overview").InnerText;
                movie.OMDBRating  = Math.Round(double.Parse(movieNode.SelectSingleNode("//rating").InnerText),1);
                string release = movieNode.SelectSingleNode("//released").InnerText;
                if (!string.IsNullOrEmpty(release))
                    movie.Year = int.Parse(release.Substring(0, 4));

                movie.Cast = new List<string>();
                foreach (XmlNode n in movieNode.SelectNodes("//cast/person[@job='Actor']"))
                {
                    string name = n.Attributes["name"].InnerText;
                    if (!string.IsNullOrEmpty(name) && !movie.Cast.Contains(name))
                        movie.Cast.Add(name);
                }

                XmlNode xmlNode = movieNode.SelectSingleNode("//images/image[@type='poster' and @size='mid']");
                if (xmlNode == null)
                    xmlNode = movieNode.SelectSingleNode("//images/image[@type='poster' and @size='original']");
                if (xmlNode == null)
                    xmlNode = movieNode.SelectSingleNode("//images/image[@type='backdrop' and @size='original']");
                string img = xmlNode == null ? string.Empty : xmlNode.Attributes["url"].InnerText;
                if (!string.IsNullOrEmpty(img))
                    movie.CoverArt = img;

                xmlNode = movieNode.SelectSingleNode("//images/image[@type='poster' and @size='thumb']");
                if (xmlNode == null)
                    xmlNode = movieNode.SelectSingleNode("//images/image[@type='backdrop' and @size='thumb']");
                img = xmlNode == null ? string.Empty : xmlNode.Attributes["url"].InnerText;
                if (!string.IsNullOrEmpty(img))
                    movie.ThumbNail = img;

                XmlNodeList nodes = movieNode.SelectNodes("//categories/category[@type='genre']");
                movie.Genres = new List<string>();
                foreach (XmlNode node in nodes)
                {
                    string n = ConvertGenre(node.Attributes["name"].InnerText);
                    if (!string.IsNullOrEmpty(n) && !movie.Genres.Contains(n))
                        movie.Genres.Add(n);
                }

                //movie.Cast.Sort();
                movie.Genres.Sort();
                return movie;
            }
            return null;
        }

        /// <summary>
        /// Clean up a genre from TMDB
        /// </summary>
        /// <param name="genre"></param>
        /// <returns></returns>
        private static string ConvertGenre(string genre)
        {
            genre  = genre.Replace(" Film",string.Empty);
            genre = genre.Trim();

            if (genre == "Action") return "Action";
            else if (genre == "Adventure") return "Adventure";
            else if (genre == "Animation") return "Animation";
            else if (genre == "Comedy") return "Comedy";
            else if (genre == "Crime") return "Crime";
            else if (genre == "Disaster") return "Disaster";
            else if (genre == "Documentary") return "Documentary";
            else if (genre == "Drama") return "Drama";
            else if (genre == "Eastern") return "Eastern";
            else if (genre == "Environmental") return "Environmental";
            else if (genre == "Erotic") return "Erotic";
            else if (genre == "Fantasy") return "Family Fantasy";
            else if (genre == "Historical") return "History";
            else if (genre == "Horror") return "Horror";
            else if (genre == "Musical") return "Musical";
            else if (genre == "Mystery") return "Mystery";
            else if (genre == "Road") return "Road Movie";
            else if (genre == "Science Fiction") return "Sci-Fi";
            else if (genre == "Thriller") return "Thriller";
            else if (genre == "Western") return "Western ";
            else genre = string.Empty;

            return genre;
        }

        /// <summary>
        /// Return all potential matches for a given movie
        /// </summary>
        /// <param name="movieName"></param>
        /// <returns></returns>
        public static List<Movie> GetMovies(string movieName)
        {

            //get the list of matches from omdb
            List<Movie> omdb = new List<Movie>();
            
            
            string url = string.Format(search, movieName);
            url = Uri.EscapeUriString(url);

            XmlDocument doc = Helper.FetchXML(url);
            if (doc != null)
            {
                XmlNodeList movieNodes = doc.SelectNodes("/OpenSearchDescription/movies/movie");
                foreach (XmlNode movieNode in movieNodes)
                {
                    try
                    {
                        string id = movieNode.SelectSingleNode("id").InnerText;
                        string imdb = movieNode.SelectSingleNode("imdb_id").InnerText;
                        Movie movie = GetMovie(id);

                        //little bit hacky, but I want to use IMDB's rating
                        if (!string.IsNullOrWhiteSpace(imdb))
                        {
                            Tuple<double, double> ratings = IMDB.GetIMDBRatings(imdb);

                            movie.IMDBRating = ratings.Item1;
                            movie.MetacriticRating = ratings.Item2;
                        }

                        if (movie!= null)
                            omdb.Add(movie);
                    }
                    catch (Exception)
                    {
                        //do nothing
                    }
                }
            }
            return omdb;
        }
    } //class
} //namespace


