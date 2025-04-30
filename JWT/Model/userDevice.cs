using JWT;

namespace Edu_plat.Model
{
    public class userDevice
    {
        public int Id { get; set; }
        public string ? DeviceToken{ get; set; }  
       
        public string ? ApplicationUserId { get; set; }
        public ApplicationUser? user { get; set; }

    }
}
