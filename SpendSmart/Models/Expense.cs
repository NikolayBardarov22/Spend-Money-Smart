using System; // Required for DateTime
using System.ComponentModel.DataAnnotations;

namespace SpendSmartCREWAI.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required] // Value is essential
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than zero.")] // Add validation
        [DataType(DataType.Currency)] // Hint for display formatting
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Description is required.")] // Add error message
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Description must be between 3 and 100 characters.")] // Add length constraint
        public string Description { get; set; } // Non-nullable string (assuming C# 8+ nullable context enabled or it's required)

        // --- Start Modification ---
        [DataType(DataType.Date)] // Specifies the data type for display/editing hints
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] // Consistent date format
        [Display(Name = "Date Created")] // Nicer display name
        public DateTime CreatedDate { get; set; }
        // --- End Modification ---

        // Constructor to ensure Description is never null if required (optional, depends on nullability settings)
        public Expense()
        {
            Description = string.Empty; // Initialize to empty instead of null
            CreatedDate = DateTime.UtcNow; // Default to now, will be set properly in controller
        }
    }
}