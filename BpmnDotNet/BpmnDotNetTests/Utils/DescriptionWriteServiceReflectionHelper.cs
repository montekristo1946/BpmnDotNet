using System.Collections.Concurrent;
using System.Reflection;
using BpmnDotNet.Handlers;

namespace BpmnDotNetTests.Utils;

internal static class DescriptionWriteServiceReflectionHelper
{
    private const string DictionaryFieldName = "_dictionary";

    public static ConcurrentDictionary<string, string> GetDictionary(
        DescriptionWriteService service)
    {
        var field = typeof(DescriptionWriteService)
            .GetField(DictionaryFieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (field == null)
        {
            throw new InvalidOperationException(
                $"Field '{DictionaryFieldName}' not found in DescriptionWriteService");
        }

        return (ConcurrentDictionary<string, string>)field.GetValue(service)!;
    }

    public static void SetDictionary(
        DescriptionWriteService service, 
        ConcurrentDictionary<string, string> dictionary)
    {
        var field = typeof(DescriptionWriteService)
            .GetField(DictionaryFieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (field == null)
        {
            throw new InvalidOperationException(
                $"Field '{DictionaryFieldName}' not found in DescriptionWriteService");
        }

        field.SetValue(service, dictionary);
    }
}