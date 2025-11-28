using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO;

public class PreOrderProductDTO
{
	[Required, Range(1, int.MaxValue)]
	public int ProductId { get; set; }

	[Required, Range(1, int.MaxValue)]
	public int Quantity { get; set; }

	[Required]
	public string Color { get; set; }

	[Required]
	public string Size { get; set; }
}
