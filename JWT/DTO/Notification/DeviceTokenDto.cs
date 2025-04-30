using JWT;

namespace Edu_plat.DTO.Notification
{
    public class DeviceTokenDto
    {
       public string? DeviceToken { get; set; }
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? user { get; set; }
    }
}
