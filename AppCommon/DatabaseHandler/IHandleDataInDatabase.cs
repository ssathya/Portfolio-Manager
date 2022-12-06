using ApplicationModels.Indexes;

namespace AppCommon.DatabaseHandler;

public interface IHandleDataInDatabase
{
    Task<bool> ExecAsync(List<IndexComponent> extractResult, IndexNames currentIndex);
}