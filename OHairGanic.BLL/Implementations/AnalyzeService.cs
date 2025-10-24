// OHairGanic.BLL/Implementations/AnalyzeService.cs
using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DAL.Models;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System.Globalization;

namespace OHairGanic.BLL.Implementations
{
    public class AnalyzeService : IAnalyzeService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AnalyzeService(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public async Task<AnalyzeResponse> AddAnalyzeAsync(CreateAnalyzeRequest dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto), "Analyze request cannot be null.");
            if (dto.CaptureId == null || dto.CaptureId <= 0) throw new ArgumentException("Invalid CaptureId. It must be a positive number.", nameof(dto.CaptureId));
            ValidateScore(dto.Oiliness, "Oiliness");
            ValidateScore(dto.Dryness, "Dryness");
            ValidateScore(dto.DandruffScore, "DandruffScore");
            if (!string.IsNullOrWhiteSpace(dto.Label) && dto.Label.Length > 100) throw new ArgumentException("Label is too long. Maximum 100 characters.", nameof(dto.Label));
            if (!string.IsNullOrWhiteSpace(dto.ModelVersion) && dto.ModelVersion.Length > 50) throw new ArgumentException("ModelVersion is too long. Maximum 50 characters.", nameof(dto.ModelVersion));

            var newAnalyze = new Analysis
            {
                CaptureId = dto.CaptureId,
                Oiliness = dto.Oiliness ?? 0,
                Dryness = dto.Dryness ?? 0,
                DandruffScore = dto.DandruffScore ?? 0,
                Label = dto.Label?.Trim() ?? string.Empty,
                ModelVersion = dto.ModelVersion?.Trim() ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };

            bool created = await _unitOfWork.Analyzes.AddAnalyzeAsync(newAnalyze);
            if (!created) throw new Exception("Failed to create analysis due to database error.");

            return Map(newAnalyze);
        }

        public async Task<AnalyzeResponse?> GetAnalyzeByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid analyze id.", nameof(id));
            var analyze = await _unitOfWork.Analyzes.GetAnalyzeByIdAsync(id);
            return analyze == null ? null : Map(analyze);
        }

        public async Task<List<AnalyzeResponse>> GetDailyAnalysesAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid userId.");
            DateTime startOfDay = DateTime.UtcNow.Date;
            DateTime endOfDay = startOfDay.AddDays(1);
            var analyses = await _unitOfWork.Analyzes.GetDailyAnalysesAsync(userId, startOfDay, endOfDay);
            return MapList(analyses);
        }

        public async Task<List<AnalyzeResponse>> GetAllAnalysesAsync()
        {
            var analyses = await _unitOfWork.Analyzes.GetAllAnalysisAsync();
            return MapList(analyses);
        }

        public async Task<List<AnalyzeResponse>> GetAnalysesByUserIdAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid userId.");
            var analyses = await _unitOfWork.Analyzes.GetAnalysesByUserIdAsync(userId);
            return MapList(analyses);
        }

        // ✅ filter theo ngày (inclusive)
        public async Task<List<AnalyzeResponse>> FilterByDayRangeAsync(int userId, DateOnly from, DateOnly to)
        {
            if (userId <= 0) throw new ArgumentException("Invalid userId.");
            if (from > to) throw new ArgumentException("'from' phải <= 'to'.");

            var startUtc = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0, DateTimeKind.Utc);
            var endUtcExclusive = new DateTime(to.Year, to.Month, to.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1);

            var list = await _unitOfWork.Analyzes.GetAnalysesByUserInRangeAsync(userId, startUtc, endUtcExclusive);
            return MapList(list);
        }

        // (giữ) range linh hoạt (YYYY | YYYY-MM | YYYY-MM-DD)
        public async Task<List<AnalyzeResponse>> FilterAnalysesAsync(
            int? targetUserId,
            string? from,
            string? to,
            bool isAdmin,
            int currentUserId)
        {
            int scopeUserId = targetUserId ?? currentUserId;
            if (targetUserId.HasValue && !isAdmin && scopeUserId != currentUserId)
                throw new UnauthorizedAccessException("You can only view your own analyses.");

            DateTime? start = null, end = null;

            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                if (!TryParseBoundary(from!, out var s1, out _))
                    throw new ArgumentException("Invalid 'from' format. Use YYYY or YYYY-MM or YYYY-MM-DD.", nameof(from));
                if (!TryParseBoundary(to!, out _, out var e2))
                    throw new ArgumentException("Invalid 'to' format. Use YYYY or YYYY-MM or YYYY-MM-DD.", nameof(to));

                start = s1; end = e2; // end-exclusive
                if (start >= end)
                    throw new ArgumentException("'from' must be earlier than 'to'.");
            }
            else if (!string.IsNullOrWhiteSpace(from))
            {
                if (!TryParseBoundary(from!, out var s, out var e))
                    throw new ArgumentException("Invalid 'from' format. Use YYYY or YYYY-MM or YYYY-MM-DD.", nameof(from));
                start = s; end = e;
            }
            else if (!string.IsNullOrWhiteSpace(to))
            {
                if (!TryParseBoundary(to!, out var s, out var e))
                    throw new ArgumentException("Invalid 'to' format. Use YYYY or YYYY-MM or YYYY-MM-DD.", nameof(to));
                start = s; end = e;
            }

            IEnumerable<Analysis> list = (start, end) switch
            {
                ({ } s, { } e) => await _unitOfWork.Analyzes.GetAnalysesByUserInRangeAsync(scopeUserId, s, e),
                _ => await _unitOfWork.Analyzes.GetAnalysesByUserIdAsync(scopeUserId)
            };

            return MapList(list);
        }

        private static void ValidateScore(double? value, string fieldName)
        {
            if (value == null) return;
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(fieldName, $"Invalid {fieldName} score: must be between 0 and 100.");
        }

        // hỗ trợ YYYY | YYYY-MM | YYYY-MM-DD
        private static bool TryParseBoundary(string input, out DateTime startUtc, out DateTime endUtc)
        {
            startUtc = default;
            endUtc = default;
            var s = input.Trim();

            if (DateTime.TryParseExact(s, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var y))
            {
                startUtc = new DateTime(y.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                endUtc = startUtc.AddYears(1);
                return true;
            }
            if (DateTime.TryParseExact(s, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ym))
            {
                startUtc = new DateTime(ym.Year, ym.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                endUtc = startUtc.AddMonths(1);
                return true;
            }
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ymd))
            {
                startUtc = new DateTime(ymd.Year, ymd.Month, ymd.Day, 0, 0, 0, DateTimeKind.Utc);
                endUtc = startUtc.AddDays(1);
                return true;
            }
            return false;
        }

        private static AnalyzeResponse Map(Analysis a) => new AnalyzeResponse
        {
            Id = a.Id,
            CaptureId = a.CaptureId,
            Oiliness = a.Oiliness,
            Dryness = a.Dryness,
            DandruffScore = a.DandruffScore,
            Label = a.Label,
            ModelVersion = a.ModelVersion,
            CreatedAt = a.CreatedAt
        };

        private static List<AnalyzeResponse> MapList(IEnumerable<Analysis?> items)
            => items.Where(a => a != null).Select(a => Map(a!)).ToList();
    }
}
