using GifStore.Data.ViewModels;
using GifStore.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GifStore.Data.Dtos;
using GifStore.Entities;
using Microsoft.Extensions.Logging;

namespace GifStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ItemsController : ControllerBase
    {
        private const int MAX_RECORD = 12;
        private const long ALLOWED_MAX_SIZE = 10485760; // 10 MB
        private readonly IItemRepository itemRepo;
        private readonly IUserRepository userRepo;
        private readonly ITagRepository tagRepo;
        private readonly IFileManager fileManager;

        public ItemsController(IItemRepository _itemRepo,  IUserRepository _userRepo, 
            ITagRepository _tagRepo, IFileManager _fileManager)
        {
            itemRepo = _itemRepo;
            userRepo = _userRepo;
            tagRepo = _tagRepo;
            fileManager = _fileManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery]int page = 1)
        {
            var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null)
            {
                return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated"});
            }

            int count = await itemRepo.Count(user);
            int skip = (page - 1) * MAX_RECORD;

            var items = await itemRepo.GetAll(skip: skip, take: MAX_RECORD, user: user);

            var responseData = new { CurrentPage = page, PageSize = MAX_RECORD, TotalCount = count, Items = items};
            
            return Ok(new ResponseVM { Status=ResponseStatus.SUCCESS, Message="Items fetched", Data = responseData});
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] SearchQueryInfo searchData)
        {
            int page = searchData.Page;
            string keyword = searchData.Keyword;
            var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null)
            {
                return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });
            }

            int count = await itemRepo.Count(user, keyword);
            int skip = (page - 1) * MAX_RECORD;

            var items = await itemRepo.Search(skip: skip, take: MAX_RECORD, user: user, keyword: keyword);

            var responseData = new { CurrentPage = page, PageSize = MAX_RECORD, TotalCount = count, Items = items };

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Items fetched", Data = responseData });
        }

        [HttpGet("{itemId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid itemId)
        {
            var item = await itemRepo.Get(itemId);
            if (item == null)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "The item is not found" });
            }

            if (! item.IsPublic)
            {
                // check login && check ownership
                var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
                if (user == null)
                {
                    return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });
                }
                if (item.User.Id != user.Id)
                {
                    return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "You do not have permission to access this item" });
                }
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Item fetched", Data = item });
        }

        [HttpPut("{itemId}")]
        public async Task<IActionResult> Update(Guid itemId, SingleFieldDto<string> itemInfo)
        {
            var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null)
            {
                return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });
            }

            var item = await itemRepo.GetRaw(itemId);
            if (item == null || item.UserId != user.Id)    // null item or un-authotised
            {
                return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to modify the item due to missing or permission issue" });
            }

            item.DisplayName = itemInfo.Data;

            var result = await itemRepo.Update(item);

            if (! result)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to modify the item" });
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Item Updated"});
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> Delete(Guid itemId)
        {
            var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null)
            {
                return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });
            }

            var item = await itemRepo.GetRaw(itemId);
            if (item == null || item.UserId != user.Id)    // null item or un-authotised
            {
                return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to delete the item due to missing or permission issue" });
            }

            var fileDeleteResult = await fileManager.Delete(item.PhysicalName);

            if (!fileDeleteResult)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to delete file on disk" });
            }

            var result = await itemRepo.Delete(item);

            if (!result)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to delete" });
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Item deleted" });
        }

        [HttpPost("tag/{itemId}")]
        public async Task<IActionResult> TagItem(Guid itemId, SingleFieldDto<string> tagInfo)
        {
            var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null)  return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });
            
            var item = await itemRepo.GetRaw(itemId);
            if (item == null || item.UserId != user.Id) return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to tag the item due to missing or permission issue" });
            
            var tag = await tagRepo.Get(tagInfo.Data);
            if (tag == null)
            {
                tag = new Tag { Title = tagInfo.Data }; 
                await tagRepo.Save(tag);
            }

            var itemTag = await tagRepo.Get(item.Id, tag.Id);
            if (itemTag != null) return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Item is already tagged" });

            itemTag = new ItemTag { Item = item, Tag = tag };
            var result = await tagRepo.TagItem(itemTag);

            if (!result)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to tag item" });
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Item is tagged", Data = tag.Title });
        }

        [HttpDelete("tag/{itemId}")]
        public async Task<IActionResult> UntagItem(Guid itemId, SingleFieldDto<string> tagInfo)
        {
            var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null) return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });

            var item = await itemRepo.GetRaw(itemId);
            if (item == null || item.UserId != user.Id) return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to tag the item due to missing or permission issue" });

            var tag = await tagRepo.Get(tagInfo.Data);
            if (tag == null) return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Item is not tagged" });

            var itemTag = new ItemTag { Item = item, Tag = tag };
            var result = await tagRepo.UntagItem(itemTag);

            if (!result)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to remove tag" });
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Tag is removed", Data = tag.Title });
        }

        [HttpPut("share/{itemId}")]
        public async Task<IActionResult> ShareItem(Guid itemId, SingleFieldDto<bool> itemInfo)
        {
            var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null) return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });

            var item = await itemRepo.GetRaw(itemId);
            if (item == null || item.UserId != user.Id) return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to change visibility of item due to missing or permission issue" });

            item.IsPublic = itemInfo.Data;
            var result = await itemRepo.Update(item);

            if (!result)
            {
                return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Unable to change item visibility" });
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Item is visibility updated" });
        }

        [HttpPost]
        [RequestSizeLimit(ALLOWED_MAX_SIZE)]
        public async Task<IActionResult> Upload(IFormFile file, CancellationToken token)
        {
            if(file == null) return BadRequest(new ResponseVM { Status = ResponseStatus.ERROR, Message = "File must be selected" });


            var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
            if (user == null) return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });

            // Upload file
            var uploadResult = await fileManager.Upload(user, file, token);

            if (uploadResult.Status == ResponseStatus.ERROR)
            {
                return BadRequest(uploadResult);
            }

            // Save to database
            var item = new Item
            {
                DisplayName = uploadResult.Data.DisplayName,
                PhysicalName = uploadResult.Data.PhysicalName,
                User = user
            };

            var itemViewModel = await itemRepo.Save(item);

            if (itemViewModel == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ResponseVM { Status = ResponseStatus.ERROR, Message = "An error occurred while uploading" });
            }

            return Ok(new ResponseVM { Status = ResponseStatus.SUCCESS, Message = "Item is saved", Data = itemViewModel });

        }

        
        [HttpGet("files/{filename}")]
        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetFile(string filename)
        {
            var item = await itemRepo.GetRawByFilename(filename);

            if (item == null) return NotFound(new ResponseVM { Status = ResponseStatus.ERROR, Message = "Image file not found" });

            
            if (!item.IsPublic)
            {
                // check login && check ownership
                var user = await userRepo.Get(HttpContext.User.FindFirstValue(ClaimTypes.Email));
                if (user == null)
                {
                    return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "User is not authenticated" });
                }
                if(item.UserId != user.Id)
                {
                    return Unauthorized(new ResponseVM { Status = ResponseStatus.ERROR, Message = "You do not have permission to access this item" });
                }
            }

            var result = await fileManager.Download(filename);
            if (result.Status == ResponseStatus.ERROR)
            {
                return BadRequest(result);
            }
            
            return PhysicalFile(result.Data.Path, result.Data.Type);


        }


    }
}
