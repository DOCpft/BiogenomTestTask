using Biogenom.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Interfaces.Repositories
{
    public interface IMaterialRepository
    {
        Task<Material> GetOrCreateAsync(string name);
    }
}
