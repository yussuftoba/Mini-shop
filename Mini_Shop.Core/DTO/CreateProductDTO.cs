using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace DTO;

public class CreateProductDTO
{
	[Required, MaxLength(60)]
	public string Name { get; set; }
	public string? Description { get; set; }

	[Required, Range(0.01, double.MaxValue)]
	public decimal Price { get; set; }

	[Required, Range(0, int.MaxValue)]
	public int Stock { get; set; }
	public List<IFormFile> ImageURLs { get; set; }
}
