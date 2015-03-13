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
        public string   FileName       { get; set; }
        public string   Directory      { get; set; }
        public string   MovieName      { get; set; }
        public int      FileYear       { get; set; }
        public string   Part           { set; get; }
        public string   HD             { set; get; }
        public DateTime FileDate       { get; set; }

        public void Refresh()
        {
            OnPropertyChanged(string.Empty);
        }
    }
}
