using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;


namespace WpfMovieDB
{
    public class IMDB  
    {
        const string BaseUrl = "http://www.imdb.com/title/{0}";

        public static Tuple<double, double> GetIMDBRatings(string imdbid)
        {
            string url = string.Format(BaseUrl, imdbid);
            string doc = Helper.Fetch(url);

            MatchCollection matches = Regex.Matches(doc, "<a href=\"criticreviews\">(\\d{2})/100</a>");
            double IMDBRating = 0;
            double metacriticRating = 0;

            if ((matches.Count == 1) && (Double.TryParse(matches[0].Groups[1].Value, out metacriticRating)))
            {
                metacriticRating = metacriticRating / 10.0;
            }
            
                 //matches = Regex.Matches(doc, "<div class=\"star-box-giga-star\">([-+]?[0-9]*\\.?[0-9]*)</div>");
            matches = Regex.Matches(doc, "<span itemprop=\"ratingValue\">([-+]?[0-9]*\\.?[0-9]*)</span>");
            if (matches.Count == 1)
                Double.TryParse(matches[0].Groups[1].Value, out IMDBRating);

            return new Tuple<double, double>(IMDBRating, metacriticRating);
        }
    }
}
