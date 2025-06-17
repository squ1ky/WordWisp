using WordWisp.API.Models.DTOs.Materials;
using WordWisp.API.Models.Requests.Materials;

namespace WordWisp.API.Services.Interfaces
{
    public interface IMaterialService
    {
        Task<MaterialDto> CreateMaterialAsync(CreateMaterialRequest request, int userId);
        Task<MaterialDto> CreateMaterialWithFileAsync(CreateMaterialRequest request, IFormFile file, int userId);
        Task<MaterialDto?> GetMaterialByIdAsync(int id, int? userId = null);
        Task<List<MaterialDto>> GetMaterialsByTopicIdAsync(int topicId, int? userId = null);
        Task<MaterialDto> UpdateMaterialAsync(int id, UpdateMaterialRequest request, int userId);
        Task<MaterialDto> UpdateMaterialWithFileAsync(int id, UpdateMaterialRequest request, IFormFile file, int userId);
        Task DeleteMaterialAsync(int id, int userId);
        Task<bool> CanAccessMaterialAsync(int materialId, int userId);
        Task<bool> CanModifyMaterialAsync(int materialId, int userId);
    }
}
