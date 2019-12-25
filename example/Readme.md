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
  | MongoDbStore\\MongoDemoSiloHost | Oreleans Silo Host hosting via [.NET Core Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) |  

- Hosting via MySQL membership store:

  | Project path  | Purpose |
  | ----------------- | ------------- |
  | MYSQL\\MySqlDemoClient | Orleans Client running on console |
  | MYSQL\\MySqlDemoHost | Oreleans Silo Host hosting via [.NET Core Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) |  

- Hosting via MS SQL Server membership store:

  | Project path  | Purpose |
  | ----------------- | ------------- |
  | SQLDB\\SqlDbDemoClient | Orleans Client running on console |
  | SQLDB\\SqlDbDemoHost | Oreleans Silo Host hosting via [.NET Core Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) |  

- Demo using [Orleans code generation when start-up](http://dotnet.github.io/orleans/Documentation/grains/code_generation.html#during-initialization):

  | Project path  | Purpose |
  | ----------------- | ------------- |
  | stub_codegen\\netstandard_mixed\\HelloNetStandard.ShareInterface | Shared Orleans RPC Interface project |
  | stub_codegen\\client\\NetStandard2ClientLib | .NET Standard 2.0 RPC caller library |
  | stub_codegen\\client\\StubCodeGenDemoClient | Orleans Client running on console |
  | stub_codegen\\backend\\HelloNetStandard2_1.Grains | .NET Standard 2.1 Orleans grain project | 
  | stub_codegen\\backend\\StubCodeGenLocalSiloHost | Oreleans Silo Host hosting via [.NET Core Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) |  
