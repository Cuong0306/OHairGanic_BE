using OHairGanic.DTO.Responses;

namespace OHairGanic.BLL.Interfaces
{
    public interface IHairAnalysisService
    {
        Task<HairAnalyzeResponse> AnalyzeImageAsync(Stream imageStream);
        Task<HairAnalyzeResponse> AnalyzeFromUrlAsync(string imageUrl, string angle);
    }
}
