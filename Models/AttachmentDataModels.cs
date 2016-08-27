using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillSlicer.Models {
    public class ProductAttachment {

        public int ID { get; set; }

        public byte[] Attachment { get; set; }

        public virtual Product Product { get; set; }

    }
}