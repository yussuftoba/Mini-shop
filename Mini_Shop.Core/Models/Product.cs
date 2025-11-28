using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models;

public class Product
{
	[Required, Key]
	public int Id { get; set; }

	[Required, MaxLength(60)]
	public string Name { get; set; }

	[Required, Range(0.01, double.MaxValue)]
	public decimal Price { get; set; }

	[Required, Range(0, int.MaxValue)]
	public int Stock { get; set; }

	public string? Description { get; set; }

	public ICollection<ProductImages>? ProductImages { get; set;}
}
