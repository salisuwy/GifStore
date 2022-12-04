# GifStore REST API

## Overview
- This RESTful API is developed in ASP.NET Core and SQLite. 
- The API allows authenticated users to upload GIF images. 
- Uploaded items are by private by default and can only be accessed by the owner. 
- Owners can change the access of their items to public so that other users can view

## Demo

https://user-images.githubusercontent.com/8425466/205523031-4ab9a54a-0300-4d4c-a59e-2a6686e0610e.mp4


## Frontend
- The frontend is built in **React**. The complete code can be found in [GifStore React App](https://github.com/salisuwy/gifstore-react-app)

## Set Up
- Clone the repo and execute the project. The database is already seeded with random data and images using **Bogus** faker
- To easy with the demo, two files are provided
  - RestClient.http: You need to install the HttpClient extension for vscode or visual studio to use this file
  - RestPostman.json: You can import this into Postman to access the endpoints 
  
## Endpoint Guides:
- Five user accounts are already registered with seeded data. Check the GifStore/Utilities/DatabaseSeeder.cs 
- You can register and account if you want or login with one of the users (email: user1@test.com ==> password: Password1)
- Login should generate a Json Web Token (JWT)
- Protected endpoints needs JWT added o the header of the request ("Authorisation": "Bearer <token>)

## CORS and Frontend Access
- To allow access to your SPA or via any frontend, you need to allow CORS access to the SPA domain. You can do this in two (2) ways:
  - appsettings.json (Recommended): Modify the CORS:Domain entry in the appsettings.json to match you app domain
  - Program.cs: Add the domain entries in the AddCors middleware in the Program.cs file
  
## Re-seeding data and Changing Database Engine
- SQLite may not be the ideal database for a production application. To switch the database engine,
  - Add the DB connection string to appsettings.json or modify the existing one
  - If new entry is added, you need to alter the setting in Program.cs
- To re-seed the data, send an OPTIONS request to the endpoint "api/users/seed-data". It should be easy using swagger  

## Changing Filestorage mechanism
- The IFileManager interface can be implemented. I only provide an implementation for LocalFileManager which saves the files locally. AzureStorage, S3 of Google file storage can be implemented
