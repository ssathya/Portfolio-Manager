using ApplicationModels.Indexes;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PsqlAccess;

namespace ManageDowList.Processing;

public class BuildDowLst
{
    #region Private Fields

    private const string dataSource = """https://en.wikipedia.org/wiki/Dow_Jones_Industrial_Average""";
    private const string tableDataTag = "td";
    private const string tableHeaderTag = "<th>";
    private const string tableRootNode = """//*[@id="constituents"]/tbody/tr""";
    private readonly ILogger<BuildDowLst> logger;
    private readonly IRepository<IndexComponent> repository;

    #endregion Private Fields

    #region Public Constructors

    public BuildDowLst(ILogger<BuildDowLst> logger, IRepository<IndexComponent> repository)
    {
        this.logger = logger;
        this.repository = repository;
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<List<IndexComponent>> ExecAsync()
    {
        var web = new HtmlWeb();
        List<IndexComponent> extractValues = new();
        HtmlDocument doc;
        doc = await web.LoadFromWebAsync(dataSource);
        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(tableRootNode);
        foreach (var row in nodes)
        {
            if (row.InnerHtml.Contains(tableHeaderTag, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogInformation($"Skipping row {row.InnerHtml}");
                continue;
            }
            if (row.InnerHtml.Contains(tableDataTag, StringComparison.InvariantCultureIgnoreCase))
            {
                IndexComponent ic = ExtractFirm(row);
                extractValues.Add(ic);
            }
        }
        await PopulateSectorAndSubSectorValues(extractValues);
        return extractValues;
    }

    #endregion Public Methods

    #region Private Methods

    private IndexComponent ExtractFirm(HtmlNode row)
    {
        int index = 0;
        var retValue = new IndexComponent();
        var rowHeader = row.SelectNodes("th");
        if (rowHeader != null)
        {
            try
            {
                var rowHdr = rowHeader.FindFirst("A");

                if (rowHdr != null)
                {
                    retValue.CompanyName = rowHdr.InnerText;
                }
            }
            catch (Exception)
            {
                logger.LogError("Unknown tag to handle in BuildDowLst:ExtractFirm");
                logger.LogError(row.GetDirectInnerText());
            }
        }
        foreach (var col in row.SelectNodes(tableDataTag))
        {
            switch (index)
            {
                case 1:
                    retValue.Ticker = col.InnerText;
                    break;

                default:
                    break;
            }
            if (++index >= 2)
            {
                break;
            }
        }
        retValue.CleanUpValues();
        retValue.ListedInIndex = IndexNames.Dow;
        return retValue;
    }

    private async Task PopulateSectorAndSubSectorValues(List<IndexComponent> extractValues)
    {
        var idxTickers = extractValues.Select(x => x.Ticker).ToList();
        var valuesInDb = (await repository.FindAll(x => idxTickers.Contains(x.Ticker)))
            .ToList();

        foreach (var extractValue in extractValues)
        {
            var valueInDb = valuesInDb?.FirstOrDefault(x => x.Ticker == extractValue.Ticker);
            if (valueInDb != default)
            {
                extractValue.Sector = valueInDb.Sector;
                extractValue.SubSector = valueInDb.SubSector;
                extractValue.CompanyName = valueInDb.CompanyName;
            }
        }
    }

    #endregion Private Methods
}