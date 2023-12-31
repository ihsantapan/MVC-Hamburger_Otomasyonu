﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCHamburgerciOtomasyonu.Entity.Entities;
using MVCHamburgerciOtomasyonu.Service.DTOs.Users;
using MVCHamburgerciOtomasyonu.Web.Consts;

namespace MVCHamburgerciOtomasyonu.Web.Controllers
{
	public class AuthController : Controller
	{
		private readonly UserManager<AppUser> userManager;
		private readonly SignInManager<AppUser> signInManager;
		private readonly IMapper _mapper;

		public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMapper mapper)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult Login()
		{

			return View();
		}
		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> Login(UserLoginDto userLoginDto)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByEmailAsync(userLoginDto.Email);
				if (user != null)
				{
					var result = await signInManager.PasswordSignInAsync(user, userLoginDto.Password, userLoginDto.RememberMe, false);
					if (result.Succeeded)
					{
						var roles = await userManager.GetRolesAsync(user);
						if (roles.Contains(RoleConsts.Superadmin))
						{
							return RedirectToAction("AdminDashboard", "Dashboard");
						}
						if (roles.Contains(RoleConsts.User))
						{

							return RedirectToAction("UserDashboard", "Dashboard");
						}
					}
					else
					{
						ModelState.AddModelError("", "E-posta adresiniz veya şifreniz yanlıştır.");
						return View();
					}
				}
				else
				{
					ModelState.AddModelError("", "E-posta adresiniz veya şifreniz yanlıştır.");
					return View();
				}
			}
			else
			{
				ModelState.AddModelError("", "E-posta adresiniz veya şifreniz yanlıştır.");
				return View();
			}
			return View();
		}
		[AllowAnonymous]
		[HttpGet]
		public IActionResult Register()
		{

			return View();
		}
		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> Register(CreateUserDto createUserDto)
		{
			if (!ModelState.IsValid)
			{
				return View();
			}

			if (createUserDto.Password != createUserDto.ConfirmPassword)
			{
				ModelState.AddModelError("", "Şifreler uyuşmuyor.");
				return View();
			}
			if (createUserDto.Password == createUserDto.ConfirmPassword)
			{

				var appUser =  _mapper.Map<AppUser>(createUserDto);
				appUser.UserName = createUserDto.EMail;
				var result = await userManager.CreateAsync(appUser, createUserDto.Password);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(appUser, "User");

					return RedirectToAction("UserDashboard", "Dashboard");
				}

				ModelState.AddModelError("", "E-posta adresiniz veya şifreniz yanlıştır.");
			}
			return View();
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("Login", "Auth");
		}
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> AccessDenied()
		{
			return View();
		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> Error404()
		{
			return View();
		}

	}
}
