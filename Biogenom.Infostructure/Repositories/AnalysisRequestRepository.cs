using Biogenom.Domain.Entities;
using Biogenom.Domain.Interfaces.Repositories;
using Biogenom.Infostructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Infrastructure.Repositories
{
    /// <summary>
    /// Обеспечивает функциональность доступа к данным для сущности AnalysisRequest. Реализует интерфейс <see cref="IAnalysisRequestRepository"/> и использует <see cref="BiogenomDbContext"/> для взаимодействия с базой данных, что обеспечивает эффективное управление данными и поддерживает масштабируемость приложения. Метод GetByIdAsync загружает запрос на анализ по его идентификатору, включая связанные сущности: Items, ItemMaterials и Material, обеспечивая полную информацию о запросе на анализ и его элементах. Если запрос с указанным идентификатором не найден, возвращает null.
    /// </summary>
    public class AnalysisRequestRepository : IAnalysisRequestRepository
    {
        private readonly BiogenomDbContext _dbContext;

        public AnalysisRequestRepository(BiogenomDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(AnalysisRequest analysisRequest)
        {
            await _dbContext.AddAsync(analysisRequest);
        }

        /// <summary>
        /// Получает запрос на анализ по его идентификатору, включая связанные сущности: Items, ItemMaterials и Material. Использует метод Include для загрузки связанных данных, обеспечивая полную информацию о запросе на анализ и его элементах. Если запрос с указанным идентификатором не найден, возвращает null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AnalysisRequest> GetByIdAsync(int id)
        {
            var result = await _dbContext.AnalysisRequests
                .Include(r => r.Items)
                    .ThenInclude(i => i.ItemMaterials)
                        .ThenInclude(im => im.Material)
                .FirstOrDefaultAsync(r => r.Id == id);

            return result!;
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
