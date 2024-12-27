// Pages/Account/MyOrders.cshtml.cs

using clothed.Data;
using clothed.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace clothed.Pages.Account
{
    public class MyOrdersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MyOrdersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Order> Orders { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }

            var userId = User.Identity.Name;
            Orders = await _context.Orders
                                   .Where(o => o.UserEmail == userId)
                                   .Include(o => o.Items)
                                   .ThenInclude(i => i.Product)
                                   .ToListAsync();

            return Page();
        }
    }
}
