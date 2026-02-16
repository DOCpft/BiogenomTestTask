using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biogenom.Domain.Entities
{
    public class Material
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ItemMaterial> ItemMatelials { get; set; }

    }
}
