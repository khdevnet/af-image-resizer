using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using AfImageResizer.Models;
using System.Net;
using System.IO;
using Microsoft.Azure.Storage.Blob;
using System.Drawing;
using LazZiya.ImageResize;
using System.Drawing.Imaging;
using System.Collections.Generic;
using AfImageResizer.Common;

namespace AfImageResizer
{
    public static class PhotosResizer
    {
        [FunctionName(nameof(ResizeImages))]
        public static async Task ResizeImages(
        [BlobTrigger(PhotosBlob.Path + "/{name}-" + PhotosCommon.OrgImgSizeKey + ".jpg", Connection = Connections.PhotosStorageConnection)] CloudBlockBlob orgImgBlob,
        [Blob(PhotosBlob.Path, FileAccess.Write, Connection = Connections.PhotosStorageConnection)] CloudBlobContainer originalContainer,
        ILogger logger)
        {
            logger.LogInformation($"Start resize image upload: {orgImgBlob.Name}");

            await originalContainer.CreateIfNotExistsAsync();


            await ResizeImageAndSave(orgImgBlob, originalContainer, PhotosCommon.MdImgSizeKey);
            await ResizeImageAndSave(orgImgBlob, originalContainer, PhotosCommon.SmImgSizeKey);

            logger.LogInformation($"Finish resize image upload: {orgImgBlob.Name}");

        }

        private static async Task ResizeImageAndSave(CloudBlockBlob orgImage, CloudBlobContainer originalContainer, string sizeKey)
        {
            var name = PhotosCommon.GetNameWithSuffix(orgImage.Name, sizeKey);
            var resizedImgBlob = originalContainer.GetBlockBlobReference(name);

            using var orgImageStream = await orgImage.OpenReadAsync();
            using var imgFromStream = Image.FromStream(orgImageStream);
            var imgSize = PhotosCommon.GetNewImgSizes(imgFromStream.Width);
            using var mdImage = imgFromStream.ScaleByWidth(imgSize[sizeKey]);
            using var mdImageWriteStream = await resizedImgBlob.OpenWriteAsync();
            mdImage.Save(mdImageWriteStream, ImageFormat.Jpeg);
            await mdImageWriteStream.CommitAsync();
        }

    }
}
