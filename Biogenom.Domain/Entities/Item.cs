using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Entities
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [ForeignKey(nameof(AnalysisRequest))]
        public int AnalysisRequestId { get; set; }
        public AnalysisRequest AnalysisRequest { get; set; }
        public ICollection<ItemMaterial> ItemMaterials { get; set; }
    }
}
