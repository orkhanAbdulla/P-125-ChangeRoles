using FrontToBack.DAL;
using FrontToBack.Models;
using FrontToBack.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontToBack.Controllers
{
    public class BasketController : Controller
    {
        private readonly Context _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(Context context,UserManager<AppUser>userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> AddBasket(int? id)
        {
            if (id == null) return RedirectToAction("Index", "Error");
            Product product =await _context.Products.FindAsync(id);
            if(product==null) return RedirectToAction("Index", "Error");
            string basket = Request.Cookies["basket"];
            List<BasketProduct> basketProductList;
            if (basket==null)
            {
                basketProductList = new List<BasketProduct>();
            }
            else
            {
                basketProductList = JsonConvert.DeserializeObject<List<BasketProduct>>(basket);
            }

            BasketProduct isExistProduct = basketProductList.FirstOrDefault(p => p.Id == product.Id);
            if (isExistProduct == null)
            {
                BasketProduct basketProduct = new BasketProduct
                {
                    Id=product.Id,
                    Count = 1
                };
                basketProductList.Add(basketProduct);
            }
            else
            {
                isExistProduct.Count++;
            }

            Response.Cookies.Append("basket", JsonConvert.SerializeObject(basketProductList), new CookieOptions { MaxAge = TimeSpan.FromMinutes(14) });

            return RedirectToAction("Index","Home");
        }
        public IActionResult ShowBasket()
        {
            string basket=Request.Cookies["basket"];
            List<BasketProduct> products = new List<BasketProduct>();
            if (basket != null)
            {
                products = JsonConvert.DeserializeObject<List<BasketProduct>>(basket);
                foreach (var item in products)
                {

                  Product product=  _context.Products.FirstOrDefault(p => p.Id == item.Id);
                    item.Price = product.Price;
                    item.ImageUrl = product.ImageUrl;
                    item.Name = product.Name;

                }
                Response.Cookies.Append("basket", JsonConvert.SerializeObject(products), new CookieOptions { MaxAge = TimeSpan.FromMinutes(14) });

            }
            return View(products);
        }

        public async Task<IActionResult> Sale()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            AppUser user = await _userManager.FindByNameAsync(User.Identity.Name);

            Sales sales = new Sales();
            sales.AppUserId = user.Id;
            sales.SaleDate = DateTime.Now;

           

            List<BasketProduct> basketProducts = JsonConvert.DeserializeObject<List<BasketProduct>>(Request.Cookies["basket"]);
            List<SalesProduct> salesProducts = new List<SalesProduct>();

            List<Product> dbProducts = new List<Product>();
            foreach (var item in basketProducts)
            {
                Product dbProduct = await _context.Products.FindAsync(item.Id);
                if (dbProduct.Count<item.Count)
                {
                    TempData["Fail"] = $"{item.Name} bazada yoxdur";
                    return RedirectToAction("ShowBasket", "Basket");
                }
                dbProducts.Add(dbProduct);
               

            }

            double total = 0;
            foreach (var basketProduct in basketProducts)
            {
                Product dbProduct = dbProducts.Find(p => p.Id == basketProduct.Id);

               await UpdateProductCount(dbProduct, basketProduct);

                SalesProduct salesProduct = new SalesProduct();
                salesProduct.SalesId = sales.Id;
                salesProduct.ProductId = dbProduct.Id;
                salesProducts.Add(salesProduct);
                total += basketProduct.Count * dbProduct.Price;
            }
            sales.SalesProducts = salesProducts;
            sales.Total = total;
            await _context.Sales.AddAsync(sales);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Sales Done";
            return RedirectToAction("Index", "Home");
        }

        private async Task UpdateProductCount(Product product,BasketProduct basketProduct)
        {
            product.Count = product.Count - basketProduct.Count;
            await _context.SaveChangesAsync();
        }
    }
}
