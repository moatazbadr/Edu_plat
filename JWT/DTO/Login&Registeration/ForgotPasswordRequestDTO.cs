using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class ForgotPasswordRequestDTO
	{

		[RegularExpression(@"^[a-zA-Z0-9._%+-]+@sci\.asu\.edu\.eg$", ErrorMessage = "Email must end with 'sci.asu.edu.eg'.")]
		[StringLength(40, ErrorMessage = "Email must not exceed 40 characters.")]
		[Required(ErrorMessage = "Email is required.")]
		public string Email { get; set; }
	}
}
