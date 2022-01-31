using FrontToBack.DAL;
using FrontToBack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class SalesController : Controller
    {
        private readonly Context _context;
        private readonly UserManager<AppUser> _userManager;

        public SalesController(Context context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {

            List<Sales> sales = _context.Sales
                .Include(x=>x.AppUser)
                .Include(p=>p.SalesProducts)
                .ThenInclude(p=>p.Product).ToList() ;
            return View(sales);
        }
    }
}
