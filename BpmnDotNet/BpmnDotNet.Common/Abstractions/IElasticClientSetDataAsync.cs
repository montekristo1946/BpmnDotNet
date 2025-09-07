namespace BpmnDotNet.Common.Abstractions;

using System.Threading.Tasks;

/// <summary>
/// Запись данных.
/// </summary>
public interface IElasticClientSetDataAsync
{
    /// <summary>
    /// Сохраняет модели истории проходов по блокам.
    /// </summary>
    /// <param name="document">Документ для сохранения.</param>
    /// <typeparam name="T">Тип документа.</typeparam>
    /// <returns>Результат сохранения.</returns>
    public Task<bool> SetDataAsync<T>(T document);
}