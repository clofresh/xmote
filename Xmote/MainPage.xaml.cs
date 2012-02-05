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
using Xmote.Xbmc;

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

        private void Keyboard_KeyUp(object sender, KeyEventArgs e)
        {
            var xbmc = Xbmc.Xbmc.instance();
            Byte[] bytecode = { 0, 0, 0xf1, BitConverter.GetBytes(e.PlatformKeyCode)[0] };
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytecode);
            }
            var intcode = BitConverter.ToInt32(bytecode, 0);
            Debug.WriteLine(String.Format("PlatformKeyCode: {0}, Key: {1}, ASCII: {2}", (int)e.PlatformKeyCode, e.Key, (int)e.Key));
            Debug.WriteLine(String.Format("SendKey({0})", intcode));
            xbmc.SendKey(intcode);
        }
    }
}