using AutoMapper;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using MultiVendorECommerce.Application.Profiles;
using System.Reflection;

namespace MultiVendorECommerce.Application.Test.Helpers;

public static class MapperTestHelper
{
    public static IMapper GetMapper()
    {
        var loggerFactory = new LoggerFactory();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(UserProfile).Assembly);
        }, loggerFactory);
        return config.CreateMapper();
    }
}