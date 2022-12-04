using GifStore.Data.ViewModels;
using GifStore.Entities;
using Microsoft.Extensions.Logging;

namespace GifStore.Repositories
{
    public class LocalFileManager : IFileManager
    {
        private readonly ILogger<LocalFileManager> logger;
        private readonly IWebHostEnvironment env;
        private readonly IConfiguration config;

        public LocalFileManager(ILogger<LocalFileManager> _logger, IWebHostEnvironment _env, IConfiguration _config)
        {
            logger = _logger;
            env = _env;
            config = _config;
        }
        public async Task<ResponseVM> Upload(User user, IFormFile file, CancellationToken token)
        {
            logger.LogCritical("Uploaded started");
            if (file.ContentType != "image/gif")
                return new ResponseVM{ Status = ResponseStatus.ERROR, Message = "Only GIF files are allowed"};

            var generatedFilename = MD5Hash.Hash.Content(
                file.FileName + user.Id.ToString() + Guid.NewGuid().ToString() + DateTime.Now.Ticks.ToString());
            var fileExtension = file.FileName.Substring(file.FileName.LastIndexOf("."));
            var displayName = file.FileName.Substring(0, file.FileName.LastIndexOf("."));
            var filesDirectory = config.GetSection("LocalFiles:Folder").Value;

            var filePath = Path.Combine(env.ContentRootPath, filesDirectory, generatedFilename + fileExtension);
            try
            {
                await file.CopyToAsync(new FileStream(filePath, FileMode.Create), token);
                var response = new ResponseVM { 
                                    Status = ResponseStatus.SUCCESS, 
                                    Message = "File uploaded successfully", 
                                    Data = new
                                    {
                                        DisplayName = displayName, 
                                        PhysicalName = generatedFilename
                                    }
                                };
                logger.LogCritical("Uploaded ended");
                                
                return response;
            }
            catch (Exception exception)
            {
                logger.LogCritical($"Exception while saving file {exception.Message}");
                return new ResponseVM { Status = ResponseStatus.ERROR, Message = "An error occurred while uploading file" };
            }
        }


        public async Task<ResponseVM> Download(string filename)
        {
            var filesDirectory = config.GetSection("LocalFiles:Folder").Value;
            var fileExtension = ".gif";
            var filePath = Path.Combine(env.ContentRootPath, filesDirectory, filename + fileExtension);

            if (!File.Exists(filePath))
            {
                return new ResponseVM { Status = ResponseStatus.ERROR, Message = "File does not exist" };
            }

            var data = new { Path = filePath, Type = "image/gif" };

            return new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "File fetched", Data = data };

        }

        public async Task<bool> Delete(string filename)
        {
            var filesDirectory = config.GetSection("LocalFiles:Folder").Value;
            var fileExtension = ".gif";
            var filePath = Path.Combine(env.ContentRootPath, filesDirectory, filename + fileExtension);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch (Exception exception)
                {
                    logger.LogCritical($"Exception while saving file {exception.Message}");
                    return false;
                }
            }
            return true;
        }
    }
}
