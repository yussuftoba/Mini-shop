using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces;

public interface IUnitOfWork: IDisposable
{
	public IRepository<Product> Products { get; }
	public IRepository<ProductImages> ProductImages { get; }

	int Save();
}
