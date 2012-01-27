using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Xmote.Xbmc
{
    public delegate void Callback(JObject response);
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


    public class Xbmc {
        private string ipAddress;
        private int    port;
        private string userName;
        private string password;

        public Xbmc(string ipAddress, int port, string userName, string password)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            this.userName = userName;
            this.password = password;
        }

        public Uri GetUri()
        {
            return new Uri(String.Format("http://{0}:{1}/jsonrpc", this.ipAddress, this.port));
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

        public void GetMovies(List<string> properties, Limits limits, Sort sort, Callback callback, Errback errback = null)
        {
            var args = new List<object> { properties, limits, sort };
            RpcRequest("VideoLibrary.GetMovies", args, callback, errback);
        }

        public void GetTvShows(List<string> properties, Limits limits, Sort sort, Callback callback, Errback errback = null)
        {
            var args = new List<object> { properties, limits, sort };
            RpcRequest("VideoLibrary.GetTVShows", args, callback, errback);
        }

        public void GetRecentlyAddedMovies(List<string> properties, Limits limits, Sort sort, Callback callback, Errback errback = null)
        {
            var args = new List<object> { properties, limits, sort };
            RpcRequest("VideoLibrary.GetRecentlyAddedMovies", args, callback, errback);
        }

        public void GetRecentlyAddedEpisodes(List<string> properties, Limits limits, Sort sort, Callback callback, Errback errback = null)
        {
            var args = new List<object> { properties, limits, sort };
            RpcRequest("VideoLibrary.GetRecentlyAddedEpisodes", args, callback, errback);
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


