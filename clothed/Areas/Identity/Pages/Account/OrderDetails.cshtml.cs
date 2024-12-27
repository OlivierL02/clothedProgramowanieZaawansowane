// Pages/Account/OrderDetails.cshtml.cs

using clothed.Data;
using clothed.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace clothed.Pages.Account
{
    public class OrderDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Order Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }

            Order = await _context.Orders
                                  .Include(o => o.Items)
                                  .ThenInclude(i => i.Product)
                                  .FirstOrDefaultAsync(o => o.Id == orderId && o.UserEmail == User.Identity.Name);

            if (Order == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
