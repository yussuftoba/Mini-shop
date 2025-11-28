using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO;

public class SignInDTO
{
	[Required(ErrorMessage = "Email can't be blank")]
	[EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
	public string Email { get; set; } = string.Empty;


	[Required(ErrorMessage = "Password can't be blank")]
	public string Password { get; set; } = string.Empty;
}
