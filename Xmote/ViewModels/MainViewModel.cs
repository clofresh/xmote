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
using Xmote.Xbmc;

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
        }

        // Bindings
        public ObservableCollection<Item> TvEpisodes  { get; private set; }
        public ObservableCollection<Item> TvShows { get; private set; }
        public ObservableCollection<Item> Movies  { get; private set; }

        // Other public stuff
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsDataLoaded { get; set; }
        public ImageBrush Background { get; private set; }

        public void LoadData()
        {
            LoadTvEpisodes();
            LoadTvShows();
            LoadMovies();

            this.IsDataLoaded = true;
        }

        #endregion

        #region private

        public bool RequestsPending 
        { 
            get { return pending > 0; }
        }

        private int pending = 0;
        private int IncrementPending() {
            pending += 1;
            NotifyPropertyChanged("RequestsPending");
            return pending;
        }
        private int DecrementPending()
        {
            pending -= 1;
            NotifyPropertyChanged("RequestsPending");
            return pending;
        }

        private void LoadTvEpisodes()
        {
            var xbmc = Xbmc.Xbmc.instance();
            IncrementPending();
            xbmc.GetRecentlyAddedEpisodes((rows) =>
            {
                foreach (var row in rows)
                {
                    int episodeId = (int)row["episodeid"];
                    var item = new TvEpisodeItem()
                    {
                        Id = episodeId,
                        Title = (string)row["title"],
                        Subtitle = (string)row["showtitle"],
                        SortKey = (string)row["firstaired"],
                        Play = new PlayCommand((e) =>
                        {
                            xbmc.PlayEpisode(episodeId);
                        })
                    };
                    item.SetThumbnail(xbmc.GetVfsUri((string)row["thumbnail"]));
                    this.TvEpisodes.Add(item);
                    if (Background == null)
                    {
                        SetBackground(xbmc.GetVfsUri((string)row["fanart"]));
                    }
                }
                DecrementPending();
            });
        }

        private void LoadTvShows()
        {
            var xbmc = Xbmc.Xbmc.instance();
            IncrementPending();
            xbmc.GetTvShows((rows) =>
            {
                foreach (var row in rows)
                {
                    var item = new TvShowItem()
                    {
                        Id = (int)row["tvshowid"],
                        Title = (string)row["title"],
                        Subtitle = (string)row["genre"],
                        Season = 1
                    };
                    item.SetThumbnail(xbmc.GetVfsUri((string)row["thumbnail"]));
                    this.TvShows.Add(item);
                }
                DecrementPending();
            });
        }
        
        private void LoadMovies()
        {
            var xbmc = Xbmc.Xbmc.instance();
            IncrementPending();
            xbmc.GetMovies((rows) =>
            {
                foreach (var row in rows)
                {
                    int movieId = (int)row["movieid"];
                    var item = new MovieItem()
                    {
                        Id = movieId,
                        Title = (string)row["title"],
                        Subtitle = (string)row["tagline"],
                        Play = new PlayCommand((e) => {
                            xbmc.PlayMovie(movieId);
                        })
                    };
                    item.SetThumbnail(xbmc.GetVfsUri((string)row["thumbnail"]));
                    this.Movies.Add(item);
                }
                DecrementPending();
            });
        }

        private void SetBackground(Uri value)
        {
            var newBackground = new ImageBrush() 
            { 
                ImageSource = new BitmapImage(value), 
                Opacity = .8,
                Stretch = Stretch.UniformToFill
            };

            if (newBackground != this.Background)
            {
                this.Background = newBackground;
                NotifyPropertyChanged("Background");
            }
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