using ApplicationModels.Indexes;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace AppCommon.DatabaseHandler;

/// <summary>
/// Handles Index components in database.
/// </summary>

public class HandleDataInDatabase : IHandleDataInDatabase
{
    #region Private Fields

    private readonly ILogger<HandleDataInDatabase> logger;
    private readonly IRepository<IndexComponent> repository;
    private IndexNames currentIndex;

    #endregion Private Fields

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HandleDataInDatabase"/> class.
    /// </summary>
    /// <param name="repository">The repository.</param>
    /// <param name="logger">The logger.</param>
    public HandleDataInDatabase(IRepository<IndexComponent> repository, ILogger<HandleDataInDatabase> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    /// <summary>Do processing</summary>
    /// <param name="extractResult">The extract result.</param>
    /// <param name="currentIndex">Index of the current.</param>
    /// <returns>Boolean: true if operation is a success</returns>
    public async Task<bool> ExecAsync(List<IndexComponent> extractResult, IndexNames currentIndex)
    {
        this.currentIndex = currentIndex;

        List<IndexComponent> existingRcds = await GetAllExistingRecords();
        if (existingRcds.Count == 0)
        {
            logger.LogError("Error obtaining values from database");
            return false;
        }
        var processingResult = await AddNewRecordsToDatabase(existingRcds, extractResult);
        if (!processingResult)
            return false;
        processingResult = await RemoveFirmsDroppedFromIndex(existingRcds, extractResult);
        if (!processingResult)
            return false;
        processingResult = await IndexDroppedHandler(existingRcds, extractResult);
        if (!processingResult)
            return false;
        processingResult = await IndexAddHandler(existingRcds, extractResult);
        if (!processingResult)
            return false;
        processingResult = await RemoveRecordsNotInAnyIndex();
        return processingResult;
    }

    /// <summary>
    /// Removes  records not in any Index.
    /// </summary>
    /// <returns></returns>
    private async Task<bool> RemoveRecordsNotInAnyIndex()
    {
        try
        {
            List<IndexComponent> recordsToProcess = (await repository.FindAll(x => x.ListedInIndex == IndexNames.None))
                .ToList();
            if (recordsToProcess.Any())
            {
                await repository.Remove(recordsToProcess);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error in RemoveRecordsNotInAnyIndex");
            logger.LogError(ex.Message);
            return false;
        }
        return true;
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>Adds the new records to database.</summary>
    /// <param name="existingRcds">The existing RCDS.</param>
    /// <param name="extractResult">The extract result.</param>
    /// <returns>boolean True if operation is success.</returns>
    private async Task<bool> AddNewRecordsToDatabase(List<IndexComponent> existingRcds, List<IndexComponent> extractResult)
    {
        //items to insert
        List<IndexComponent> recordsToProcess = new();
        foreach (var rcd in extractResult)
        {
            var newRcd = existingRcds.FirstOrDefault(x => x.Ticker == rcd.Ticker);
            if (newRcd == null)
            {
                rcd.ListedInIndex |= currentIndex;
                recordsToProcess.Add(rcd);
            }
        }
        //Any new records? Insert them to database.
        if (recordsToProcess.Any())
        {
            try
            {
                await repository.Add(recordsToProcess);
            }
            catch (Exception ex)
            {
                logger.LogError("Unable to add records to database");
                logger.LogError($"{ex.Message}");
            }
        }
        return true;
    }

    /// <summary>
    /// Gets all existing records.
    /// </summary>
    /// <returns>List of index components in database</returns>
    private async Task<List<IndexComponent>> GetAllExistingRecords()
    {
        try
        {
            List<IndexComponent> returnValue = (await repository.FindAll()).ToList();
            logger.LogInformation($"Database has {returnValue.Count} records");
            return returnValue;
        }
        catch (Exception ex)
        {
            logger.LogError("Error in GetAllExistingRecords");
            logger.LogError($"{ex.Message}");
        }
        return new List<IndexComponent> { };
    }

    /// <summary>
    /// If a security is already in database but not in index then update the record.
    /// </summary>
    /// <param name="existingRcds">The existing RCDS.</param>
    /// <param name="extractResult">The extract result.</param>
    /// <returns>Boolean: true if operation is a success</returns>
    private async Task<bool> IndexAddHandler(List<IndexComponent> existingRcds, List<IndexComponent> extractResult)
    {
        List<IndexComponent> recordsToProcess = new();
        List<string> newTickers = extractResult.Select(x => x.Ticker).ToList();
        recordsToProcess = existingRcds.Where(x => newTickers.Contains(x.Ticker))
            .Where(x => !x.ListedInIndex.HasFlag(currentIndex)).ToList();
        if (recordsToProcess.Any())
        {
            foreach (var rcdToUpdate in recordsToProcess)
            {
                rcdToUpdate.ListedInIndex |= currentIndex;
            }
            try
            {
                await repository.Update(recordsToProcess);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred in IndexAddHandler");
                logger.LogError($"{ex.Message}");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// If a security in database previously has included in index but its state has changed.
    /// </summary>
    /// <param name="existingRcds">The existing RCDS.</param>
    /// <param name="extractResult">The extract result.</param>
    /// <returns>Boolean: true if operation is a success</returns>
    private async Task<bool> IndexDroppedHandler(List<IndexComponent> existingRcds, List<IndexComponent> extractResult)
    {
        List<string> newTickers = extractResult.Select(x => x.Ticker).ToList();
        var staleRcds = existingRcds
            .Where(x => x.ListedInIndex.HasFlag(currentIndex))
            .Where(x => !newTickers.Contains(x.Ticker)).ToList();
        if (staleRcds.Any())
        {
            foreach (var staleRcd in staleRcds)
            {
                staleRcd.ListedInIndex ^= currentIndex;
            }
            try
            {
                await repository.Update(staleRcds);
            }
            catch (Exception ex)
            {
                logger.LogError("Error occurred in IndexDroppedHandler");
                logger.LogError($"{ex.Message}");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// If a security was listed only in this index and now dropped then remove from database.
    /// </summary>
    /// <param name="existingRcds">The existing RCDS.</param>
    /// <param name="extractResult">The extract result.</param>
    /// <returns></returns>
    private async Task<bool> RemoveFirmsDroppedFromIndex(List<IndexComponent> existingRcds, List<IndexComponent> extractResult)
    {
        List<string> newTickers = extractResult.Select(x => x.Ticker).ToList();
        var staleRcds = existingRcds
            .Where(x => x.ListedInIndex.Equals(currentIndex))
            .Where(x => !newTickers.Contains(x.Ticker)).ToList();
        if (staleRcds.Any())
        {
            try
            {
                await repository.Remove(staleRcds);
            }
            catch (Exception ex)
            {
                logger.LogError("Error while trying to remove stale records");
                logger.LogError(ex.Message);
                return false;
            }
        }
        return true;
    }

    #endregion Private Methods
}