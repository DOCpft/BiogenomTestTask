using Biogenom.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Interfaces.Repositories
{
    public interface IAnalysisRequestRepository
    {
        Task<AnalysisRequest> GetByIdAsync(int id);
        Task AddAsync(AnalysisRequest analysisRequest);
        Task SaveChangesAsync();
    }
}
