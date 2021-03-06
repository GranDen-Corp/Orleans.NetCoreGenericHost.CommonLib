# GranDen.Orleans.Server.SharedInterface

Package for Orleans Grain to provide Dependency Injection function interface.

If you have some custom classes need to use in Dependency Injection in Grains project, be sure to implement `IServiceConfigDelegate` interface in a non-grain class, for example:

```cs
public class HelloGrainServiceConfigure : IGrainServiceConfigDelegate
{
    public Action<IApplicationPartManager> AppPartConfigurationAction =>
        (part) =>
        {
            part.AddDynamicPart(typeof(Greeter).Assembly);
        };

    public Action<HostBuilderContext, IServiceCollection> ServiceConfigurationAction =>
        (ctx, service) =>
        {
            service.AddTransient<IGreeter, Greeter>();
        };
}
```

Or you can extend **AbstractServiceConfigDelegate<TGrainClass>** class, that will only need to implement the `ServiceConfigurationAction` lambada function property.

See [example](../../example) projects for usage.
