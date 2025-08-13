using EcoFashionBackEnd.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Saved_Supplier")]
public class SavedSupplier
{
    [Key]
    public Guid SavedSupplierId { get; set; } = Guid.NewGuid();

    [ForeignKey("Designer")]
    public Guid? DesignerId { get; set; }
    public virtual Designer? Designer { get; set; }

    [ForeignKey("Supplier")]
    public Guid? SupplierId { get; set; }
    public virtual Supplier? Supplier { get; set; }
}