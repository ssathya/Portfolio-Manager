using ApplicationModels.Indexes;

namespace ManageSPList.Processing.Interfaces;
public interface IBuildSP500Lst
{
    Task<List<IndexComponent>> ExcecAsync();
}