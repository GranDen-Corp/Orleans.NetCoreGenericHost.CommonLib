# GranDen.Orleans.NetCoreGenericHost.CommonLib

Extension method for easier creating Orleans Silo running on .NET Core Generic Host, default using [Orleans.Providers.MongoDB](https://www.nuget.org/packages/Orleans.Providers.MongoDB) to provide clustering and grain storage.

To use default implmentation, use `OrleansSiloBuilderExtension.CreateHostBuilder()` method.

See [example](../../example) projects for usage.
