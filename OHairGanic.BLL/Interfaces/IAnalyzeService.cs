// OHairGanic.BLL/Interfaces/IAnalyzeService.cs
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;

namespace OHairGanic.BLL.Interfaces
{
    public interface IAnalyzeService
    {
        Task<AnalyzeResponse> AddAnalyzeAsync(CreateAnalyzeRequest dto);
        Task<AnalyzeResponse?> GetAnalyzeByIdAsync(int id);
        Task<List<AnalyzeResponse>> GetDailyAnalysesAsync(int userId);
        Task<List<AnalyzeResponse>> GetAllAnalysesAsync();
        Task<List<AnalyzeResponse>> GetAnalysesByUserIdAsync(int userId);

        // ✅ mới: filter theo ngày (inclusive) cho “mine”
        Task<List<AnalyzeResponse>> FilterByDayRangeAsync(int userId, DateOnly from, DateOnly to);

        // (giữ) filter linh hoạt cũ để tương thích
        Task<List<AnalyzeResponse>> FilterAnalysesAsync(
            int? targetUserId,
            string? from,
            string? to,
            bool isAdmin,
            int currentUserId);
    }
}
