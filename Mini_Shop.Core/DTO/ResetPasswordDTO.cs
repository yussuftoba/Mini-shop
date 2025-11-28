using System;
using System.Collections.Generic;
using System.Text;

namespace DTO;

public class ResetPasswordDTO
{
	public string Email { get; set; }
	public string Token { get; set; }
	public string NewPassword { get; set; }
}
