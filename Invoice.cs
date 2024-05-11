
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vehicle
{
    public class Invoice
    {
        public int IdInvoice { get; set; }  // Assuming "idInvoice" is the invoice identifier
        public DateTime IssueDate { get; set; }
        public int IdStore { get; set; }
        public int IdWarehouse { get; set; }
        public int IdStatus { get; set; }

        // Navigation properties can be ignored for now (if not used)
        public InvoiceStatus IdStatusNavigation { get; set; }  // Assuming InvoiceStatus class exists
        public Store IdStoreNavigation { get; set; }  // Assuming Store class exists
        public Warehouse IdWarehouseNavigation { get; set; }  // Assuming Warehouse class exists

        public List<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();  // Initialize empty list
    }

    // Assuming separate classes exist for nested objects (optional)
    public class InvoiceStatus
    {
        // ... properties for InvoiceStatus
    }

    public class Store
    {
        // ... properties for Store
    }

    public class Warehouse
    {
        // ... properties for Warehouse
    }

    public class InvoiceProduct
    {
        // ... properties for InvoiceProduct (likely related to products in the invoice)
    }
}
