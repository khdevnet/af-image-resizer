using System.Text.Json.Serialization;

namespace AfImageResizer.Models
{
    public class UploadImage
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
