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
    }
}