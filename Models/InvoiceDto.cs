using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vehicle.Models
{
    public class InvoiceDto
    {
        public int IdInvoice { get; set; }
        public int? IdDriver { get; set; }
        public int IdStatus { get; set; }
        public string StoreName { get; set; }
        public string WarehouseName { get; set; }
        public DateTime IssueDate { get; set; }
    }
}
