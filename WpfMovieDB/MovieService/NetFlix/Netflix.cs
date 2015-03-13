using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace WpfMovieDB
{
    class Netflix 
    {
        private static string consumerKey = "dnnh7s9gqgef86cxx2ap7eb7";
        private static string secretKey = "uPHNd6vAWe";
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string Strip(string text)
        {
            return Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="movieName"></param>
        /// <returns></returns>
        public static List<Movie> GetMovies(string movieName)
        {
            XmlDocument results = MovieSearch(movieName);

            List<Movie> list = new List<Movie>();

            foreach (XmlNode node in results.SelectNodes("//catalog_titles/catalog_title"))
            {
                Movie movie = new Movie();
                movie.MovieName     = node.SelectSingleNode("title").Attributes["regular"].InnerText;
                movie.Year          = int.Parse(node.SelectSingleNode("release_year").InnerText);
                
                double rating = 0;
                if (node.SelectSingleNode("average_rating") != null)
                    double.TryParse(node.SelectSingleNode("average_rating").InnerText, out rating);

                movie.NetflixRating = rating * 2.0;
                movie.ThumbNail     = node.SelectSingleNode("box_art").Attributes["large"].InnerText; //small
                movie.CoverArt      = node.SelectSingleNode("box_art").Attributes["large"].InnerText;
                movie.Description   = Strip(node.SelectSingleNode("link[@title='synopsis']/synopsis").InnerText);
                 
                List<string>  Cast = new List<string>();
                List<string>  Genres = new List<string>();

                try
                {
                    foreach (XmlNode person in node.SelectNodes("link[@title='cast']/people/link"))
                    {
                        Cast.Add(person.Attributes["title"].InnerText);
                    }
                }
                catch (Exception /*e*/)
                {
                }

                try
                {
                    foreach (XmlNode genre in node.SelectNodes("//category[@scheme='http://api-public.netflix.com/categories/genres']"))
                    {
                        string genreText = genre.Attributes["label"].InnerText;
                        Genres.Add(genreText);
                    }
                }
                catch (Exception /*e*/)
                {
                }

                movie.Cast = Cast;
                movie.Genres = Genres;

                list.Add(movie);
            }

            return list;
        }

        private static XmlDocument MovieSearch(string movieName)
        {
            Uri requestUrl = new Uri(@"http://api.netflix.com/catalog/titles");

            OAuthBase oauth = new OAuthBase();
            oauth.AddQueryParameter("term", oauth.UrlEncode(movieName));
            oauth.AddQueryParameter("max_results", "10");
            oauth.AddQueryParameter("expand", "synopsis,cast,directors,box_art");

            // prepare outputs
            string normalizedUrl;
            string normalizedRequestParameters;

            
            // generate request signature
            string sig = oauth.GenerateSignature(requestUrl,
                                                 consumerKey, 
                                                 secretKey, 
                                                 null, 
                                                 null,		// token , tokenSecret (not needed)
                                                 "GET", 
                                                 oauth.GenerateTimeStamp(), 
                                                 oauth.GenerateNonce(),
                                                 out normalizedUrl, 
                                                 out normalizedRequestParameters);
            
            // construct request
            string httpRequest = requestUrl + "?" +
                                 normalizedRequestParameters +
                                 "&oauth_signature=" + oauth.UrlEncode(sig);

            return Helper.FetchXML(httpRequest);
        }
    }
}
