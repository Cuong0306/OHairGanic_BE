using OHairGanic.BLL.Interfaces;
using OHairGanic.DAL.Models;
using OHairGanic.DAL.UnitOfWork;
using OHairGanic.DTO.Requests;
using OHairGanic.DTO.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OHairGanic.BLL.Implementations
{
    public class CaptureService : ICaptureService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CaptureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CaptureResponse> AddCaptureAsync(int userId, CreateCaptureRequest dto)
        {
            if (userId == null)
                throw new Exception("Invalid userId in token");
            var newCapture = new Capture
            {
                UserId = userId,
                Angle = dto.Angle,
                ImageUrl = dto.ImageUrl,
                TakenAt = DateTime.UtcNow,

            };
            await _unitOfWork.Captures.AddCaptureAsync(newCapture);
            return new CaptureResponse
            {
                Id = newCapture.Id,
                UserId = newCapture.UserId,
                Angle = newCapture.Angle,
                ImageUrl = newCapture.ImageUrl,
                TakenAt = newCapture.TakenAt
                
            };
        }

        public async Task<bool> DeleteCaptureAsync(int id)
        {
            var result = await _unitOfWork.Captures.DeleteCaptureAsync(id);
            if (!result)
            {
                throw new Exception("Failed to delete Capture");
            }
            return true;
        }

        public async Task<List<CaptureResponse>> GetAllCapturesAsync()
        {
            var captures = await _unitOfWork.Captures.GetAllCapturesAsync();
            var captureResponse = captures.Select(captures => new CaptureResponse
            {
                UserId = captures.UserId,
                Id = captures.Id,
                Angle = captures.Angle,
                ImageUrl = captures.ImageUrl,
                TakenAt = captures.TakenAt
            }).ToList();

            return captureResponse;
        }

        public async Task<CaptureResponse> GetCaptureByIdAsync(int id)
        {
            var capture = await _unitOfWork.Captures.GetCaptureByIdAsync(id);
            if (capture == null)
            {
                throw new Exception("Capture not found");
            }
            else
            {
                return new CaptureResponse
                {
                    UserId = capture.UserId,
                    Id = capture.Id,
                    Angle = capture.Angle,
                    ImageUrl = capture.ImageUrl,
                    TakenAt = capture.TakenAt
                };
            }
        }
    }
}
