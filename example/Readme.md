# GranDen Orleans Library Demo

This folder contains demo project for using GranDen Orleans Server & Client project:

- Grain Projects:  

  | Project path  | Purpose |
  | ---------------- | ------------- |
  | Grains\\`GrainDemo` |  A demo grain that shows basic usage and state persistance |
  | RPC_Interface\IGrainDemo | RPC interface for `GrainDemo` grain |
  | Grains\\`GrainWith3rdPartyLib` | A demo grain that has using 3rd party libary ( [Newtonsoft.Json](http://www.nuget.org/packages/Newtonsoft.Json), [MongoDB.Bson](https://www.nuget.org/packages/MongoDB.Bson/) ) |
  | RPC_Interface\IGrainWith3rdPartyLib | RPC interface for `GrainWith3rdPartyLib` grain |

- Localhost In-Memeory hosting:  

  | Project path  | Purpose |
  | ----------------- | ------------- |
  | InRAM\\DemoClient | Orleans Client running on console |
  | InRAM\\HostingDemo | Oreleans Silo Host hosting via [.NET Core Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) |

- Hosting via MongoDB membership store:  

  | Project path  | Purpose |
  | ----------------- | ------------- |
  | MongoDbStore\\MongoDemoConsoleClient | Orleans Client running on console |
  | InRAM\\MongoDemoSiloHost | Oreleans Silo Host hosting via [.NET Core Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) |  
