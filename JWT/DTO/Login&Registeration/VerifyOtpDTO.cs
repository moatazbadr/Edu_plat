using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class VerifyOtpDTO
	{
		[Required(ErrorMessage = "OTP is required.")]
		[RegularExpression(@"^\d{5}$", ErrorMessage = "OTP must be exactly 5 digits.")]
		public string Otp { get; set; }
		
		[Required(ErrorMessage = "Email is required.")]
		[RegularExpression(@"^[a-zA-Z0-9._%+-]+@sci\.asu\.edu\.eg$", ErrorMessage = "Email must end with 'sci.asu.edu.eg'.")]
		[StringLength(40, ErrorMessage = "Email must not exceed 40 characters.")]
		public string email { get; set; }
	}
}
