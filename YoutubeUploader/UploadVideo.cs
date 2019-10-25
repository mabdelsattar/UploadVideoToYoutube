using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YoutubeUploader.Models;

namespace YoutubeUploader
{
    public class UploadVideo
    {
        public static string url;
        public async Task<string> Run(Info info)
        {
            UserCredential credential;
           // dsAuthorizationBroker.RedirectUri = "http://127.0.0.1:57785/authorize/";

            using (var stream = new FileStream("D:\\client_secrets.json", FileMode.Open, FileAccess.Read))
            {

              //   credential = await dsAuthorizationBroker.AuthorizeAsync(
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the
                    // authenticated user's YouTube channel, but doesn't allow other types of access.
                    new[] { YouTubeService.Scope.YoutubeUpload },
                    "user",
                    CancellationToken.None
                );
            }



            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });

            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = info.title;
            video.Snippet.Description = info.description;
            video.Snippet.Tags = new string[] { "tag1", "tag2" };
            video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "public"; // or "private" or "public"
            var filePath = info.source; // Replace with path to actual movie file.

            using (var client = new WebClient())
            {
                var content = client.DownloadData(filePath);
                using (var stream = new MemoryStream(content))
                {

                    var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", stream, "video/*");
                    videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                    videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                    await videosInsertRequest.UploadAsync();
                    return url;
                }
            }
            return "";
        
        }
        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine("{0} bytes sent.", progress.BytesSent);
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                    break;
            }
        }

        void videosInsertRequest_ResponseReceived(Video video)
        {
            Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
            url = "https://www.youtube.com/watch?v="+video.Id;
        }
    }
}
