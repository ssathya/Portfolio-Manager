using System.Text.RegularExpressions;

namespace ApplicationModels.Indexes;

public class IndexComponent : Entity
{
    #region Public Properties

    public string CompanyName { get; set; } = string.Empty;
    public IndexNames ListedInIndex { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string SubSector { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;

    #endregion Public Properties

    #region Public Methods

    public void CleanUpValues()
    {
        const string Ampersand = @"&";
        Regex whiteChars = new(@"[\r\n\t'/\\]");

        RegexOptions options = RegexOptions.Multiline;
        Regex htmlAmpToReadAmp = new(@"&amp;", options);

        CompanyName = whiteChars.Replace(CompanyName, string.Empty).Trim();
        CompanyName = htmlAmpToReadAmp.Replace(CompanyName, Ampersand);
        Ticker = whiteChars.Replace(Ticker, string.Empty).Trim();

        Sector = whiteChars.Replace(Sector, string.Empty).Trim();
        Sector = htmlAmpToReadAmp.Replace(Sector, Ampersand);

        SubSector = whiteChars.Replace(SubSector, string.Empty).Trim();
        SubSector = htmlAmpToReadAmp.Replace(SubSector, Ampersand);

        string pattern = @"\.";
        string substitution = @"-";
        Regex regex = new Regex(pattern, options);
        Ticker = regex.Replace(Ticker, substitution);
    }

    #endregion Public Methods
}

[Flags]
public enum IndexNames
{
    None = 0b_0000_0000,
    SnP = 0b_0000_0001,
    Nasdaq = 0b_0000_0010,
    Dow = 0b_0000_0100
}