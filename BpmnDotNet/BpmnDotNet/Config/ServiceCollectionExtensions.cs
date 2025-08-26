using System.Reflection;
using BpmnDotNet.Common.Abstractions;
using BpmnDotNet.Handlers;
using BpmnDotNet.Interfaces.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BpmnDotNet.Config;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessProcess(this IServiceCollection services, string pathDiagram)
    {
        services.AddScoped<IPathFinder>(options =>
        {
            var loggerFactory = options.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<PathFinder>();

            return new PathFinder(logger);
        });

        services.AddSingleton<IBpmnClient>(options =>
        {
            var loggerFactory = options.GetRequiredService<ILoggerFactory>();
            var pathFinder = options.GetRequiredService<IPathFinder>();
            return BpmnClientBuilder.Build(pathDiagram, loggerFactory, pathFinder);
        });


        return services;
    }

    /// <summary>
    ///     Регистрация по пространству имен.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="predicate">Filter registered handlers based on type</param>
    /// <typeparam name="THandler"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IServiceCollection AutoRegisterHandlersFromAssemblyOf<THandler>
        (this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var assemblyToRegister = GetAssembly<THandler>();

        RegisterAssembly(services, assemblyToRegister);

        return services;
    }


    private static IEnumerable<Type> GetImplementedHandlerInterfaces(Type type)
    {
        var allInterfaces = type.GetInterfaces();
        var sort = allInterfaces
            .Where(i => i.Name == nameof(IBpmnHandler));

        return sort;
    }


    private static Assembly GetAssembly<THandler>()
    {
        return typeof(THandler).Assembly;
    }

    private static bool IsClass(Type type)
    {
        return !type.IsInterface && !type.IsAbstract;
    }

    private static void RegisterAssembly(
        IServiceCollection services,
        Assembly assemblyToRegister,
        string? namespaceFilter = null)
    {
        var allAssemblyObj = assemblyToRegister.GetTypes();

        var sortClass = allAssemblyObj.Where(IsClass);

        var emplementeds = sortClass
            .Select(type => new
            {
                Type = type,
                ImplementedHandlerInterfaces = GetImplementedHandlerInterfaces(type).ToList()
            });

        var typesToAutoRegister = emplementeds
            .Where(a => a.ImplementedHandlerInterfaces.Any());

        if (!string.IsNullOrEmpty(namespaceFilter))
            typesToAutoRegister = typesToAutoRegister.Where(a =>
                a.Type.Namespace != null && a.Type.Namespace.StartsWith(namespaceFilter));

        foreach (var type in typesToAutoRegister) RegisterType(services, type.Type);
    }

    private static void RegisterType(IServiceCollection services, Type typeToRegister)
    {
        var implementedHandlerInterfaces = GetImplementedHandlerInterfaces(typeToRegister).ToArray();

        foreach (var handlerInterface in implementedHandlerInterfaces)
            services.AddScoped(handlerInterface, typeToRegister);
    }
}