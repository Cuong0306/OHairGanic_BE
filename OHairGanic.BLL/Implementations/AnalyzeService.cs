using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.Models;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Implementations
{
    public class AnalyzeService : IAnalyzeService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AnalyzeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

    
        public async Task<AnalyzeResponse> AddAnalyzeAsync(CreateAnalyzeRequest dto)
        {
            
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Analyze request cannot be null.");

            
            if (dto.CaptureId == null || dto.CaptureId <= 0)
                throw new ArgumentException("Invalid CaptureId. It must be a positive number.", nameof(dto.CaptureId));

            
            ValidateScore(dto.Oiliness, "Oiliness");
            ValidateScore(dto.Dryness, "Dryness");
            ValidateScore(dto.DandruffScore, "DandruffScore");

            
            if (!string.IsNullOrWhiteSpace(dto.Label) && dto.Label.Length > 100)
                throw new ArgumentException("Label is too long. Maximum 100 characters.", nameof(dto.Label));

            if (!string.IsNullOrWhiteSpace(dto.ModelVersion) && dto.ModelVersion.Length > 50)
                throw new ArgumentException("ModelVersion is too long. Maximum 50 characters.", nameof(dto.ModelVersion));

            
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
            if (!created)
                throw new Exception("Failed to create analysis due to database error.");

            
            return new AnalyzeResponse
            {
                Id = newAnalyze.Id,
                CaptureId = newAnalyze.CaptureId,
                Oiliness = newAnalyze.Oiliness,
                Dryness = newAnalyze.Dryness,
                DandruffScore = newAnalyze.DandruffScore,
                Label = newAnalyze.Label,
                ModelVersion = newAnalyze.ModelVersion,
                CreatedAt = newAnalyze.CreatedAt
            };
        }

        
        public async Task<AnalyzeResponse> GetAnalyzeByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid analyze id.", nameof(id));

            var analyze = await _unitOfWork.Analyzes.GetAnalyzeByIdAsync(id);
            if (analyze == null)
                throw new KeyNotFoundException($"No analysis found with id = {id}.");

            return new AnalyzeResponse
            {
                Id = analyze.Id,
                CaptureId = analyze.CaptureId,
                Oiliness = analyze.Oiliness,
                Dryness = analyze.Dryness,
                DandruffScore = analyze.DandruffScore,
                Label = analyze.Label,
                ModelVersion = analyze.ModelVersion,
                CreatedAt = analyze.CreatedAt
            };
        }

        
        private static void ValidateScore(double? value, string fieldName)
        {
            if (value == null) return; 
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(fieldName, $"Invalid {fieldName} score: must be between 0 and 100.");
        }
    }
}
