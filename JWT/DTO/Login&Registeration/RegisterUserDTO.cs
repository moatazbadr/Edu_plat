using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class RegisterUserDTO
	{
		[RegularExpression(@"^[a-zA-Z]{3,}$", ErrorMessage = "Username must contain at least 3 English letters.")]
		[StringLength(15, ErrorMessage = "Username must not exceed 15 characters.")]
		[Required(ErrorMessage = "UserName is required.")]
		public string UserName { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@sci\.asu\.edu\.eg$", ErrorMessage = "Email must end with 'sci.asu.edu.eg'.")]
		[StringLength(40, ErrorMessage = "Email must not exceed 40 characters.")]
		[Required(ErrorMessage = "Email is required.")]
		public string Email { get; set; }  // Email Must end @'sci.asu.edu.eg

		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,15}$",
         ErrorMessage = "Password must be 8-15 characters long, and include at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
		[Required(ErrorMessage = "Password is required.")]
		public string Password { get; set; }  // with at least one uppercase letter, one lowercase letter, one digit, and one special character

		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }


	}
}
