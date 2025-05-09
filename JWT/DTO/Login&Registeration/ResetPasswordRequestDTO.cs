using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class ResetPasswordRequestDTO
	{
		[Required(ErrorMessage = "Email is required.")]
		[RegularExpression(@"^[a-zA-Z0-9._%+-]+@sci\.asu\.edu\.eg$", ErrorMessage = "Email must end with 'sci.asu.edu.eg'.")]
		[StringLength(40, ErrorMessage = "Email must not exceed 40 characters.")]
		public string Email { get; set; }
	
		[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,15}$",
		 ErrorMessage = "Password must be 8-15 characters long, and include at least one lowercase letter, one uppercase letter, one digit, and one special character.")]
		[Required(ErrorMessage = "Password is required.")]
		public string NewPassword { get; set; }
		
		[Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.") ]
		public string ConfirmPassword { get; set; }
		
	}
}
