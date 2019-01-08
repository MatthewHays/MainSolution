using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Catel.Logging;
using Catel.IoC;
using Catel.MVVM;
using Orc.Controls;
using System.Threading;

namespace WpfTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {

            /*Catel.MVVM.
            ServiceLocator.Default.*/
            //ServiceLocator.Default.RegisterType<ICommandManager>()
            //ServiceLocator.Default.RegisterInstance(typeof(IApplicationLogFilterGroupService), new Orc.Controls.ApplicationLogFilterGroupService(new Orc.FileSystem.FileService(), new Catel.Runtime.Serialization.Xml.XmlSerializer()));
            ServiceLocator.Default.RegisterInstance(typeof(IApplicationLogFilterGroupService), new service());
            ServiceLocator.Default.RegisterInstance(typeof(ICommandManager), new Catel.MVVM.CommandManager());

            InitializeComponent();
            //LogViewer.LogListenerType = typeof(MainWindow);

            var t = new Thread(() =>
            {
                int i = 0;
                while (true)
                {
                    switch (i % 3)
                    {
                        case 0:
                            Log.Error($"Hello {i++}");
                            break;
                        case 1:
                            Log.Warning($"Hello {i++}");
                            break;
                        case 2:
                            Log.Info($"Hello {i++}");
                            break;
                    }
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }
                
            });
            t.IsBackground = true;
            t.Start();

            //LogManager.RegisterDebugList ener();
            /*for (var i=0; i < 1000; ++i)
                Log.Error($"Hello {i}");*/
        }
    }

    public class service : IApplicationLogFilterGroupService
    {
        public Task<IEnumerable<LogFilterGroup>> LoadAsync()
        {
            return null;
        }

        public Task SaveAsync(IEnumerable<LogFilterGroup> filterGroups)
        {
            return null;
        }
    }
}
