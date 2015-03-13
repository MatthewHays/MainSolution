using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfMovieDB
{
    static class MovieServiceManager
    {
        /// <summary>
        /// Return all potential matches for a given movie
        /// </summary>
        /// <param name="movieName"></param>
        /// <returns></returns>
        public static List<Movie> GetMovies(string movieName)
        {
            //get the list of matches from netflix
            List<Movie> netflix = Netflix.GetMovies(movieName);

            //get the list of matches from omdb
            List<Movie> omdb = OMDB.GetMovies(movieName);

            //union the two together, outer join, merging results where ==
            List<Movie> returnList = new List<Movie>();
            
            returnList.AddRange(netflix);

            foreach (Movie movie in omdb)
            {
                string normalisedName = Helper.NormaliseName(movie.MovieName);
                Movie existingMovie = returnList.FirstOrDefault(m => Helper.NormaliseName(m.MovieName) == normalisedName && m.Year == movie.Year);

                if (existingMovie == null)
                {
                    returnList.Add(movie);
                }
                else
                {
                    existingMovie.MovieName        = existingMovie.MovieName;
                    existingMovie.Year             = existingMovie.Year > 0 ? existingMovie.Year : movie.Year;
                    existingMovie.OMDBRating       = movie.OMDBRating;
                    existingMovie.IMDBRating       = movie.IMDBRating;
                    //existingMovie.NetflixRating    = movie.NetflixRating;
                    existingMovie.MetacriticRating = movie.MetacriticRating;
                    existingMovie.Description      = string.IsNullOrEmpty(movie.Description) ? existingMovie.Description : movie.Description;
                    existingMovie.CoverArt         = string.IsNullOrEmpty(movie.CoverArt) ? existingMovie.CoverArt : movie.CoverArt;
                    existingMovie.ThumbNail        = string.IsNullOrEmpty(movie.ThumbNail) ? existingMovie.ThumbNail : movie.ThumbNail;
                    existingMovie.Genres           = movie.Genres.Count > 0 ? movie.Genres : existingMovie.Genres;
                    existingMovie.Cast             = movie.Cast.Count > 0 ? movie.Cast : existingMovie.Cast;
                };
            }

            return returnList;
        }
    }
}
