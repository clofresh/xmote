using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace Xmote
{
    public class Item : INotifyPropertyChanged, IComparable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private int _Id;
        public int Id
        {
            get { return _Id; }
            set
            {
                if (value != _Id)
                {
                    _Id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    NotifyPropertyChanged("Title");
                    NotifyPropertyChanged("TargetUri");
                }
            }
        }

        private string _Subtitle;
        public string Subtitle
        {
            get { return _Subtitle; }
            set
            {
                if (value != _Subtitle)
                {
                    _Subtitle = value;
                    NotifyPropertyChanged("Subtitle");
                }
            }
        }

        public ImageSource Thumbnail { get; private set; }
        public void SetThumbnail(Uri value)
        {
            var newThumbnail = new BitmapImage(value);
            if (newThumbnail != this.Thumbnail)
            {
                this.Thumbnail = newThumbnail;
                NotifyPropertyChanged("Thumbnail");
            }
        }

        public string SortKey { get; set; }
        public int CompareTo(object obj)
        {
            Item item = obj as Item;
            if (item == null)
            {
                throw new ArgumentException("Object is not Item");
            }
            return this.SortKey.CompareTo(item.SortKey);
        }

        private ICommand _Play;
        public ICommand Play
        {
            get { return _Play; }
            set
            {
                if (value != _Play)
                {
                    _Play = value;
                    NotifyPropertyChanged("Play");
                }
            }
        }

        public Uri TargetUri;

    }


    public class PlayCommand : ICommand
    {
        Action<object> _executeDelegate;

        public PlayCommand(Action<object> executeDelegate)
        {
            _executeDelegate = executeDelegate;
        }

        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }

        public bool CanExecute(object parameter) { return true; }
        public event EventHandler CanExecuteChanged;
    }

    public class TvShowItem : Item
    {
        private int _Season;
        public int Season
        {
            get { return _Season; }
            set
            {
                if (value != _Season)
                {
                    _Season = value;
                    NotifyPropertyChanged("_Season");
                }
            }
        }

        public Uri TargetUri
        {
            get
            {
                return new Uri(String.Format("/TvShowsPage.xaml?Id={0}&Season={1}", Id, Season), UriKind.Relative);
            }
        }

    }

    public class TvSeasonItem : Item
    {
        public ObservableCollection<TvEpisodeItem> Episodes { get; private set; }
        private int _Season;
        public int Season
        {
            get { return _Season; }
            set
            {
                if (value != _Season)
                {
                    _Season = value;
                    NotifyPropertyChanged("_Season");
                }
            }
        }
        public TvSeasonItem() 
        {
            Episodes = new ObservableCollection<TvEpisodeItem>();
        }
    }

    public class TvEpisodeItem : Item
    {
        private int _Season;
        public int Season
        {
            get { return _Season; }
            set
            {
                if (value != _Season)
                {
                    _Season = value;
                    NotifyPropertyChanged("_Season");
                }
            }
        }
    }


    public class MovieItem : Item
    {
    }

}