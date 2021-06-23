using AfImageResizer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfImageResizer.Models
{
    public class PhotoResponseModel : ImageMetadata, IResponseNavigation
    {
        public dynamic GetNavigation()
        {
            var format = PhotosRouting.Host + 
                PhotosRouting.Download
                .Replace("{name}", "{0}")
                .Replace("{size:alpha?}", "{1}");

            return PhotosCommon.Sizes.ToDictionary(s => s, s => string.Format(format, $"{Name}_{ImageId}", s));
        }
    }

    public class ResponseItem<TModel> where TModel : IResponseNavigation
    {
        public TModel Data { get; set; }
        public dynamic Navigation => Data.GetNavigation();
    }

    public interface IResponseNavigation
    {
        dynamic GetNavigation();
    }
}
