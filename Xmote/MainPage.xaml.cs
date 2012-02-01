using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;

namespace Xmote
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler((sender, e) =>
            {
                if (!App.ViewModel.IsDataLoaded)
                {
                    try
                    {
                        App.ViewModel.LoadData();
                    }
                    catch (NoSettings)
                    {
                        GoToSettings();
                    }
                }
            });
        }

        private void GoToSettings(object sender = null, EventArgs e = null)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
    }
}