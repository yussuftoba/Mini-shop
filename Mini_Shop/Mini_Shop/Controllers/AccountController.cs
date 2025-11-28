using DTO;
using Identity;
using Interfaces;
using IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace Mini_Shop.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly EmailSender _emailSender;
		private readonly IJWTService _jwtService;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<ApplicationRole> _roleManager;

		public AccountController(IUnitOfWork unitOfWork, EmailSender emailSender, IJWTService jwtService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
		{
			_unitOfWork = unitOfWork;
			_emailSender = emailSender;
			_jwtService = jwtService;
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		[HttpPost("SignUpForUser")]
		public async Task<IActionResult> SignUpForUser(SignUpDTO signUpDTO)
		{
			if (ModelState.IsValid)
			{
				var user =await _userManager.FindByEmailAsync(signUpDTO.Email);

				if(user == null)
				{
					var applicationUser = new ApplicationUser
					{
						Email = signUpDTO.Email,
						PhoneNumber = signUpDTO.PhoneNumber,
						UserName = signUpDTO.Email,
						PersonName = signUpDTO.PersonName
					};

					var result = await _userManager.CreateAsync(applicationUser, signUpDTO.Password);

					if (result.Succeeded)
					{
						if(await _roleManager.FindByNameAsync("user") is null)
						{
							var role = new ApplicationRole
							{
								Name = "user"
							};
							await _roleManager.CreateAsync(role);
						}
						await _userManager.AddToRoleAsync(applicationUser, "user");

						//sign In

						await _signInManager.SignInAsync(applicationUser, isPersistent: false);

						var token = await _jwtService.CreateJwtToken(applicationUser);

						return Ok(token);
					}
					else
					{
						foreach(var error in result.Errors)
						{
							ModelState.AddModelError("Register", error.Description);
						}
						return BadRequest(ModelState);
					}
				}
				return BadRequest("This email is already registered.");
			}

			return BadRequest(ModelState);
		}

		[HttpPost("SignUpForAdmin")]
		public async Task<IActionResult> SignUpForAdmin(SignUpDTO signUpDTO)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(signUpDTO.Email);

				if (user == null)
				{
					var applicationUser = new ApplicationUser
					{
						Email = signUpDTO.Email,
						PhoneNumber = signUpDTO.PhoneNumber,
						UserName = signUpDTO.Email,
						PersonName = signUpDTO.PersonName
					};

					var result = await _userManager.CreateAsync(applicationUser, signUpDTO.Password);

					if (result.Succeeded)
					{
						if (await _roleManager.FindByNameAsync("admin") is null)
						{
							var role = new ApplicationRole
							{
								Name = "admin"
							};
							await _roleManager.CreateAsync(role);
						}
						await _userManager.AddToRoleAsync(applicationUser, "admin");

						//sign In

						await _signInManager.SignInAsync(applicationUser, isPersistent: false);

						var token = await _jwtService.CreateJwtToken(applicationUser);

						return Ok(token);
					}
					else
					{
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError("Register", error.Description);
						}
						return BadRequest(ModelState);
					}
				}
				return BadRequest("This email is already registered.");
			}

			return BadRequest(ModelState);
		}

		[HttpPost("SignIn")]
		public async Task<IActionResult> SignIn(SignInDTO signInDTO)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(signInDTO.Email);

				if(user != null)
				{
					var result = await _signInManager.PasswordSignInAsync(signInDTO.Email, signInDTO.Password, isPersistent: false, lockoutOnFailure: false);

					if (result.Succeeded)
					{
						await _signInManager.SignInAsync(user, isPersistent: false);

						var token = await _jwtService.CreateJwtToken(user);
						return Ok(token);
					}
				}
				return BadRequest("Invalid Email or Password");
			}
			return BadRequest(ModelState);
		}

		[HttpPost("ForgetPassword")]
		public async Task<IActionResult> ForgetPassword(ForgetPasswordDTO forgetPasswordDTO)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(forgetPasswordDTO.Email);

				if (user == null)
				{
					return NotFound("User with this email is not found.");
				}

				var token = await _userManager.GeneratePasswordResetTokenAsync(user);

				_emailSender.SendEmail("Password Reset Code", forgetPasswordDTO.Email, token);

				return Ok(new { message = "Password reset code sent." });
			}
			return BadRequest(ModelState);
		}

		[HttpPost("ResetPassword")]
		public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
		{
			if (ModelState.IsValid)
			{
				var user=await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
				if(user == null)
				{
					return NotFound("User with this email is not found.");
				}

				var result =await _userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);

				if (result.Succeeded)
				{
					return Ok(new { message = "Password has been reset successfully!" });
				}
				return BadRequest(result.Errors);
			}
			return BadRequest(ModelState);
		}

	}
}
