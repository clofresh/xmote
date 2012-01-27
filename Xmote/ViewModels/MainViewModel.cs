using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;

namespace Xmote
{
    public class MainViewModel : INotifyPropertyChanged
    {

        #region public

        public MainViewModel()
        {
            this.TvEpisodes = new ObservableCollection<Item>();
            this.TvShows = new ObservableCollection<Item>();
            this.Movies = new ObservableCollection<Item>();
            this.Host = "127.0.0.1";
            this.Port = 8080;
            this.User = "xbmc";
            this.Password = "xbmc";
        }

        // Bindings
        public ObservableCollection<Item> TvEpisodes  { get; private set; }
        public ObservableCollection<Item> TvShows { get; private set; }
        public ObservableCollection<Item> Movies  { get; private set; }

        // Other public stuff
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsDataLoaded { get; private set; }
        public ImageBrush Background { get; private set; }

        public void LoadData()
        {
            var xbmc = new Xmote.Xbmc.Xbmc(this.Host, this.Port, this.User, this.Password);
            LoadTvEpisodes(xbmc);
            LoadTvShows(xbmc);
            LoadMovies(xbmc);

            this.IsDataLoaded = true;
        }

        #endregion

        #region private

        private string Host;
        private int Port;
        private string User;
        private string Password;

        private void LoadTvEpisodes(Xmote.Xbmc.Xbmc xbmc)
        {
            var properties = new List<string> { "title", "showtitle", "fanart", "thumbnail", "firstaired" };
            var limits = new Xmote.Xbmc.Limits { start = 0, end = 100 };
            var sort = new Xmote.Xbmc.Sort
            {
                ignorearticle = true,
                order = Xmote.Xbmc.SortOrder.Descending,
                method = Xmote.Xbmc.SortMethod.Date
            };

            xbmc.GetRecentlyAddedEpisodes(properties, limits, sort, (data) =>
            {
                foreach (var row in data.SelectToken("result.episodes"))
                {
                    int episodeId = (int)row["episodeid"];
                    var item = new Item()
                    {
                        Title = (string)row["title"],
                        Subtitle = (string)row["showtitle"],
                        SortKey = (string)row["firstaired"],
                        Play = new PlayCommand((e) =>
                        {
                            xbmc.PlayEpisode(episodeId);
                        })
                    };
                    item.SetThumbnail(GetVfsUri((string)row["thumbnail"]));
                    this.TvEpisodes.Add(item);
                    if (Background == null)
                    {
                        SetBackground(GetVfsUri((string)row["fanart"]));
                    }
                }
            });
        }

        private void LoadTvShows(Xmote.Xbmc.Xbmc xbmc)
        {
            var properties = new List<string> { "title", "genre", "thumbnail" };
            var limits = new Xmote.Xbmc.Limits { start = 0, end = 100 };
            var sort = new Xmote.Xbmc.Sort
            {
                ignorearticle = true,
                order = Xmote.Xbmc.SortOrder.Ascending,
                method = Xmote.Xbmc.SortMethod.Title
            };

            xbmc.GetTvShows(properties, limits, sort, (data) =>
            {
                foreach (var row in data.SelectToken("result.tvshows"))
                {
                    var item = new Item()
                    {
                        Title = (string)row["title"],
                        Subtitle = (string)row["genre"]
                    };
                    item.SetThumbnail(GetVfsUri((string)row["thumbnail"]));
                    this.TvShows.Add(item);
                }
            });
        }
        
        private void LoadMovies(Xmote.Xbmc.Xbmc xbmc)
        {
            var properties = new List<string> { "title", "tagline", "thumbnail" };
            var limits = new Xmote.Xbmc.Limits { start = 0, end = 100 };
            var sort = new Xmote.Xbmc.Sort
            {
                ignorearticle = true,
                order = Xmote.Xbmc.SortOrder.Ascending,
                method = Xmote.Xbmc.SortMethod.Title
            };

            xbmc.GetMovies(properties, limits, sort, (data) =>
            {
                foreach (var row in data.SelectToken("result.movies"))
                {
                    int movieId = (int)row["movieid"];
                    var item = new Item()
                    {
                        Title = (string)row["title"],
                        Subtitle = (string)row["tagline"],
                        Play = new PlayCommand((e) => {
                            xbmc.PlayMovie(movieId);
                        })
                    };
                    item.SetThumbnail(GetVfsUri((string)row["thumbnail"]));
                    this.Movies.Add(item);
                }
            });
        }

        private void SetBackground(Uri value)
        {
            var newBackground = new ImageBrush() { ImageSource = new BitmapImage(value), Opacity = .8 };
            if (newBackground != this.Background)
            {
                this.Background = newBackground;
                NotifyPropertyChanged("Background");
            }
        }

        private Uri GetVfsUri(string uriString)
        {
            return new Uri(String.Format("http://{0}:{1}@{2}:{3}/vfs/{4}", this.User, this.Password, this.Host, this.Port, uriString));
        }

        private void NotifyPropertyChanged(String propertyName)
        {
            Debug.WriteLine(propertyName + " property changed");
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}