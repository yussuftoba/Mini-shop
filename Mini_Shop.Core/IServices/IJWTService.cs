using Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace IServices;

public interface IJWTService
{
	Task<object> CreateJwtToken(ApplicationUser user);
}
