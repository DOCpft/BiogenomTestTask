using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Entities
{
    public class ItemMaterial
    {
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; }

    }
}
