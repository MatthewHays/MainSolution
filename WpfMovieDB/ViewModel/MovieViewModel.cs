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
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace WpfMovieDB
{
    class MovieViewModel : ViewModelBase
    {
        public static string server = ConfigurationManager.AppSettings["MovieDir"];
        public static string xmlFile = ConfigurationManager.AppSettings["XMLFile"];
        public static string htmlFile = ConfigurationManager.AppSettings["HTMLFile"];
        public static string moviedataDir = ConfigurationManager.AppSettings["MovieDataDir"];
        private const string regex = @"^(?<moviename>[a-zA-Z0-9\.\,\'\-\&\!\s]+?)(?<year>\(\d{4}\))?(?<HD>\sHD)?(\sAC3)?(?<part>\s\d-\d)?$";

        public ObservableCollection<FileMovie> FileMovies 
        {
            get { return _fileMovies; }
            set { _fileMovies = value; }
        }

        private Dictionary<String, Movie> _movies = new Dictionary<string,Movie>();
        private ObservableCollection<FileMovie> _fileMovies = new FastObservableCollection<FileMovie>();

        public MovieViewModel()
        {
            FileMovie.Movies = _movies;

            if (isDesignMode)
            {
                FileMovie tableMovie = new FileMovie();
                tableMovie.FileName = "File.avi";
                tableMovie.MovieName = "TestMovie";

                FileMovie tableMovie2 = new FileMovie();
                tableMovie2.FileName = "File2.avi";
                tableMovie2.MovieName = "TestMovie2";

                FileMovie tableMovie3 = new FileMovie();
                tableMovie3.FileName = "File3.avi";
                tableMovie3.MovieName = "TestMovie3";

                Movie movie = new Movie();
                movie.MovieName = "TestMovie";
                movie.Year = 2010;
                movie.JenWantsToWatch = true;
                movie.Watched = true;
                movie.IMDBRating = 1.2;
                movie.NetflixRating = 1.1;
                movie.Description = "Test Description";

                _movies.Add(movie.MovieName + "." + movie.Year, movie);

                Movie movie2 = new Movie();
                movie2.MovieName = "TestMovie2";
                movie2.Year = 2011;
                movie2.JenWantsToWatch = true;
                movie2.Watched = true;
                movie2.IMDBRating = 1.3;
                movie2.NetflixRating = 1.2;
                movie2.Description = "Test Description2";

                _movies.Add(movie2.MovieName + "." + movie2.Year, movie2);

                FileMovies.Add(tableMovie);
                FileMovies.Add(tableMovie2);
                FileMovies.Add(tableMovie3);
            }
            else
            {
                LoadMovieFile(xmlFile);
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

                Action a = new Action(() => { OnPropertyChanged("FileMovies"); });

                Task.Factory.StartNew(() => GetFiles()).ContinueWith((c) => 
                    { 
                        _fileMovies = c.Result;
                        dispatcher.BeginInvoke(a); 
                    });
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
                /*if (File.Exists(file))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Movie>));

                    FileStream movieDataFile = File.OpenRead(file);
                    List<Movie> movies = (List<Movie>)serializer.Deserialize(movieDataFile);
                    movieDataFile.Close();

                    foreach (Movie movie in movies)
                    {
                        _movies.Add(movie.MovieName + "." + movie.Year, movie);
                    }

                    return true;
                }*/

                string jsonFile = file + ".json";
                if (File.Exists(jsonFile))
                {
                    FileStream movieFile = File.OpenRead(jsonFile);
                    StreamReader sr = new StreamReader(movieFile);

                    List<Movie> movies = JsonConvert.DeserializeObject<List<Movie>>(sr.ReadToEnd());
                    foreach (Movie movie in movies)
                    {
                        _movies.Add(movie.MovieName + "." + movie.Year, movie);
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


        public void SaveMovieFile()
        {
            try
            {
                //XmlSerializer serializer = new XmlSerializer(typeof(List<Movie>));

                List<Movie> movies = new List<Movie>();
                foreach (KeyValuePair<string, Movie> kvp in _movies)
                    movies.Add(kvp.Value);

                /*movies.Sort((a, b) => a.MovieName.CompareTo(b.MovieName));

                File.Delete(xmlFile);
                FileStream movieDataFile = File.Create(xmlFile);
                serializer.Serialize(movieDataFile, movies);
                movieDataFile.Close();*/


                string json = JsonConvert.SerializeObject(movies, Formatting.Indented);
                string jsonLocation = xmlFile + ".json";
                System.IO.TextWriter writeFile = new StreamWriter(jsonLocation);
                writeFile.WriteLine(json);
                writeFile.Flush();
                writeFile.Close();
                writeFile = null;

                /*FileStream htmFile = File.Create(htmlFile);
                using (StreamWriter w = new StreamWriter(htmFile, Encoding.UTF8))
                {
                    w.WriteLine(@"<!DOCTYPE html>
                                <html>
                                <head>
                                <script type='text/javascript' src='\\server\videos\moviedata\web\jquery.js'></script>
                                <script type='text/javascript' src='\\server\videos\moviedata\web\jquery.datatables.js'></script>
                                <script type='text/javascript' src='\\server\videos\moviedata\web\jquery.lazyload.js'></script>
                                <script type='text/javascript' src='\\server\videos\moviedata\web\jquery.scrollstop.js'></script> 

                                <script type='text/javascript' charset='utf-8'>
                                    $(function() 
                                    {
                                        $('img').lazyload(
                                        {
                                            event: 'scrollstop'
                                        });
                                    });

                                    $(document).ready(function() 
                                    {
                                        $('#movies').dataTable();
                                    } );
                                </script>


                                </head>
                                
                                <style type='text/css'>
                                    @import '\\server\videos\moviedata\web\tablestyles.css';
                                </style>

                                <body>
                                <table class='display' id='movies' width='100%'>
                                   <thead>
                                       <tr>
                                           <th>AlbumArt</th>
                                           <th>MovieName</th>
                                           <th>Year</th>
                                           <th>Description</th>
                                           <th>Genres</th>
                                       </tr>
                                   </thead>
                                <tbody>");

                    foreach (Movie movie in movies)
                    {
                        //"<td> <img src=\" " + movie.CoverArtConverted + "\" width=\"100\" height=\"100\" /></td>" + 

                        w.WriteLine(string.Format(@"<tr>
                                    <td><img class='lazy' src='{0}' data-original='{1}' width='100' height='100'/></td> 
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                    </tr>", 
                                    @"\\server\videos\moviedata\web\grey.gif", movie.CoverArtConverted, movie.MovieName, movie.Year, movie.Description, string.Join(",", movie.Genres)));
                    }

                     w.WriteLine("</tbody>");
                     w.WriteLine("</table>");
                     w.WriteLine("</body>");
                     w.WriteLine("</html>");
                }
                htmFile.Close();*/

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private ObservableCollection<FileMovie> GetFiles()
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            FastObservableCollection<FileMovie> _fileMoviesTemp = new FastObservableCollection<FileMovie>();

            try
            {
                HashSet<string> extensions = new HashSet<string>() { ".AVI", ".MKV", ".MP4", ".M4V" };

                Regex compiledRegex = new Regex(regex, RegexOptions.Compiled);

                Directory.EnumerateFiles(server + @"\", "*.*", SearchOption.TopDirectoryOnly)
                    .AsParallel()
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToUpper()))
                    .Select(file =>
                    {
                        var fileName = Path.GetFileName(file);
                        var fileNameWithOutExtension = Path.GetFileNameWithoutExtension(fileName);

                        if (fileNameWithOutExtension != textInfo.ToTitleCase(fileNameWithOutExtension))
                        {
                            string newFile = file.Replace(fileNameWithOutExtension, textInfo.ToTitleCase(fileNameWithOutExtension));
                            if (!string.IsNullOrEmpty(newFile))
                            {
                                File.Move(file, newFile);
                                file = newFile;
                            }
                        }

                        Match m = compiledRegex.Match(fileNameWithOutExtension);

                        FileMovie fileMovie = new FileMovie();
                        fileMovie.FileName = fileName;
                        fileMovie.Directory = server;
                        fileMovie.MovieName = m.Groups["moviename"].Value.Trim();
                        fileMovie.Part = m.Groups["part"].Value.Trim();
                        fileMovie.HD = m.Groups["HD"].Value.Trim();

                        string year = Regex.Replace(m.Groups["year"].Value, @"[/(/)]", string.Empty);
                        int iYear = 0;
                        int.TryParse(year, out iYear);
                        fileMovie.FileYear = iYear;
                        fileMovie.FileDate = File.GetCreationTime(file);

                        lock (this)
                        {
                            _fileMoviesTemp.Add(fileMovie);
                        }

                        return file;
                    }).ToList();

                /*
                //find all movies for which there are no file movies and delete them
                var excessMovies = _movies.Where(m => _fileMovies.FirstOrDefault(tm => tm.MovieName == m.Value.MovieName) == null);

                List<string> toDelete = new List<string>();
                foreach (var excessMovie in excessMovies)
                {
                    toDelete.Add(excessMovie.Key);
                }

                foreach (string movieName in toDelete)
                {
                    _movies.Remove(movieName);
                }*/

                /*
                //check for thumbnail pics that dont have any movies and delete them
                List<string> pics = new List<string>(Directory.GetFiles(moviedataDir, "*.jpg", SearchOption.TopDirectoryOnly));
                List<string> extrapics = pics.Where(
                    pic =>
                        _movies.FirstOrDefault(m => Helper.Clean(m.Key.ToUpper()) == Helper.Clean(Path.GetFileNameWithoutExtension(pic).ToUpper())).Key == null).ToList();

                foreach (string file in extrapics)
                {
                    System.Diagnostics.Debug.WriteLine(file);
                    if (Path.GetFileNameWithoutExtension(file).ToUpper() != "BLANK")
                        File.Delete(file);
                }*/

                /*
                //rename all the movie files to include dates
                foreach (FileMovie fileMovie in FileMovies)
                {
                    string fileName = fileMovie.MovieName;
                    if (fileMovie.Year > 0)
                        fileName = fileName + " (" + fileMovie.Year + ")";
                    if (!string.IsNullOrWhiteSpace(fileMovie.Part))
                        fileName += " " + fileMovie.Part;
                    if (!string.IsNullOrWhiteSpace(fileMovie.HD))
                        fileName += " " + fileMovie.HD;
                    fileName += Path.GetExtension(fileMovie.FileName);

                    List<string> movies = FileMovies.Where(fm => fm.MovieName == fileMovie.MovieName).Select(fm => fm.MovieName).ToList();

                    if (fileName != fileMovie.FileName && !String.IsNullOrEmpty(fileName))
                    {
                        System.Diagnostics.Debug.WriteLine(fileName + " - " + fileMovie.FileName);
                        try
                        {
                            if (!File.Exists(fileMovie.Directory + "\\" + fileName))
                                File.Move(fileMovie.Directory + "\\" + fileMovie.FileName, fileMovie.Directory + "\\" + fileName);
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine("Error renaming - " + e.Message);
                        }

                        fileMovie.FileName = fileName;
                    }
                }*/
            }
            catch (IOException e)
            {
                MessageBox.Show("Cannot access movie server");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return _fileMoviesTemp;
        }


        public void UpdateMovieName(FileMovie fileMovie, string newName)
        {
            FileInfo info = new FileInfo(fileMovie.Directory + @"\" + fileMovie.FileName);

            Movie movie = fileMovie.Movie;

            //rename the file
            info.MoveTo(fileMovie.Directory + @"\" + newName);

            //rename the file object
            Match m = Regex.Match(newName, regex);
            fileMovie.MovieName = m.Groups[1].Value.Trim();
            fileMovie.FileName = newName;

            if (_movies.ContainsKey(movie.MovieName + "." + movie.Year))
                _movies.Remove(movie.MovieName + "." + movie.Year);
            
            fileMovie.Movie = null;
        }

        public void UpdateFileDate(FileMovie fileMovie)
        {
            File.SetCreationTime(fileMovie.Directory + @"\" + fileMovie.FileName, DateTime.Now);
            fileMovie.FileDate = DateTime.Now;
        }


        public void AddMovie(Movie movie)
        {
            lock (this)
            {
                //Add the movie meta info to the list
                if (_movies.ContainsKey(movie.MovieName + "." + movie.Year))
                {
                    Movie initialMovie = _movies[movie.MovieName + "." + movie.Year];
                    _movies.Remove(movie.MovieName + "." + movie.Year);

                    movie.Watched = initialMovie.Watched;
                    movie.JenWantsToWatch = movie.JenWantsToWatch;
                }

                _movies.Add(movie.MovieName + "." + movie.Year, movie);
            }
        }


        public void RemoveFileMovie(FileMovie fileMovie)
        {
            lock (this)
            {
                FileMovies.Remove(fileMovie);
                if (fileMovie.Movie != null)
                    RemoveMovie(fileMovie.Movie);
            }
        }


        public void RemoveMovie(Movie movie)
        {
            lock (this)
            {
                _movies.Remove(movie + "." + movie.Year);
                Helper.RemoveImage(movie.MovieName + "." + movie.Year);
            }
        }

        public void LookupAllMovies()
        {
            CountdownEvent count = new CountdownEvent(FileMovies.Count());
            Window mainWindow = Application.Current.MainWindow;
            mainWindow.Cursor = Cursors.Wait;
            ThreadPool.RegisterWaitForSingleObject(count.WaitHandle, new WaitOrTimerCallback(WaitProc), mainWindow, -1, true);

            foreach (FileMovie fileMovie in FileMovies)
            {
                if (fileMovie.Part == "2-2" || fileMovie.DataDownloaded)
                {
                    count.Signal(); 
                    continue;
                }

                System.Diagnostics.Debug.WriteLine("fileMovie.MovieName - ");
                Movie movie = Helper.GetMovie(fileMovie.MovieName, fileMovie.Year, true, mainWindow);
                if (movie == null)
                {
                    System.Diagnostics.Debug.WriteLine("Not found");
                    continue;
                }

                //force the movie names to be the same
                movie.MovieName = fileMovie.MovieName;
                System.Diagnostics.Debug.WriteLine("Found");
                AddMovie(movie);

                count.Signal();

                /*FileMovie outerFileMovie = fileMovie;
                ThreadPool.QueueUserWorkItem(new WaitCallback((innerMovieObj) =>
                {
                    FileMovie innerFileMovie = innerMovieObj as FileMovie;
                    Movie movie = Helper.GetMovie(innerFileMovie.MovieName, innerFileMovie.Year, true, mainWindow);
                    if (movie == null)
                    {
                        System.Diagnostics.Debug.WriteLine("No movie data for - " + innerFileMovie.MovieName);
                        return;
                    }

                    //force the movie names to be the same
                    movie.MovieName = fileMovie.MovieName;
                    System.Diagnostics.Debug.WriteLine("Found movie - " +  movie.MovieName);
                    AddMovie(movie);

                    count.Signal();

                }), outerFileMovie);*/
            }
        }


        public void UpdateAllRatings()
        {
            DebugWindow debug = new DebugWindow();
            debug.Show();

            CountdownEvent count = new CountdownEvent(_movies.Count);
            Window mainWindow = Application.Current.MainWindow;
            mainWindow.Cursor = Cursors.Wait;
            ThreadPool.RegisterWaitForSingleObject(count.WaitHandle, new WaitOrTimerCallback(WaitProc), mainWindow, -1, true);

            foreach (Movie movie in _movies.Values)
            {
                /*if (!(movie.NetflixRating == 0 && movie.IMDBRating == 0 && movie.MetacriticRating == 0 && movie.OMDBRating == 0))
                {
                    count.Signal();
                    continue;
                }*/

                Movie newMovie = Helper.GetMovie(movie.MovieName, movie.Year, false, mainWindow);
                System.Diagnostics.Debug.WriteLine(count.CurrentCount + " : " + movie.MovieName);
                debug.AddLine(count.CurrentCount + " : " + movie.MovieName);
                if (newMovie != null)
                {
                    
                    debug.AddLine("OMDB "+ movie.OMDBRating + " -> " + newMovie.OMDBRating);
                    movie.OMDBRating = newMovie.OMDBRating;
                    debug.AddLine("IMDB " + movie.IMDBRating + " -> " + newMovie.IMDBRating);
                    movie.IMDBRating = newMovie.IMDBRating;
                    debug.AddLine("Netflix " + movie.NetflixRating + " -> " + newMovie.NetflixRating);
                    movie.NetflixRating = newMovie.NetflixRating;
                    debug.AddLine("Metacritic " + movie.MetacriticRating + " -> " + newMovie.MetacriticRating);
                    movie.MetacriticRating = newMovie.MetacriticRating;

                    
                }

                Thread.Sleep(1000);

                count.Signal();

                /*Movie outerMovie = movie;
                ThreadPool.QueueUserWorkItem(new WaitCallback((innerMovieObj) =>
                {
                    Movie innerMovie = innerMovieObj as Movie;
                    Movie newMovie = Helper.GetMovie(innerMovie.MovieName, innerMovie.Year, false, mainWindow);

                    lock (this)
                    {
                        System.Diagnostics.Debug.WriteLine(count.CurrentCount + " : " + innerMovie.MovieName);
                        if (newMovie != null)
                        {
                            System.Diagnostics.Debug.WriteLine(innerMovie.OMDBRating + " -> " + newMovie.OMDBRating);
                            innerMovie.OMDBRating = newMovie.OMDBRating;
                            System.Diagnostics.Debug.WriteLine(innerMovie.IMDBRating + " -> " + newMovie.IMDBRating);
                            innerMovie.IMDBRating = newMovie.IMDBRating;
                            System.Diagnostics.Debug.WriteLine(innerMovie.NetflixRating + " -> " + newMovie.NetflixRating);
                            innerMovie.NetflixRating = newMovie.NetflixRating;
                            System.Diagnostics.Debug.WriteLine(innerMovie.MetacriticRating + " -> " + newMovie.MetacriticRating);
                            innerMovie.MetacriticRating = newMovie.MetacriticRating;
                        }

                        count.Signal();
                    }


                }), outerMovie);*/
            }
            debug.Close();
        }

        #region Private Methods

        private  void WaitProc(object state, bool timedOut)
        {
            Window mainWindow = state as Window;

            if (mainWindow == null || mainWindow.Dispatcher.Thread == Thread.CurrentThread)
            {
                //else just show all the results to the user and let them decide
                MessageBox.Show("Lookup complete");
                mainWindow.Cursor = Cursors.Arrow;
            }
            else
            {
                mainWindow.Dispatcher.Invoke(
                    new Action
                    (
                        () =>
                        {
                            WaitProc(state, timedOut);
                        }
                    ));
            }
            
        }

        #endregion
    } //class
} //namespace
