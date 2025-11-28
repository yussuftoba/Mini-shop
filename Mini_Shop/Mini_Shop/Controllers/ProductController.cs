using DTO;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using System.Diagnostics;
using System.Security.Claims;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IWebHostEnvironment _env;
	private readonly IConfiguration _config;
	private readonly EmailSender _emailSender;

	public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment env, IConfiguration config, EmailSender emailSender)
	{
		_unitOfWork = unitOfWork;
		_env = env;
		_config = config;
		_emailSender = emailSender;
	}

	[HttpGet("GetAllProduct")]
	public async Task<IActionResult> GetAllProduct()
	{
		var products = await _unitOfWork.Products.FindAllAsync(x => true, new string[] { "ProductImages" });

		if (products == null)
		{
			return NotFound("There's any thing in the system!");
		}
		var productsDTO = new List<ProductDTO>();
		foreach (var product in products)
		{
			productsDTO.Add(new ProductDTO
			{
				Id = product.Id,
				Name = product.Name,
				Description = product.Description ?? string.Empty,
				Price = product.Price,
				Stock = product.Stock,
				ImageURLs = product.ProductImages?.Select(p => p.ImageUrl).ToList()
			});
		}

		return Ok(productsDTO);
	}

	[HttpGet("GetProductById/{id:int}")]
	public IActionResult GetProductById(int id)
	{
		if (id <= 0)
		{
			return BadRequest("Invalid product ID.");
		}

		var product = _unitOfWork.Products.FindOneItem(p => p.Id == id, new string[] { "ProductImages" });

		if (product == null)
		{
			return NotFound("Product is not found");
		}

		var productDTO = new ProductDTO()
		{
			Id = product.Id,
			Name = product.Name,
			Description = product.Description ?? string.Empty,
			Price = product.Price,
			Stock = product.Stock,
			ImageURLs = product.ProductImages?.Select(p => p.ImageUrl).ToList()
		};

		return Ok(productDTO);
	}

	[Authorize(Roles ="admin")]
	[HttpPost("AddProduct")]
	public async Task<IActionResult> AddProduct([FromForm] CreateProductDTO productDTO)
	{
		if (ModelState.IsValid)
		{
			if (productDTO.ImageURLs == null || productDTO.ImageURLs.Count == 0)
			{
				return BadRequest("At least one image is required.");
			}

			var product = new Product()
			{
				Name = productDTO.Name,
				Description = productDTO.Description,
				Price = productDTO.Price,
				Stock = productDTO.Stock
			};

			await _unitOfWork.Products.AddAsync(product);
			_unitOfWork.Save();

			string folderName = _env.WebRootPath + "/Images/";

			foreach (var file in productDTO.ImageURLs)
			{
				var imageName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

				using (var stream = System.IO.File.Create(folderName + imageName))
				{
					file.CopyTo(stream);
				}

				var ProductImage = new ProductImages()
				{
					ProductId = product.Id,
					ImageUrl = imageName
				};

				await _unitOfWork.ProductImages.AddAsync(ProductImage);
			}

			_unitOfWork.Save();
			return Created();
		}
		return BadRequest(ModelState);
	}

	[Authorize(Roles = "admin")]
	[HttpPut("UpdateProduct/{id:int}")]
	public async Task<IActionResult> UpdateProduct(int id, [FromForm] CreateProductDTO productDTO)
	{
		if (ModelState.IsValid)
		{
			if (id <= 0)
			{
				return BadRequest("Invalid product ID.");
			}

			var product = _unitOfWork.Products.FindOneItem(p => p.Id == id, new string[] { "ProductImages" });

			if (product == null)
			{
				return NotFound("This id isn't found in the System");
			}

			product.Name = productDTO.Name;
			product.Price = productDTO.Price;
			product.Stock = productDTO.Stock;
			product.Description = productDTO.Description;

			string folderName = _env.WebRootPath + "/Images/";

			foreach (var oldImage in product.ProductImages)
			{
				System.IO.File.Delete(folderName + oldImage.ImageUrl);
			}

			product.ProductImages.Clear();

			foreach (var file in productDTO.ImageURLs)
			{
				var imageName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

				using (var stream = System.IO.File.Create(folderName + imageName))
				{
					file.CopyTo(stream);
				}

				var ProductImage = new ProductImages()
				{
					ProductId = product.Id,
					ImageUrl = imageName
				};

				await _unitOfWork.ProductImages.AddAsync(ProductImage);

			}

			_unitOfWork.Products.Update(product);
			_unitOfWork.Save();

			return Ok("Product updated successfully");
		}

		return BadRequest(ModelState);
	}

	[Authorize(Roles = "admin")]
	[HttpDelete("DeleteProduct/{id:int}")]
	public IActionResult DeleteProduct(int id)
	{
		if (id <= 0)
		{
			return BadRequest("Invalid product ID.");
		}

		var product = _unitOfWork.Products.FindOneItem(p => p.Id == id, new string[] { "ProductImages" });
		if (product == null)
		{
			return NotFound("Product is not found");
		}

		var folderName = _env.WebRootPath + "/Images/";

		foreach (var file in product.ProductImages!)
		{
			System.IO.File.Delete(folderName + file.ImageUrl);
		}

		_unitOfWork.Products.Delete(product);
		_unitOfWork.Save();

		return Ok("Product deleted successfully.");
	}

	[Authorize]
	[HttpPost("PreOrderProduct")]
	public IActionResult PreOrderProduct([FromForm] PreOrderProductDTO preorderDTO)
	{
		if (ModelState.IsValid)
		{
			//get authorized user data
			var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var email = User.FindFirstValue(ClaimTypes.Email);
			var userName = User.Identity.Name;
			var phoneNumber=User.FindFirstValue(ClaimTypes.MobilePhone);

			var product = _unitOfWork.Products.FindOneItem(p => p.Id == preorderDTO.ProductId, new string[] { "ProductImages" });

			if (product == null)
			{
				return NotFound("Product is not found");
			}

			if(product.Stock < preorderDTO.Quantity)
			{
				return BadRequest("Requested quantity exceeds available stock.");
			}

			// Calculate total outside the string for cleaner code
			decimal totalPrice = product.Price * preorderDTO.Quantity;


			var adminEmail = _config["EmailSender:FromEmail"];
			string subject = "New Preorder Received";

			string htmlMessage = $@"
<!DOCTYPE html>
<html>
<body style='margin:0; padding:0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
    
    <table role='presentation' style='width:100%; border-collapse:collapse; background-color: #f4f4f4;'>
        <tr>
            <td align='center' style='padding: 40px 0;'>
                
                <table role='presentation' style='width:600px; border-collapse:collapse; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); overflow: hidden;'>
                    
                    <tr>
                        <td style='background-color: #007bff; padding: 20px; text-align: center; color: #ffffff;'>
                            <h2 style='margin: 0; font-size: 24px;'>New Preorder Created</h2>
                        </td>
                    </tr>

                    <tr>
                        <td style='padding: 30px;'>
                            <p style='margin-top: 0; color: #555;'>
                                <strong>Customer:</strong> {userName}<br>
                                <strong>Email:</strong> <a href='mailto:{email}' style='color: #007bff; text-decoration: none;'>{email}</a><br>
								<strong>Phone Number:</strong> <a href='mailto:{phoneNumber}' style='color: #007bff; text-decoration: none;'>{phoneNumber}</a>
                            </p>
                            
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>

                            <table style='width:100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 0; color: #555;'><strong>Product:</strong></td>
                                    <td style='padding: 8px 0; text-align: right; color: #333;'>{product.Name}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #555;'><strong>Quantity:</strong></td>
                                    <td style='padding: 8px 0; text-align: right; color: #333;'>{preorderDTO.Quantity}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #555;'><strong>Size:</strong></td>
                                    <td style='padding: 8px 0; text-align: right; color: #333;'>{preorderDTO.Size}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #555;'><strong>Color:</strong></td>
                                    <td style='padding: 8px 0; text-align: right; color: #333;'>{preorderDTO.Color}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0; color: #555;'><strong>Price:</strong></td>
                                    <td style='padding: 8px 0; text-align: right; color: #333;'>{product.Price}</td>
                                </tr>
                                <tr style='border-top: 2px solid #333;'>
                                    <td style='padding: 15px 0; font-size: 18px; font-weight: bold;'>Total Price:</td>
                                    <td style='padding: 15px 0; text-align: right; font-size: 18px; font-weight: bold; color: #28a745;'>
                                        {totalPrice}
                                    </td>
                                </tr>
                            </table>
                            
                            <p style='text-align: center; font-size: 12px; color: #999; margin-top: 30px;'>
                                Order Date: {DateTime.Now:g}
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";


			_emailSender.SendEmail(subject, adminEmail, htmlMessage);

			return Ok("Preorder request sent successfully.");
		}
		return BadRequest(ModelState);
	}
}
