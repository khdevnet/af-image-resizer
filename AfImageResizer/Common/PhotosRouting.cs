using System;
using System.Collections.Generic;
using System.Text;

namespace AfImageResizer.Common
{
    public static class PhotosRouting
    {
        public static string Host => $"http://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}/api/";
        private const string Photos = "photos";
        public const string Download = Photos + "/download/{name}/{size:alpha?}";
        public const string Search = Photos + "/search/{searchTerm:alpha?}";
        public const string Upload = Photos + "/upload";
    }
}
