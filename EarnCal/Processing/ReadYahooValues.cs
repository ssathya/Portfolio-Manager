using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EarnCal.Processing;

public class ReadYahooValues
{
    //private const string innerNode = """//*[@id="cal-res-table"]/div[1]/table/tbody/tr/td[3]/span[1]""";
    private const string innerNode = """//*[@id="cal-res-table"]/div[1]/table/tbody/tr[*]/td[3]/span[1]""";

    private const string urlKey = "Yahoo";
    private readonly IConfiguration configuration;
    private readonly DateTime defaultDt = new DateTime(1900, 1, 1).ToUniversalTime();
    private readonly ILogger<ReadYahooValues> logger;

    public ReadYahooValues(IConfiguration configuration
        , ILogger<ReadYahooValues> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<DateTime> GetValuesFromWeb(string ticker)
    {
        var valueToReturn = defaultDt;

        string? urlToUse = configuration[urlKey];
        if (urlToUse == null)
        {
            return defaultDt;
        }
        urlToUse = urlToUse.Replace("""{Symbol}""", ticker.ToUpper())
            .Trim();
        var web = new HtmlWeb();

        HtmlDocument doc;
        try
        {
            doc = await web.LoadFromWebAsync(urlToUse);
        }
        catch (Exception ex)
        {
            logger.LogError("Unable to get data from Yahoo");
            logger.LogError(ex.Message);
            return defaultDt;
        }
        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(innerNode);
        if (nodes != null && nodes.Count != 0)
        {
            List<DateTime> extractDates = new();
            foreach (var node in nodes)
            {
                var nodeValue = node.InnerText;
                if (!string.IsNullOrEmpty(nodeValue))
                {
                    var parseResult = DateTime.TryParse(nodeValue, out var requiredDate);
                    if (parseResult && requiredDate <= DateTime.UtcNow)
                    {
                        extractDates.Add(requiredDate.ToUniversalTime());
                    }
                }
            }
            if (extractDates.Count > 0)
            {
                extractDates.Sort();
                valueToReturn = extractDates.Last();
                return valueToReturn;
            }
        }

        return valueToReturn;
    }
}