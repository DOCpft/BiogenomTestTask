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
    /// Обеспечивает функциональность доступа к данным для сущности Material. Реализует интерфейс <see cref="IMaterialRepository"/> и использует <see cref="BiogenomDbContext"/> для взаимодействия с базой данных, что обеспечивает эффективное управление данными и поддерживает масштабируемость приложения. Метод GetOrCreateAsync обеспечивает уникальность материалов по имени, упрощая процесс их получения или создания при необходимости.
    /// </summary>
    public class MaterialRepository : IMaterialRepository
    {
        private readonly BiogenomDbContext _context;

        public MaterialRepository(BiogenomDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Метод для получения материала по имени. Если материал с таким именем уже существует в базе данных, он возвращается. Если материала нет, создается новый материал с указанным именем, сохраняется в базе данных и возвращается. Этот метод обеспечивает уникальность материалов по имени и упрощает процесс их получения или создания при необходимости.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<Material> GetOrCreateAsync(string name)
        {
            var material = await _context.Materials.FirstOrDefaultAsync(m => m.Name == name);
            if(material != null)
                return material;
            else 
            {
                var newMaterial = new Material { Name = name };
                _context.Materials.Add(newMaterial);
                await _context.SaveChangesAsync();
                return newMaterial;
            }
        }
    }
}
