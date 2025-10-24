using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Interfaces
{
    public interface IAnalyzeService
    {
        // 🔹 Thêm mới phân tích vào bảng Analyses
        Task<AnalyzeResponse> AddAnalyzeAsync(CreateAnalyzeRequest dto);

        // 🔹 Lấy phân tích theo ID
        Task<AnalyzeResponse> GetAnalyzeByIdAsync(int id);

        // 🔹 Lấy danh sách phân tích trong ngày (của user hiện tại)
        Task<List<AnalyzeResponse>> GetDailyAnalysesAsync(int userId);

        // 🆕 Lấy toàn bộ phân tích (Admin)
        Task<List<AnalyzeResponse>> GetAllAnalysesAsync();

        // 🆕 Lấy danh sách phân tích theo userId (Admin hoặc chính user đó)
        Task<List<AnalyzeResponse>> GetAnalysesByUserIdAsync(int userId);
        // 🆕 range filter theo from/to
        Task<List<AnalyzeResponse>> FilterAnalysesAsync(
            int? targetUserId,
            string? from,
            string? to,
            bool isAdmin,
            int currentUserId);

    }
}
