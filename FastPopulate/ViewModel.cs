using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfMovieDB;

namespace FastPopulate
{
    static class Enumerable
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
                yield return item;
            }
        }
    }

    public class ViewModel : ViewModelBase
    {
        private BindingList<FileMovie> _fileMovies; // = new BindingList<FileMovie>();
        private BindingList<FileMovie> _fileMovies2 = new BindingList<FileMovie>();
        private Thread _getFiles;

        public BindingList<FileMovie> Files
        {
            get { return _fileMovies; }
            set { _fileMovies = value; }
        }

        public ViewModel()
        {
            if (!isDesignMode)
            {
                /*_getFiles = new Thread(new ThreadStart(GetFiles));
                _getFiles.Start();*/
                GetFiles();
            }
        }

        

        private const string server = @"\\server\videos\";
        
        private const string regex = @"^(?<moviename>[a-zA-Z0-9\.\,\'\-\&\!\s]+?)(?<year>\(\d{4}\))?(?<HD>\sHD)?(\sAC3)?(?<part>\s\d-\d)?$";

        private void GetFiles()
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.IsBackground = true;
            TextInfo textInfo = cultureInfo.TextInfo;

            var start = DateTime.Now;
            System.Diagnostics.Debug.WriteLine(start.ToString());
            try
            {
                HashSet<string> extensions = new HashSet<string>() { ".AVI", ".MKV", ".MP4", ".M4V" };
                List<FileMovie> list = new List<FileMovie>();

                Regex compiledRegex = new Regex(regex, RegexOptions.Compiled);


                Directory.EnumerateFiles(server + @"\", "*.*", SearchOption.TopDirectoryOnly)
                    .AsParallel()
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToUpper()))
                    .Select(file => 
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    if (fileName != textInfo.ToTitleCase(fileName))
                    {
                        string newFile = file.Replace(fileName, textInfo.ToTitleCase(fileName));
                        if (!string.IsNullOrEmpty(newFile))
                        {
                            File.Move(file, newFile);
                            file = newFile;
                        }
                    }

                    Match m = compiledRegex.Match(fileName);

                    FileMovie fileMovie = new FileMovie();
                    fileMovie.FileName  = fileName;
                    fileMovie.Directory = server;
                    fileMovie.MovieName = m.Groups["moviename"].Value.Trim();
                    fileMovie.Part      = m.Groups["part"].Value.Trim();
                    fileMovie.HD        = m.Groups["HD"].Value.Trim();

                    string year = Regex.Replace(m.Groups["year"].Value, @"[/(/)]", string.Empty);
                    int iYear = 0;
                    int.TryParse(year, out iYear);
                    fileMovie.FileYear = iYear;
                    fileMovie.FileDate = File.GetCreationTime(file);
                    
                    _fileMovies2.Add(fileMovie);

                    return file;
                }).ToList();

                _fileMovies = _fileMovies2;
                System.Diagnostics.Debug.WriteLine("Time - " + (DateTime.Now - start).ToString());
                OnPropertyChanged("Files");

            }
            catch (IOException e)
            {
                MessageBox.Show("Cannot access movie server");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
