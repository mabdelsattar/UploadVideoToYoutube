using System;
using System.Collections.Generic;
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

namespace YoutubeUploader.Controllers
{
    public class HomeController : ApiController
    {
        
        // GET: api/Home
        [HttpPost]
        public async Task<string> UploadVideo([FromBody] Info info)               
        {
            string distination= "";
            try
            {
              distination = await new UploadVideo().Run(info);
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }

            return distination;
        }

      

    }
}
