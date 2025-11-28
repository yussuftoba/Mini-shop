using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
	public class EmailOtpTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : class
	{
		public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
		{
			// Always allow generating this token, even if 2FA is not enabled
			return Task.FromResult(false);
		}
	}
}
