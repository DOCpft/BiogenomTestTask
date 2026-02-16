using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Entities
{
    public class AnalysisRequest
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RawAiResponse { get; set; }
        public ICollection<Item> Items { get; set; }
        public string? UploadedFileRef { get; set; }

    }
}
