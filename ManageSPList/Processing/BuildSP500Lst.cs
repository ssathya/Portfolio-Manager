using ApplicationModels.Indexes;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace ManageSPList.Processing;

public class BuildSP500Lst : IBuildSP500Lst
{
    #region Private Fields

    private const string dataSource = @"https://en.wikipedia.org/wiki/List_of_S%26P_500_companies";
    private const string nodeEleToProcess = @"//*[@id=\""constituents\""]/tbody/tr";
    private const string tableData = @"td";
    private const string tableHeader = @"<th>";
    private readonly ILogger<BuildSP500Lst> logger;

    #endregion Private Fields

    #region Public Constructors

    public BuildSP500Lst(ILogger<BuildSP500Lst> logger)
    {
        this.logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<List<IndexComponent>> ExcecAsync()
    {
        var web = new HtmlWeb();
        List<IndexComponent> extractValues = new();
        HtmlDocument doc;
        try
        {
            doc = await web.LoadFromWebAsync(dataSource);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BuildSP500Lst:ExcecAsync; Could not get values from Wikipedia");
            return extractValues;
        }
        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(nodeEleToProcess);
        if (!nodes.Any())
        {
            return extractValues;
        }
        foreach (var node in nodes)
        {
            if (node.InnerHtml.Contains(tableHeader, StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }
            IndexComponent ic = ExtractFirmFromNode(node);
            extractValues.Add(ic);
        }
        return extractValues;
    }

    #endregion Public Methods

    #region Private Methods

    private IndexComponent ExtractFirmFromNode(HtmlNode node)
    {
        int index = 0;
        var rValue = new IndexComponent();
        foreach (HtmlNode col in node.SelectNodes(tableData))
        {
            switch (index)
            {
                case 0:
                    rValue.Ticker = col.InnerText;
                    break;

                case 1:
                    rValue.CompanyName = col.InnerText;
                    break;

                case 3:
                    rValue.Sector = col.InnerText;
                    break;

                case 4:
                    rValue.SubSector = col.InnerText;
                    break;

                default:
                    break;
            }
            index++;
        }
        rValue.CleanUpValues();
        rValue.IdxName = IndexNames.SnP;
        return rValue;
    }

    #endregion Private Methods
}