using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using AfImageResizer.Models;
using System.IO;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Azure.Documents.Linq;
using AfImageResizer.Common;

namespace AfImageResizer
{
    public static class PhotosCatalog
    {
        [FunctionName(nameof(Download))]
        public static async Task<FileContentResult> Download(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = PhotosRouting.Download)] HttpRequest req, string name, string size = Common.PhotosCommon.OrgImgSizeKey,
            [Blob(PhotosBlob.Path + "/{name}-{size}.jpg", FileAccess.Read, Connection = Connections.PhotosStorageConnection)] Stream imgStream = default,
            ILogger logger = default)
        {
            logger.LogInformation($"Start download: {name}-{size}");

            var imgBytes = new byte[imgStream.Length];

            await imgStream.ReadAsync(imgBytes, 0, (int)imgStream.Length);

            logger.LogInformation($"Finish download: {name}-{size}");

            return new FileContentResult(imgBytes, "image/jpeg")
            {
                FileDownloadName = $"{name}-{size}.jpg"
            };
        }

        [FunctionName(nameof(Search))]
        public static async Task<IActionResult> Search(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = PhotosRouting.Search)] string searchTerm,
            [CosmosDB(PhotosDatabase.Name, ImageMetadata.CollectionName, ConnectionStringSetting = Connections.CosmosDbConnection)] DocumentClient client,
            ILogger logger)
        {
            logger?.LogInformation("Searching...");

            var collectionUri = UriFactory.CreateDocumentCollectionUri("photos", "metadata");

            var query = string.IsNullOrWhiteSpace(searchTerm)
                ? GetCollection(client, collectionUri)
                : GetCollection(client, collectionUri).Where(p => p.Description.Contains(searchTerm));

            var photos = await ReadPhotos(query.AsDocumentQuery());

            return new OkObjectResult(photos);
        }

        private static IOrderedQueryable<ImageMetadata> GetCollection(DocumentClient client, System.Uri collectionUri)
        {
            return client.CreateDocumentQuery<ImageMetadata>(
                collectionUri,
                new FeedOptions() { EnableCrossPartitionQuery = true });
        }

        private static async Task<List<ResponseItem<PhotoResponseModel>>> ReadPhotos(IDocumentQuery<ImageMetadata> query)
        {
            var results = new List<ResponseItem<PhotoResponseModel>>();

            while (query.HasMoreResults)
            {
                foreach (var result in await query.ExecuteNextAsync<ImageMetadata>())
                {
                    results.Add(new ResponseItem<PhotoResponseModel>
                    {
                        Data = new PhotoResponseModel
                        {
                            ImageId = result.ImageId,
                            Name = result.Name,
                            ImageUrl = result.ImageUrl,
                            Description = result.Description
                        }
                    });
                }
            }

            return results;
        }
    }
}
