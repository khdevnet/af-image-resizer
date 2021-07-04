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
using AfImageResizer.Common;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AfImageResizer
{
    public static class PhotosStorage
    {
        [FunctionName(nameof(UploadImage))]
        public static async Task<HttpResponseMessage> UploadImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = PhotosRouting.Upload)] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var body = await req.Content.ReadAsStringAsync();

            var uploadImage = JsonConvert.DeserializeObject<UploadImage>(body);

            string instanceId = await starter.StartNewAsync(nameof(PhotosTransaction), uploadImage);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(nameof(PhotosTransaction))]
        public static async Task<string> PhotosTransaction(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var model = context.GetInput<UploadImage>();
            //TODO: make idempotence
            var metadata = await context.CallActivityAsync<ImageMetadata>(nameof(SaveToStorage), model);
            var id = await context.CallActivityAsync<string>(nameof(SaveToDb), metadata);
            return id;
        }

        [FunctionName(nameof(SaveToStorage))]
        public static async Task<ImageMetadata> SaveToStorage(
            [ActivityTrigger] UploadImage reqImage,
            [Blob(PhotosBlob.Path, FileAccess.ReadWrite, Connection = Connections.PhotosStorageConnection)] CloudBlobContainer originalContainer,
            ILogger logger)
        {
            var id = Guid.NewGuid().ToString("N");
            var blobName = PhotosCommon.GetNameWithSuffix(reqImage.Name, PhotosCommon.OrgImgSizeKey, id);

            logger.LogInformation($"Start image upload id: {id} name: {blobName}");

            await originalContainer.CreateIfNotExistsAsync();

            using (var imageStream = await new WebClient().OpenReadTaskAsync(new Uri(reqImage.ImageUrl)))
            {
                var blobImage = originalContainer.GetBlockBlobReference(blobName);
                await blobImage.UploadFromStreamAsync(imageStream);
            }

            logger.LogInformation($"Finish image upload id: {id} name: {blobName}");
            return new ImageMetadata
            {
                ImageId = id,
                Name = reqImage.Name,
                Description = reqImage.Description,
                ImageUrl = reqImage.ImageUrl
            };
        }

        [FunctionName(nameof(SaveToDb))]
        public static async Task<string> SaveToDb(
           [ActivityTrigger] ImageMetadata metadata,
           [CosmosDB(PhotosDatabase.Name, ImageMetadata.CollectionName, ConnectionStringSetting = Connections.CosmosDbConnection, CreateIfNotExists = true)] IAsyncCollector<ImageMetadata> imagesMetadata,
           ILogger logger)
        {
            logger.LogInformation($"Start image saving id: {metadata.ImageId} name: {metadata.Name}");

            await imagesMetadata.AddAsync(metadata);

            logger.LogInformation($"Finish image saving id: {metadata.ImageId} name: {metadata.Name}");

            return metadata.ImageId;
        }
    }
}
