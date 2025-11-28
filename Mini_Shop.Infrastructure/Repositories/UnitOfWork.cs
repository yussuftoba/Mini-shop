using DatabaseContext;
using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories;

public class UnitOfWork : IUnitOfWork
{
	private readonly Mini_ShopDbContext _context;

	public UnitOfWork(Mini_ShopDbContext context)
	{
		_context = context;
		Products = new Repository<Product>(context);
		ProductImages = new Repository<ProductImages>(context);
	}
	public IRepository<Product> Products { get; }

	public IRepository<ProductImages> ProductImages { get; }

	public void Dispose()
	{
		_context.Dispose();
	}

	public int Save()
	{
		return _context.SaveChanges();
	}
}
