using System;
using System.Collections.Generic;
using System.Text;

namespace AfImageResizer.Models
{
    public class ImageMetadata
    {
        public const string CollectionName = "metadata";

        public string ImageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
