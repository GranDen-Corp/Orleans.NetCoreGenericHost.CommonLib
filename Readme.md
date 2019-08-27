# Orleans.NetCoreGenericHost.CommonLib

Libraries for Orleans ([http://dotnet.github.io/orleans](http://dotnet.github.io/orleans)) hosting on [.NET Core Generic Host](http://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) instance.

See [example](./example) projects for usage.

## [GranDen.Orleans.NetCoreGenericHost.CommonLib](./src/GranDen.Orleans.NetCoreGenericHost.CommonLib)

Provide extension method for easier creating Orleans Silo running on .NET Core Generic Host, default using [Orleans.Providers.MongoDB](https://www.nuget.org/packages/Orleans.Providers.MongoDB) to provide cluster hosting and grain storage.

## [GranDen.Orleans.Server.SharedInterface](./src/GranDen.Orleans.Server.SharedInterface)

Package for Orleans Grain to provide Dependency Injection function interface.

## [GranDen.Orleans.NetCoreGenericHost.Serilog](./src/GranDen.Orleans.NetCoreGenericHost.Serilog)

Package for Silo Host usning [Serilog](https://serilog.net/).
