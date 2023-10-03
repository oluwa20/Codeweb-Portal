using System.ComponentModel.DataAnnotations;

namespace SMS.ViewModels
{
    public class StudentViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string? StudentName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string? Address { get; set; }
    }
}
