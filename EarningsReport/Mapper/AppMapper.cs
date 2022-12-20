using ApplicationModels.FinancialStatement;
using AutoMapper;
using Xbrl.FinancialStatement;

namespace EarningsReport.Mapper;

public class AppMapper : Profile
{
    public AppMapper()
    {
        CreateMap<FinancialStatement, FinStatements>()
            .ForMember(d => d.Ticker, s => s.Ignore())
            .ForMember(d => d.Id, s => s.Ignore())
            .ForMember(d => d.FilingType, s => s.Ignore())
            .ReverseMap();
    }
}