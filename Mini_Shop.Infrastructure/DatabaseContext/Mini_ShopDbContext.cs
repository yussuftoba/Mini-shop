using Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseContext;

public class Mini_ShopDbContext:IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
	public Mini_ShopDbContext(DbContextOptions<Mini_ShopDbContext> options):base(options)
	{
	}

	public DbSet<Product> Products { get; set; }
	public DbSet<ProductImages> ProductImages { get; set; }

}
