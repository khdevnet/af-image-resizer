using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using AfImageResizer.Models;

namespace AfImageResizer
{
    public static class ImageUploader
    {
        [FunctionName(nameof(UploadImage))]
        public static async Task<IActionResult> UploadImage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] UploadImage uploadImage,
            ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");


            return new OkObjectResult(Guid.NewGuid());
        }
    }
}
