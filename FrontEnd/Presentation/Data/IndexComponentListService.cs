using ApplicationModels.Indexes;
using PsqlAccess;

namespace Presentation.Data;

public class IndexComponentListService
{
    private readonly ILogger<IndexComponentListService> logger;
    private readonly IRepository<IndexComponent> repository;

    public IndexComponentListService(ILogger<IndexComponentListService> logger
        , IRepository<IndexComponent> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    public async Task<List<IndexComponent>> ExecAsync()
    {
        try
        {
            var idxComponents = (await repository.FindAll())
                .ToList();
            return idxComponents;
        }
        catch (Exception ex)
        {
            logger.LogError($"Unable to get Index Components from database");
            logger.LogError(ex.Message);
        }
        return new List<IndexComponent>();
    }
}