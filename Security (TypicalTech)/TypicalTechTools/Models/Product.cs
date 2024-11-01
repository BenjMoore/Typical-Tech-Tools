using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace TypicalTechTools.Models
{
    public class Product
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductDescription { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
