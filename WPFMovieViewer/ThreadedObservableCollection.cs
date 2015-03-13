using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace WPFMovieViewer
{
    /// <summary>
    /// A collection which notifies dispatcher listeners on the thread of their owner
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadedObservableCollection<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var eh = CollectionChanged;
            if (eh != null)
            {
                foreach (Delegate del in eh.GetInvocationList())
                {
                    DispatcherObject dispatcher = del.Target as DispatcherObject;

                    if (dispatcher != null && dispatcher.CheckAccess() == false)
                    {
                        dispatcher.Dispatcher.Invoke(DispatcherPriority.DataBind, (Action)(() => OnCollectionChanged(e)));
                    }
                    else
                    {
                        NotifyCollectionChangedEventHandler n = del as NotifyCollectionChangedEventHandler;
                        n(this, e);
                    }
                }
            }
        }
    }
}
