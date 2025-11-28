using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

public class ProductImages
{
	public int Id { get; set; }

	[ForeignKey("Product")]
	public int ProductId { get; set; }
	public Product Product { get; set; }
	public string ImageUrl { get; set; }
}
