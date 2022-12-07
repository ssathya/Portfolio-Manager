using ApplicationModels.Indexes;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace ManageQQQList.Processing;

public class BuildNasdaqLst
{
    #region Private Fields

    private const string dataSource = @"https://en.wikipedia.org/wiki/Nasdaq-100";
    private const string tableDataTag = "td";
    private const string tableHeaderTag = "<th>";
    private const string tableRootNode = """//*[@id='constituents']/tbody/tr""";
    private readonly ILogger<BuildNasdaqLst> logger;

    #endregion Private Fields

    #region Public Constructors

    public BuildNasdaqLst(ILogger<BuildNasdaqLst> logger)
    {
        this.logger = logger;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<List<IndexComponent>> ExecAsync()
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
            logger.LogError(ex, "BuildNasdaqLst:ExcecAsync; Could not get values from Wikipedia");
            return extractValues;
        }
        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(tableRootNode);
        foreach (var row in nodes)
        {
            if (row.InnerHtml.Contains(tableHeaderTag, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogInformation($"Skipping row {row.InnerHtml}");
                continue;
            }
            IndexComponent ic = ExtractFirm(row);
            extractValues.Add(ic);
        }
        return extractValues;
    }

    #endregion Public Methods

    #region Private Methods

    private IndexComponent ExtractFirm(HtmlNode row)
    {
        int index = 0;
        var retValue = new IndexComponent();
        foreach (var col in row.SelectNodes(tableDataTag))
        {
            switch (index)
            {
                case 0:
                    retValue.CompanyName = col.InnerText;
                    break;

                case 1:
                    retValue.Ticker = col.InnerText;
                    break;

                case 2:
                    retValue.Sector = col.InnerText;
                    break;

                case 3:
                    retValue.SubSector = col.InnerText;
                    break;

                default:
                    break;
            }
            index++;
        }
        retValue.CleanUpValues();
        retValue.ListedInIndex = IndexNames.Nasdaq;
        return retValue;
    }

    #endregion Private Methods
}