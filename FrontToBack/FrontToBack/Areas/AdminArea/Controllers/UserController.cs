using FrontToBack.DAL;
using FrontToBack.Models;
using FrontToBack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Context _context;

        public UserController(
            UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            Context context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<IActionResult> Index(string name)
        {
            var users = name == null ?  _userManager.Users.ToList() :
                _userManager.Users.Where(u => u.FullName.ToLower().Contains(name.ToLower())).ToList();
            //List<UserReturnVM> userReturnVM =new List<UserReturnVM>();
            //foreach (var user in users)
            //{
                
            //    UserReturnVM userReturn = new UserReturnVM();
            //    userReturn.FullName = user.FullName;
            //    userReturn.Email = user.Email;
            //    userReturn.UserName = user.UserName;
            //    userReturn.Role = (await _userManager.GetRolesAsync(user))[0];
            //    userReturnVM.Add(userReturn);
            //}
            return View(users);
        }

        public async Task<IActionResult>Detail(string id)
        {
            var user =await  _userManager.FindByIdAsync(id);
            UserRoleVM userRoleVm = new UserRoleVM();
            userRoleVm.AppUser = user;
            userRoleVm.Roles = await _userManager.GetRolesAsync(user);
            return View(userRoleVm);
        }
        public async Task<IActionResult> IsActive(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (user.IsActive)
            {
                user.IsActive = false;
            }
            else
            {
                user.IsActive = true;
            }

            await  _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}
