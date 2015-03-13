using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;

namespace iTunesNormaliser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //find the two xml files
            //iTunes Music Library.xml


            //original
            //<key>Music Folder</key><string>file://localhost/C:/Users/Matthew/Music/</string> 
            //<key>Location</key><string>file://localhost/C:/Users/Matthew/Music/4%20Strings/Believe/01%20Take%20Me%20Away%20(Into%20The%20Night).mp3</string> 


            //new values
            //<key>Music Folder</key><string>\\homeserver\music\Jenny\</string>
            //<key>Location</key><string>/Jenny/Theme/Hollywood%20Goes%20To%20War/11%20Platoon.mp3</string>
            this.Cursor = Cursors.Wait;

            string server1 = @"\\Homeserver\Music\Matthew\iTunes\";
            string server2 = @"\\Homeserver\Music\Jenny\iTunes\";

            Dictionary<string, string> replacements = new Dictionary<string, string>()
            {
                {@"<key>Music Folder</key><string>file://localhost/C:/Users/Matthew/Music/</string>", 
                 @"<key>Music Folder</key><string>\\homeserver\music\Matthew\</string>"},
                
                {@"<key>Music Folder</key><string>file://localhost/C:/Users/Jenny/Music/</string>", 
                 @"<key>Music Folder</key><string>\\homeserver\music\Jenny\</string>"},
                
                {@"<key>Location</key><string>file://localhost/C:/Users/Matthew/Music/",
                 @"<key>Location</key><string>/Matthew/"},
                
                {@"<key>Location</key><string>file://localhost/C:/Users/Jenny/Music/", 
                 @"<key>Location</key><string>/Jenny/"}
            };

            string[] files = new string[2];
            files[0] = Directory.GetFiles(server1, "iTunes Music Library.xml", SearchOption.TopDirectoryOnly)[0];
            files[1] = Directory.GetFiles(server2, "iTunes Music Library.xml", SearchOption.TopDirectoryOnly)[0];

            foreach (string filePath in files)
            {
                StreamReader reader = new StreamReader(filePath);
                string originalcontent = reader.ReadToEnd();
                reader.Close();
                
                string content = originalcontent;

                foreach (KeyValuePair<string, string> kvp in replacements)
                {
                    content = content.Replace(kvp.Key, kvp.Value);
                }

                StreamWriter writer = new StreamWriter(filePath);
                writer.Write(content);
                writer.Close();
            }

            this.Cursor = Cursors.Arrow;
            Application.Current.Shutdown();
            
        }
    }
}
