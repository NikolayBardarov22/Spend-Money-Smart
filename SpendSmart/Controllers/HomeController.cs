using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SpendSmartCREWAI.Models;
using Microsoft.EntityFrameworkCore;
using System; 

namespace SpendSmartCREWAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SpendSmartDbContext _context;

        public HomeController(ILogger<HomeController> logger, SpendSmartDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
           
            ViewBag.TotalExpenses = _context.Expenses.Sum(e => (decimal?)e.Value) ?? 0m;
            ViewBag.ExpenseCount = _context.Expenses.Count();
            return View();
        }

        public IActionResult Expenses()
        {
            var allExpenses = _context.Expenses
                                      .OrderByDescending(e => e.CreatedDate) 
                                      .ToList();

            var totalExpenses = allExpenses.Sum(x => x.Value);
            ViewBag.Expenses = totalExpenses;

            var topExpenses = allExpenses
                                .OrderByDescending(e => e.Value)
                                .Take(3)
                                .ToList();
            ViewBag.TopExpenses = topExpenses;

            return View(allExpenses);
        }

        public IActionResult CreateEditExpense(int? id)
        {
            if (id != null) 
            {
                var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
                if (expenseInDb == null)
                {
                    return NotFound(); 
                }
              
                return View(expenseInDb);
            }
            else
            {
                return View(new Expense());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEditExpenseForm(Expense model)
        {
            if (model.Id == 0) // Creating a new expense
            {
                model.CreatedDate = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(model.Description))
                {
                    ModelState.AddModelError(nameof(model.Description), "Description cannot be empty.");
                }
            }
            else // Updating an existing expense
            {
                // Fetch the existing date from the database to prevent overwriting
                var existingDate = _context.Expenses
                                           .Where(e => e.Id == model.Id)
                                           .Select(e => e.CreatedDate)
                                           .FirstOrDefault();

                if (existingDate != default(DateTime))
                {
                    model.CreatedDate = existingDate; // Restore the original creation date
                }
                else
                {
                    ModelState.AddModelError("", "Could not find original expense data. Update failed.");
                  
                    return View("CreateEditExpense", model); 
                }
            }

            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    _context.Expenses.Add(model);
                }
                else
                {
                    _context.Expenses.Update(model);
                }
                _context.SaveChanges();
                return RedirectToAction("Expenses"); 
            }

           
            return View("CreateEditExpense", model);
        }

        public IActionResult DeleteExpense(int id)
        {
            var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
            if (expenseInDb != null)
            {
                _context.Expenses.Remove(expenseInDb);
                _context.SaveChanges();
            }
            // Optionally add a TempData message here for success/failure
            return RedirectToAction("Expenses");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}