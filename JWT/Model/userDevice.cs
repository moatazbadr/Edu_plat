using JWT;

namespace Edu_plat.Model
{
    public class userDevice
    {
        public int Id { get; set; }
        public string ? DeviceToken{ get; set; }  
       
        public int StudentId { get; set; }
        public Student? student { get; set; }
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

    }
}
