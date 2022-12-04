using GifStore.Data;
using GifStore.Entities;
using Microsoft.AspNetCore.Identity;
using Bogus;

namespace GifStore.Utilities
{
    public class DatabaseSeeder
    {
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment env;
        private readonly IPasswordHasher<User> passwordHarsher;
        private readonly GifStoreDbContext db;

        public DatabaseSeeder(IConfiguration _config, 
            IWebHostEnvironment _env, 
            IPasswordHasher<User> _passwordHarsher, 
            GifStoreDbContext _db)
        {
            config = _config;
            env = _env;
            passwordHarsher = _passwordHarsher;
            db = _db;
        }


        public async Task Run()
        {
            var faker = new Faker();
            var rng = new Random();

            // Seed users
            var users = new List<User>
            {
                new User { Fullname=faker.Name.FullName(), Email="user1@test.com" },
                new User { Fullname=faker.Name.FullName(), Email="user2@test.com" },
                new User { Fullname=faker.Name.FullName(), Email="user3@test.com" },
                new User { Fullname=faker.Name.FullName(), Email="user4@test.com" },
                new User { Fullname=faker.Name.FullName(), Email="user5@test.com" }
            };
            users[0].PasswordHash = passwordHarsher.HashPassword(users[0], "Password1");
            users[1].PasswordHash = passwordHarsher.HashPassword(users[1], "Password2");
            users[2].PasswordHash = passwordHarsher.HashPassword(users[2], "Password3");
            users[3].PasswordHash = passwordHarsher.HashPassword(users[3], "Password4");
            users[4].PasswordHash = passwordHarsher.HashPassword(users[4], "Password5");
            db.Users.AddRange(users);
            await db.SaveChangesAsync();

            // Seed tags
            var tags = new List<Tag>
            {
                new Tag { Title = "nature"},
                new Tag { Title = "animals"},
                new Tag { Title = "japan"},
                new Tag { Title = "dubai"},
                new Tag { Title = "zip-it"},
                new Tag { Title = "calling-home"},
                new Tag { Title = "daddy's-cuties"},
                new Tag { Title = "taser"}
            };
            db.Tags.AddRange(tags);
            await db.SaveChangesAsync();

            // Creating the items: FOR-EACH user
            var itemsList = new List<Item>();
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];

                var numItems = i == 0 ? 35 : rng.Next(3, 25);     // 35 items for user[0] and between 3-25 for other users

                for (int j = 0; j < numItems; j++)
                {
                    var item = new Item
                    {
                        DisplayName = faker.Name.Random.Words(rng.Next(1, 5)),
                        PhysicalName = faker.Name.Random.Hash(rng.Next(20, 40)),
                        User = user
                    };
                    itemsList.Add(item);
                }
            }
            db.Items.AddRange(itemsList);
            await db.SaveChangesAsync();

            
            // Link Items to tags
            var itemTagsList = new List<ItemTag>();
            for (int i = 0; i < itemsList.Count; i++)
            {
                var item = itemsList[i];
                var numOfTags = rng.Next(0, tags.Count);
                for (int j = 0; j < numOfTags; j++)
                {
                    var tag = tags[j];
                    var itemTag = new ItemTag { Item = item, Tag = tag };
                    itemTagsList.Add(itemTag);
                }
            }

            db.ItemTags.AddRange(itemTagsList);
            await db.SaveChangesAsync();
            
            
            // Generate images for all items and save to folder
            var filesDirectory = config.GetSection("LocalFiles:Folder").Value;
            var fileExtension = ".gif";
            var httpClient = new HttpClient();

            foreach (var item in itemsList)
            {
                var imageUrl = faker.Image.LoremFlickrUrl(600, 550);
                var stream = await httpClient.GetStreamAsync(imageUrl);
                var filePath = Path.Combine(env.ContentRootPath, filesDirectory, item.PhysicalName + fileExtension);
                using (FileStream outputFileStream = new FileStream(filePath, FileMode.Create)) {  
                    await stream.CopyToAsync(outputFileStream);  
                }
            }
            
            await Task.CompletedTask;
        }
    }
}
