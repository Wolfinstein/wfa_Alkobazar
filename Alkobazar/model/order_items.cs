//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Alkobazar.model
{
    using System;
    using System.Collections.Generic;
    
    public partial class order_items
    {
        public int id { get; set; }
        public int order_id { get; set; }
        public int product_id { get; set; }
        public int order_quantity { get; set; }
    
        public virtual order order { get; set; }
        public virtual product product { get; set; }
    }
}
