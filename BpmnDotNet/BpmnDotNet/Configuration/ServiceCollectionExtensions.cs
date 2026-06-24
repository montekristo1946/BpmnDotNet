using BpmnDotNet.BpmnEngineDomain.Abstractions;
using BpmnDotNet.BpmnEngineDomain.Handlers;
using BpmnDotNet.BpmnValidator;
using BpmnDotNet.BpmnValidator.Abstractions;
using BpmnDotNet.ClientDomain.Abstractions;
using BpmnDotNet.HistoryDomain.Abstractions;

namespace BpmnDotNet.Configuration;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BpmnDotNet.Abstractions.Handlers;
using BpmnDotNet.ElasticClientDomain.Abstractions;
using BpmnDotNet.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Extensions BpmnDotNet.
/// </summary>
[UnconditionalSuppressMessage("Trimming", "IL2072:AssemblyGetTypesTrimming")]
[UnconditionalSuppressMessage("Trimming", "IL2026:AssemblyGetTypesTrimming")]
[UnconditionalSuppressMessage("Trimming", "IL2098:AssemblyGetTypesTrimming")]
[UnconditionalSuppressMessage("Trimming", "IL2067:AssemblyGetTypesTrimming")]
[UnconditionalSuppressMessage("Trimming", "IL2070:AssemblyGetTypesTrimming")]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Зарегистрирует IBpmnClient.
    /// </summary>
    /// <param name="services">IServiceCollection.</param>
    /// <param name="pathDiagram">Путь до диаграмм.</param>
    /// <returns>Коллекцию сервисов.</returns>
    public static IServiceCollection AddBusinessProcess(this IServiceCollection services, string pathDiagram)
    {
        services.AddSingleton<IHistoryNodeStateWriter, HistoryNodeStateWriter>();
        services.AddSingleton<IDescriptionWriteService, DescriptionWriteService>();
        services.AddSingleton<IXmlSerializationProcessSection, XmlSerializationProcessSection>();
        services.AddSingleton<IXmlSerializationBpmnDiagramSection, XmlSerializationBpmnDiagramSection>();
        services.AddSingleton<IProcessModelBuilder, ProcessModelBuilder>();
        services.AddSingleton<ICheckBpmnProcessDto, CheckBpmnProcessDto>();

        services.AddSingleton<IBpmnClient>(options =>
        {
            var loggerFactory = options.GetRequiredService<ILoggerFactory>();
            var elasticClient = options.GetRequiredService<IElasticClientSetDataAsync>();
            var historyNodeStateWriter = options.GetRequiredService<IHistoryNodeStateWriter>();
            var descriptionWriteService = options.GetRequiredService<IDescriptionWriteService>();
            var serializerProcessSection = options.GetRequiredService<IXmlSerializationProcessSection>();
            var serializerDiagramSection = options.GetRequiredService<IXmlSerializationBpmnDiagramSection>();

            var processModelBuilder = options.GetRequiredService<IProcessModelBuilder>();
            var checkBpmnProcessDto = options.GetRequiredService<ICheckBpmnProcessDto>();

            return BpmnClientBuilder.Build(
                pathDiagram,
                loggerFactory,
                elasticClient,
                historyNodeStateWriter,
                descriptionWriteService,
                serializerProcessSection,
                serializerDiagramSection,
                processModelBuilder,
                checkBpmnProcessDto);
        });

        return services;
    }

    /// <summary>
    ///     Регистрация handlers из всей сборки.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <typeparam name="THandler">Тип регистрируемых сервисов.</typeparam>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AutoRegisterBpmnHandlersFromAssemblyOf<THandler>(
        this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var assemblyToRegister = GetAssembly<THandler>();

        RegisterAssembly(services, assemblyToRegister);

        return services;
    }

    /// <summary>
    ///     Регистрация handlers из конкретной сборки.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="handlerType">Type handler.</param>
    /// <returns>IServiceCollection.</returns>
    public static IServiceCollection AutoRegisterBpmnHandlersFromAssemblyNamespaceOf(this IServiceCollection services,
        Type handlerType)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (handlerType == null)
        {
            throw new ArgumentNullException(nameof(handlerType));
        }

        RegisterAssembly(services, handlerType.Assembly, handlerType.Namespace);

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
                ImplementedHandlerInterfaces = GetImplementedHandlerInterfaces(type).ToList(),
            });

        var typesToAutoRegister = emplementeds
            .Where(a => a.ImplementedHandlerInterfaces.Any());

        if (!string.IsNullOrEmpty(namespaceFilter))
        {
            typesToAutoRegister = typesToAutoRegister.Where(a =>
                a.Type.Namespace != null && a.Type.Namespace.StartsWith(namespaceFilter));
        }

        foreach (var type in typesToAutoRegister)
        {
            RegisterType(services, type.Type);
        }
    }

    private static void RegisterType(IServiceCollection services, Type typeToRegister)
    {
        var implementedHandlerInterfaces = GetImplementedHandlerInterfaces(typeToRegister).ToArray();

        foreach (var handlerInterface in implementedHandlerInterfaces)
        {
            services.AddScoped(handlerInterface, typeToRegister);
        }
    }
}