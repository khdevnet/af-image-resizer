using System.Collections.Generic;
using System.IO;

namespace AfImageResizer.Common
{
    public static class PhotosCommon
    {
        public const string OrgImgSizeKey = "org";
        public const string SmImgSizeKey = "sm";
        public const string MdImgSizeKey = "md";

        public static IEnumerable<string> Sizes = new[] { OrgImgSizeKey, SmImgSizeKey, MdImgSizeKey };

        public static string GetName(string name)
        {
            return Path.GetFileNameWithoutExtension(name);
        }

        public static string GetNameWithSuffix(string name, string suffix, string id = null)
        {
            var names = name.Split('-');

            return names.Length == 1 && !string.IsNullOrEmpty(id)
                ? $"{name}_{id}-{suffix}.jpg"
                : $"{names[0]}-{suffix}{Path.GetExtension(name)}";
        }

        public static Dictionary<string, int> GetNewImgSizes(int width)
        {
            var mdImgSize = (int)width / 2;
            var smImgSize = (int)width / 4;

            return new Dictionary<string, int>() { { PhotosCommon.SmImgSizeKey, smImgSize }, { PhotosCommon.MdImgSizeKey, mdImgSize } };
        }
    }
}
