using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SpendSmartCREWAI.Models;
using Microsoft.EntityFrameworkCore;
using System; // Required for DateTime

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
            // Prepare some data for the improved Index page (optional)
            ViewBag.TotalExpenses = _context.Expenses.Sum(e => (decimal?)e.Value) ?? 0m;
            ViewBag.ExpenseCount = _context.Expenses.Count();
            return View();
        }

        public IActionResult Expenses()
        {
            var allExpenses = _context.Expenses
                                      .OrderByDescending(e => e.CreatedDate) // Order by date by default
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

        // --- GET Action for Create/Edit Form ---
        public IActionResult CreateEditExpense(int? id)
        {
            if (id != null) // EDIT Case
            {
                var expenseInDb = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
                if (expenseInDb == null)
                {
                    return NotFound(); // Not found page if ID doesn't exist
                }
                // Pass the found expense to the view for editing
                return View(expenseInDb);
            }
            else // CREATE Case (id is null)
            {
                // *** ENSURE THIS LINE IS PRESENT AND CORRECT ***
                // Pass a new (non-null) Expense object to the view
                // This prevents the NullReferenceException in the view
                return View(new Expense());
            }
        }


        // --- POST Action for Saving Form Data ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEditExpenseForm(Expense model)
        {
            if (model.Id == 0) // Creating a new expense
            {
                model.CreatedDate = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(model.Description)) // Example server-side check
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
                    // Handle case where expense wasn't found (should ideally not happen if ID is set)
                    ModelState.AddModelError("", "Could not find original expense data. Update failed.");
                    // Return the view with the model to show error
                    return View("CreateEditExpense", model); // Return the same view on error
                }
            }

            // Re-check model state AFTER potentially adding errors or modifying model
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
                return RedirectToAction("Expenses"); // Redirect after successful save/update
            }

            // If model state is invalid, return the view with the current model
            // This ensures validation errors are shown and user doesn't lose input
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