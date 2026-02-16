using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Application.Interfaces
{
    public interface IAiService
    {
        Task<List<string>> PredictMainObjectsAsync(byte[] imageBytes);
        Task<Dictionary<string, List<string>>> PredictMaterialsAsync(byte[] imageBytes, List<string> items, string? existingFileRef = null);

        // Гарантированно вернуть fileRef: вернёт существующий если передан, иначе загрузит
        Task<string> EnsureUploadedAsync(byte[] imageBytes, string? existingFileRef = null);
    }
}
