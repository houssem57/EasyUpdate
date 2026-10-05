using System.ComponentModel.DataAnnotations;

namespace EasyUpdate.ViewModels
{
    public class VerifyEmailModel
    {
        [Required(ErrorMessage = "Email is Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }
}
