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
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;

namespace Xmote
{
    public class NoSettings : Exception { }

    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            
            Settings settings;
            try
            {
                settings = ReadSettings();
            }
            catch (NoSettings)
            {
                settings = new Settings();
            }
            this.ConnectionName.Text = settings.ConnectionName;
            this.Host.Text = settings.Host;
            this.Port.Text = Convert.ToString(settings.Port);
            this.User.Text = settings.User;
            this.Password.Password = settings.Password;

        }

        private void Save(object sender, EventArgs e)
        {
            // Instantiate a new Settings object from the form data
            var settings = new Settings()
            {
                ConnectionName = this.ConnectionName.Text,
                Host = this.Host.Text,
                Port = Convert.ToInt32(this.Port.Text),
                User = this.User.Text,
                Password = this.Password.Password
            };
            WriteSettings(settings);
            App.ViewModel.IsDataLoaded = false;
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }

        private void WriteSettings(Settings settings)
        {
            // Serialize the Settings object and write to isolated storage
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var file = new IsolatedStorageFileStream("settings.json", System.IO.FileMode.OpenOrCreate, store))
                {
                    settings.Serialize(file);
                }
            }
        }

        public static Settings ReadSettings()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Stream file = null;
                try
                {
                    file = store.OpenFile("settings.json", FileMode.Open);
                    return Settings.Deserialize(file);
                }
                catch (IsolatedStorageException)
                {
                    throw new NoSettings();
                }
                finally
                {
                    if (file != null)
                    {
                        file.Dispose();
                    }
                }
            }
        }

    }

    public class Settings
    {
        public string ConnectionName;
        public string Host;
        public int Port;
        public string User;
        public string Password;

        public Settings()
        {
            ConnectionName = "";
            Host = "";
            Port = 80;
            User = "";
            Password = "";
        }

        public static Settings Deserialize(Stream file)
        {
            var serializer = Settings.Serializer();
            return (Settings)serializer.ReadObject(file);
        }
        
        public static DataContractJsonSerializer Serializer()
        {
            return new DataContractJsonSerializer(typeof(Settings));
        }

        public override string ToString()
        {
            return String.Format("{0}: xbmc://{1}:{2}@{3}:{4}", ConnectionName, Host, Port, User, Password);
        }

        public void Serialize(Stream file)
        {
            var serializer = Settings.Serializer();
            serializer.WriteObject(file, this);
        }
    }
}