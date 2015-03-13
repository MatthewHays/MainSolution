using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.Configuration;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Threading;
using System.Xml;

namespace WpfMovieDB
{
    static class Helper
    {
        public static string moviedataDir = ConfigurationManager.AppSettings["MovieDataDir"];

        public static String Clean(string name)
        {
            return name.Replace(",", "");
        }

        public static void Try(Action method)
        {
            try
            {
                method();
            }
            catch (Exception)
            {
            }
        }

        public static void RemoveImage(string movieName)
        {
            movieName = Clean(movieName);

            GC.Collect();

            try
            {
                if (File.Exists(moviedataDir + "\\" + movieName + ".jpg"))
                    File.Delete(moviedataDir + "\\" + movieName + ".jpg");
            }
            catch (Exception /*e*/)
            {
                //TODO - do something here
            }
        }

        public static string GetImage(String movieName, string url)
        {
            return GetImage(movieName, url, true);
        }

        public static string GetImage(String movieName, string url, bool cache)
        {
            try
            {
                movieName = Clean(movieName);

                if (string.IsNullOrEmpty(movieName))
                    return moviedataDir + "\\" + "blank.jpg";
                
                if (!File.Exists(moviedataDir + "\\" + movieName + ".jpg") && !string.IsNullOrEmpty(url))
                {
                    WebClient objwebClient = new WebClient();
                    url = url.Replace("/large/", "/ghd/");
                    MemoryStream ms = new MemoryStream(objwebClient.DownloadData(url));
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms);

                    image = ImageResize(image);

                    if (!Directory.Exists(moviedataDir))
                        Directory.CreateDirectory(moviedataDir);
                    if (cache)
                    {
                        image.Save(moviedataDir + "\\" + movieName + ".jpg", ImageFormat.Jpeg);
                    }
                }

                return moviedataDir + "\\" + movieName + ".jpg";
                
            }
            catch (Exception /*e*/)
            {
                return moviedataDir + "\\" + "blank.jpg";
            }
        }

        /// <summary>
        /// Resize an image to 600 * 800 window
        /// </summary>
        /// <param name="src_image"></param>
        /// <returns></returns>
        private static System.Drawing.Image ImageResize(System.Drawing.Image src_image)
        {
            int width = 800 * src_image.Width / src_image.Height;
            int height = 800;

            Bitmap bitmap = new Bitmap(width, height, src_image.PixelFormat);
            Graphics new_g = Graphics.FromImage(bitmap);

            new_g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            new_g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            new_g.DrawImage(src_image, 0, 0, bitmap.Width, bitmap.Height);

            return bitmap;
        }

        public static string NormaliseName(string name)
        {
            string result = name.ToUpper();
            
            /*result = result.Replace(" VIII", " 8");
            result = result.Replace(" VII",  " 7");
            result = result.Replace(" VI",   " 6");
            result = result.Replace(" V",    " 5");
            result = result.Replace(" IV",   " 4");*/
            result = result.Replace(" III",  " 3");
            result = result.Replace(" II",   " 2");
            result = result.Replace("&", "AND");

            result = Regex.Replace(result, @"[^a-zA-Z0-9]", "");

            result = result.Replace("THREE", "3");
            result = result.Replace("TWO",   "2");
            result = result.Replace("ONE",   "1");

            result = result.Replace("PART", "");
            result = result.Replace("COLLECTORSEDITION", "");
            
            return result.Trim();
        }

        private static List<string> AcceptedGenres = new List<string>
        {
            "Action",
            "Sci-Fi",
            "Thriller",
            "Adventure",
            "Animation",
            "Comedy",
            "Horror",
            "Drama",
            "Western",
            "Documentary",
            "Romance",
            "Crime"
        };

        public static List<string> NormaliseGenres(List<string> genres)
        {
            List<string> cleanedGenres = new List<string>();

            //TODO - potentially normalise SciFi -> Sci-Fi  etc?
            //spelling errors?
            foreach (string acceptedGenres in AcceptedGenres)
            {
                if (genres.FirstOrDefault(i => i.Contains(acceptedGenres)) != null)
                    cleanedGenres.Add(acceptedGenres);
            }

            return new List<string>(cleanedGenres.Distinct());
        }


        /// <summary>
        /// Download movie info from an online source
        /// </summary>
        /// <param name="movieName"></param>
        /// <returns></returns>
        public static Movie GetMovie(string movieName, int year, bool bestMatch = false, DispatcherObject parent = null)
        {
            List<Movie> movies = MovieServiceManager.GetMovies(movieName);
            Movie movie = null;
            if (movies.Count == 0)
            {
                return null;
            }
            else
            {
                string normalisedMovieName = Helper.NormaliseName(movieName);
                List<Movie> bestMatchMovies = movies.FindAll(m => Helper.NormaliseName(m.MovieName) == normalisedMovieName && m.Year == year);
                if (bestMatchMovies.Count == 1)
                    return bestMatchMovies[0];
                
                bestMatchMovies = movies.FindAll(m => Helper.NormaliseName(m.MovieName) == normalisedMovieName);
                if (bestMatchMovies.Count == 1)
                    return bestMatchMovies[0];
                
                /*bestMatchMovies = movies.FindAll(m => Helper.NormaliseName(m.MovieName).Contains(normalisedMovieName) || normalisedMovieName.Contains(Helper.NormaliseName(m.MovieName)));
                if (bestMatchMovies.Count == 1)
                    return bestMatchMovies[0];*/

                if (bestMatch)
                    return null;

                if (parent == null || parent.Dispatcher.Thread == Thread.CurrentThread)
                {
                    //else just show all the results to the user and let them decide
                    SelectionForm form = new SelectionForm(movieName + " - " + year, movies);
                    if (form.ShowDialog() == true)
                        movie = form.SelectedMovie;
                }
                else
                {
                    parent.Dispatcher.Invoke(
                        new Action
                        (
                            () =>
                            { //else just show all the results to the user and let them decide
                                SelectionForm form = new SelectionForm(movieName + " - " + year, movies);
                                if (form.ShowDialog() == true)
                                    movie = form.SelectedMovie;
                            }
                        ));
                }
            }

            return movie;
        }

        public static string Fetch(string url)
        {
            int attempt = 0;
            while (attempt < 2)
            {
                attempt++;
                try
                {
                    WebRequest req = HttpWebRequest.Create(url);
                    //req.Timeout = 1000;

                    WebResponse resp = req.GetResponse();

                    Stream s = resp.GetResponseStream();

                    XmlDocument doc = new XmlDocument();
                    string payload = new StreamReader(s).ReadToEnd();
                    s.Close();
                    resp.Close();
                    return payload;
                    
                }
                catch (Exception)
                {
                    //Logger.ReportWarning("Error requesting: " + url + "\n" + ex.ToString());
                }
            }

            return string.Empty;

        }

        /// <summary>
        /// Fetch an XmlDocument from an http url
        /// </summary>
        /// <param name="url"></param>
        /// <returns>document on success, null on failure</returns>
        public static XmlDocument FetchXML(string url)
        {
            string payload = Fetch(url);

            if (string.IsNullOrWhiteSpace(payload))
                return new XmlDocument();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(payload);
                return doc;
            }
            catch (Exception)
            {
                //Logger.ReportWarning("Error requesting: " + url + "\n" + ex.ToString());
            }

            return new XmlDocument();
        }

    } //class

}
