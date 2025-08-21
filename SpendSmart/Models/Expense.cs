using System; 
using System.ComponentModel.DataAnnotations;

namespace SpendSmartCREWAI.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be greater than zero.")]
        [DataType(DataType.Currency)]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Description must be between 3 and 100 characters.")]
        public string Description { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Created")]
        public DateTime CreatedDate { get; set; }

        public Expense()
        {
            Description = string.Empty;
            CreatedDate = DateTime.UtcNow;
        }
    }
}