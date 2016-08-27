using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BillSlicer.Models {

    public class Room {

        // Model of a room

        [Key]
        public int ID { get; set; }

        public int Number { get; set; }

        public string Name { get; set; }

        // The roommates
        public virtual ICollection<ApplicationUser> Users { get; set; }

        // Multiple products which roommates frequently buy
        public virtual ICollection<Product> Products { get; set; }

        // The receipts roommates want to split
        public virtual ICollection<Receipt> Receipts { get; set; }

    }
    public class Receipt {

        // Model of a receipt

        public int ID { get; set; }

        // The name of the receipt, which is optional
        public string Name { get; set; }

        // Date the receipt was created
        [Required]
        public DateTime DateAdded { get; set; }

        // Date the receipt was last modified
        public DateTime? LastDateModified { get; set; }

        // The room
        public virtual Room Room { get; set; }

        // The roommate who added the receipt
        public virtual ApplicationUser Roommate { get; set; }

        public virtual ICollection<Product> Products { get; set; }

    }

    public class Product {

        // Model of a receipt

        [Required]
        public int ID { get; set; }

        // The name of the receipt
        [Required]
        public string Name { get; set; }

        public virtual ICollection<ProductAttachment> Attachment { get; set; }

        // The price
        [Required]
        [DataType (DataType.Currency)]
        [DisplayFormat (DataFormatString = "{0:c}")]
        public decimal Price { get; set; }

        // A brief description
        [DataType (DataType.MultilineText)]
        public string Description { get; set; }

        // The room in which we find the product
        public virtual Room Room { get; set; }

        // The roommate who created the product
        public virtual ApplicationUser Roommate { get; set; }

        // The state of the Roommate-Buys-Product checkboxes 
        public virtual ICollection<Split> Checkages { get; set; }

        public virtual ICollection<Receipt> Receipts { get; set; }


    }

    public class Split {

        public decimal Quantity { get; set; }

        public int ID { get; set; }
        public virtual Product Product { get; set; }
        public virtual Receipt Receipt { get; set; }

        public string SplitString { get; set; }

    }


}