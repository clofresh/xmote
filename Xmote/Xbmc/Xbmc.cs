using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Xmote.Xbmc
{
    public delegate void Callback(JToken response);
    public delegate void Errback(Exception e);

    public class SortOrder
    {
        public const string Ascending = "ascending";
        public const string Descending = "descending";
    }

    public class SortMethod
    {
        public const string None = "none";
        public const string Label = "label";
        public const string Date = "date";
        public const string Size = "size";
        public const string File = "file";
        public const string DriveType = "drivetype";
        public const string Track = "track";
        public const string Duration = "duration";
        public const string Title = "title";
        public const string Artist = "artist";
        public const string Album = "album";
        public const string Genre = "genre";
        public const string Year = "year";
        public const string VideoRating = "videorating";
        public const string ProgramCount = "programcount";
        public const string Playlist = "playlist";
        public const string Episode = "episode";
        public const string VideoTitle = "videotitle";
        public const string SortTitle = "sorttitle";
        public const string ProductionCode = "productioncode";
        public const string SongRating = "songrating";
        public const string MpaaRating = "mpaarating";
        public const string VideoRuntime = "videoruntime";
        public const string Studio = "studio";
        public const string FullPath = "fullpath";
        public const string LastPlayed = "lastplayed";
        public const string Unsorted = "unsorted";
        public const string Max = "max";
    }

    public class VideoField
    { }


    public class Limits
    {
        public int start;
        public int end;
    }

    public class Sort
    {
        public string order;
        public bool ignorearticle;
        public string method;
    }

    public class XbmcException : Exception { }
    public class InvalidHost : XbmcException { }
    public class InvalidPort : XbmcException { }
    public class InvalidUserPassword : XbmcException { }

    public class Xbmc {
        private string ipAddress;
        private int    port;
        private string userName;
        private string password;

        public static Xbmc instance()
        {
            return Xbmc.FromSettings(SettingsPage.ReadSettings());
        }

        public static Xbmc FromSettings(Settings settings)
        {
            if (settings.Host == null || settings.Host == "") throw new InvalidHost();
            if (settings.Port == null || settings.Port == 0) throw new InvalidPort();
            if (settings.User == null && settings.Password != null) throw new InvalidUserPassword();

            return new Xbmc(settings.Host, settings.Port, settings.User, settings.Password);
        }

        public Xbmc(string ipAddress, int port, string userName, string password)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.userName = userName;
            this.password = password;
        }

        public Uri GetSendKeyUri(int key)
        {
            return new Uri(String.Format("http://{0}:{1}/xbmcCmds/xbmcHttp?command=Sendkey({2})", ipAddress, port, key));
        }

        public Uri GetUri()
        {
            return new Uri(String.Format("http://{0}:{1}/jsonrpc", this.ipAddress, this.port));
        }

        public Uri GetVfsUri(string uriString)
        {
            return new Uri(String.Format("http://{0}:{1}@{2}:{3}/vfs/{4}", userName, password, ipAddress, port, uriString));
        }


        private WebClient RpcRequest(string method, List<object> args, Callback callback, Errback errback)
        {
            var client = new WebClient();
            var uri = GetUri();
            client.Credentials = new NetworkCredential(this.userName, this.password);
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            client.UploadStringCompleted += (sender, e) =>
            {
                try
                {
                    var response = JObject.Parse(e.Result);
                    
                    if (callback != null)
                    {
                        callback(response);
                    }
                }
                catch (WebException exception)
                {
                    if (errback != null)
                    {
                        errback(exception);
                    }
                    else
                    {
                        Debug.WriteLine("Error (" + uri + "): " + method);
                        Debug.WriteLine(exception);
                    }
                }
            };
            var body = BuildRequest(method, args);
            Debug.WriteLine("Request: " + uri + " | " + method + " " + body);
            client.UploadStringAsync(uri, body); 
            return client;
        }

        public void GetMovies(Callback callback, Errback errback = null, List<string> properties = null, Limits limits = null, Sort sort = null)
        {
            if (properties == null)
            {
                properties = new List<string> { "title", "tagline", "thumbnail" };
            }

            if (limits == null)
            {
                limits = new Limits { start = 0, end = 100 };
            }

            if (sort == null)
            {
                sort = new Sort
                {
                    ignorearticle = true,
                    order = SortOrder.Descending,
                    method = SortMethod.Title
                };
            }
                        
            var args = new List<object> { properties, limits, sort };
            RpcRequest("VideoLibrary.GetMovies", args, (data) =>
            {
                callback(data.SelectToken("result.movies"));
            }, errback);
        }

        public void GetTvShows(Callback callback, Errback errback = null, List<string> properties = null, Limits limits = null, Sort sort = null)
        {
            if (properties == null)
            {
                properties = new List<string> { "title", "genre", "thumbnail" };
            }

            if (limits == null)
            {
                limits = new Limits { start = 0, end = 100 };
            }

            if (sort == null)
            {
                sort = new Sort
                {
                    ignorearticle = true,
                    order = SortOrder.Ascending,
                    method = SortMethod.Title
                };
            }

            var args = new List<object> { properties, limits, sort };
            RpcRequest("VideoLibrary.GetTVShows", args, (data) =>
            {
                callback(data.SelectToken("result.tvshows"));
            }, errback);
        }

        public void GetTvSeasons(int tvShowId, Callback callback, Errback errback = null, List<string> properties = null, Limits limits = null, Sort sort = null)
        {
            if (properties == null)
            {
                properties = new List<string> { "showtitle", "season", "fanart", "thumbnail", "episode", "playcount" };
            }

            if (limits == null)
            {
                limits = new Limits { start = 0, end = 100 };
            }

            if (sort == null)
            {
                sort = new Sort
                {
                    ignorearticle = true,
                    order = SortOrder.Ascending,
                    method = SortMethod.Year
                };
            }

            var args = new List<object> { tvShowId, properties, limits, sort };
            RpcRequest("VideoLibrary.GetSeasons", args, (data) =>
            {
                callback(data.SelectToken("result.seasons"));
            }, errback);
        }

        public void GetTvEpisodes(int tvShowId, int season, Callback callback, Errback errback = null, List<string> properties = null, Limits limits = null, Sort sort = null)
        {
            if (properties == null)
            {
                properties = new List<string> { "title", "showtitle", "fanart", "thumbnail", "firstaired" };
            }

            if (limits == null)
            {
                limits = new Limits { start = 0, end = 100 };
            }

            if (sort == null)
            {
                sort = new Sort
                {
                    ignorearticle = true,
                    order = SortOrder.Ascending,
                    method = SortMethod.Episode
                };
            }
            var args = new List<object> { tvShowId, season, properties, limits, sort };
            RpcRequest("VideoLibrary.GetEpisodes", args, (data) =>
            {
                callback(data.SelectToken("result.episodes"));
            }, errback);
        }

        public void GetRecentlyAddedEpisodes(Callback callback, Errback errback = null, List<string> properties = null, Limits limits = null, Sort sort = null)
        {
            if (properties == null)
            {
                properties = new List<string> { "title", "showtitle", "fanart", "thumbnail", "firstaired" };
            }

            if (limits == null)
            {
                limits = new Limits { start = 0, end = 100 };
            }

            if (sort == null)
            {
                sort = new Sort
                {
                    ignorearticle = true,
                    order = SortOrder.Descending,
                    method = SortMethod.Date
                };
            }

            var args = new List<object> { properties, limits, sort };
            RpcRequest("VideoLibrary.GetRecentlyAddedEpisodes", args, (data) =>
            {
                callback(data.SelectToken("result.episodes"));
            }, errback);
        }

        public void PlayMovie(int movieId, Callback callback = null, Errback errback = null)
        {
            var args = new List<object> { new Dictionary<string, object>() { {"movieid", movieId} } };
            RpcRequest("Player.Open", args, callback, errback);
        }

        public void PlayEpisode(int episodeId, Callback callback = null, Errback errback = null)
        {
            var args = new List<object> { new Dictionary<string, object>() { { "episodeid", episodeId } } };
            RpcRequest("Player.Open", args, callback, errback);
        }

        public void SendKey(int key)
        {
            var client = new WebClient();
            client.Credentials = new NetworkCredential(this.userName, this.password);
            client.DownloadStringCompleted += (sender, e) =>
            {
                try
                {
                    Debug.WriteLine(String.Format("Send key {0}", key));
                }
                catch (WebException exception)
                {
                    Debug.WriteLine(String.Format("Send key {0}", key));
                }
            };
            client.DownloadStringAsync(GetSendKeyUri(key));
        }

        static string BuildRequest(string method, List<object> args)
        {
            var request = new Dictionary<string, object>() {
                {"id", 1},
                {"jsonrpc", "2.0"},
                {"method",  method},
                {"params", args}
            };

            return JsonConvert.SerializeObject(request);
        }
    }
}


