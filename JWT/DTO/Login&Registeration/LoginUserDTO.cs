using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class LoginUserDTO
	{
     
		
        public string email	{ get; set; }

		public string Password { get; set; }

	   public string? DeviceToken { get; set; }

	}
}
