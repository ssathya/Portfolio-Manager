using ApplicationModels.Quotes;
using AutoMapper;

namespace QuotesManager.Mapper;

public class AppMapper : Profile
{
    public AppMapper()
    {
        CreateMap<YahooQuote, YQuotes>()
            .ForMember(d => d.Id, s => s.Ignore())
            .ReverseMap();
        CreateMap<CompressedQuote, YahooQuote>()
            .ForMember(d => d.Close, s => s.MapFrom(s1 => s1.ClosingPrice))
            .ForMember(d => d.Ticker, s => s.Ignore())
            .ForMember(d => d.Open, s => s.Ignore())
            .ForMember(d => d.High, s => s.Ignore())
            .ForMember(d => d.Low, s => s.Ignore())
            .ReverseMap();
    }
}