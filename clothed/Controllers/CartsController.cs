using System;
using Microsoft.AspNetCore.Mvc;
using clothed.Data;
using clothed.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace clothed.Controllers
{
    public class CartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public CartsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect($"/Identity/Account/Login?ReturnUrl={Url.Action("Index", "Home")}");
            }

            var userEmail = User.Identity.Name;
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserEmail == userEmail);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserEmail = userEmail,
                    CreatedAt = DateTime.Now,
                    Items = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }

        public async Task<IActionResult> ViewCart()
        {
            var userEmail = User.Identity.Name;
            var cart = await _context.Carts
                .Where(c => c.UserEmail == userEmail)
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (cart == null)
            {
                return View(new Cart());
            }

            return View(cart);
        }

        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.Identity.Name;

            var cart = await _context.Carts
                .Where(c => c.UserEmail == userEmail)
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("ViewCart");
            }

            var random = new Random();
            var orderNumber = random.Next(1000000000, 2000000000).ToString();

            var order = new Order
            {
                OrderNumber = orderNumber,
                UserEmail = userEmail,
                CreatedAt = DateTime.Now,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);

            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            var emailSubject = "CLOTHED | Your Order Confirmation";
            var orderDetails = string.Join("<br>", cart.Items.Select(item =>
                $"{item.Product.Name} (Quantity: {item.Quantity})"));

            var emailBody = $@"
        <html>
        <body style='font-family: Arial, sans-serif; background-color: #000000; color: #fff; padding: 20px;'>
            <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: rgba(0, 0, 0, 0.8); border-radius: 8px; text-align: center;'>
                <h2 style='color: #00ffff;'>Thank you for your order!</h2>
                <p>Your order number is <strong>{orderNumber}</strong>.</p>
                <p>Below are the items you ordered:</p>
                <div style='text-align: left;'>
                    <p>{orderDetails}</p>
                </div>
                <p>Your items will be shipped soon. Thank you for shopping with CLOTHED.</p>
                <p style='font-size: 14px; color: #bbb;'>Best regards,<br>The CLOTHED Team</p>
            </div>
        </body>
        </html>";

            SendEmail(userEmail, emailSubject, emailBody);

            return RedirectToAction("OrderConfirmation", new { orderNumber });
        }

        private bool SendEmail(string email, string subject, string confirmLink)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();
                message.From = new MailAddress("noreply@clothed.com");
                message.To.Add(email);
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = confirmLink;

                smtpClient.Port = 587;
                smtpClient.Host = "smtp.gmail.com";

                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("clothed.666@gmail.com", "bjwcaamwvinfifog");
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Send(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        [Authorize]
        public async Task<IActionResult> OrderConfirmation(string orderNumber)
        {
            var order = await _context.Orders
                .Where(o => o.OrderNumber == orderNumber)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [Authorize]
        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Where(o => o.Id == orderId)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return View("~/Areas/Identity/Pages/Account/OrderDetails.cshtml", order);
        }
    }
}
