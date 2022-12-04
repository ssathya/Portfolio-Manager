using ApplicationModels.Indexes;

namespace ManageSPList.Processing;
public interface IBuildSP500Lst
{
    Task<List<IndexComponent>> ExcecAsync();
}