using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Xmote.Xbmc;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Xmote
{
    public class TvShowsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TvSeasonItem> Seasons { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageBrush Background { get; private set; }
        private void SetBackground(Uri value)
        {
            var newBackground = new ImageBrush() { ImageSource = new BitmapImage(value), Opacity = .8 };
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

        public bool IsDataLoaded { get; private set; }
        public void LoadData(Pivot TvPivot, int tvShowId, int seasonNum)
        {
            Seasons = new ObservableCollection<TvSeasonItem>();
            var xbmc = Xbmc.Xbmc.instance();
            xbmc.GetTvSeasons(tvShowId, (rows) =>
            {
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var num = (int)row["season"];
                        var season = new TvSeasonItem() 
                        {
                            Title = String.Format("Season {0}", num)
                        };

                        xbmc.GetTvEpisodes(tvShowId, num, (rows2) =>
                        {
                            if (rows2 != null)
                            {
                                foreach (var row2 in rows2)
                                {
                                    int episodeId = (int)row2["episodeid"];
                                    var item = new TvEpisodeItem()
                                    {
                                        Id = episodeId,
                                        Title = (string)row2["title"],
                                        Subtitle = (string)row2["showtitle"],
                                        SortKey = (string)row2["firstaired"],
                                        Play = new PlayCommand((e) =>
                                        {
                                            xbmc.PlayEpisode(episodeId);
                                        })
                                    };
                                    item.SetThumbnail(xbmc.GetVfsUri((string)row2["thumbnail"]));
                                    season.Episodes.Add(item);
                                    if (Background == null)
                                    {
                                        SetBackground(xbmc.GetVfsUri((string)row2["fanart"]));
                                    }
                                    season.Episodes.Add(item);
                                }
                            }
                            Seasons.Add(season);
                            NotifyPropertyChanged("Seasons");
                        });
                    }
                }
            });
        }    
    }

    public partial class TvShowsPage : PhoneApplicationPage
    {

        public TvShowsPage()
        {
            InitializeComponent();
            var ViewModel = new TvShowsViewModel();
            DataContext = ViewModel;
            this.Loaded += new RoutedEventHandler((sender, e) =>
            {
                if (!ViewModel.IsDataLoaded)
                {
                    ViewModel.LoadData(
                        TvPivot,
                        Convert.ToInt32(this.NavigationContext.QueryString["Id"]), 
                        Convert.ToInt32(this.NavigationContext.QueryString["Season"])
                    );
                }
            });
        }
    }
}