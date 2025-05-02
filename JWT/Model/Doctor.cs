using Edu_plat.Model.Exams;
using JWT;
using System.ComponentModel.DataAnnotations.Schema;

namespace Edu_plat.Model
{
	public class Doctor
	{
		// pk 
		public int DoctorId { get; set; }

		[ForeignKey("applicationUser")]
		public string? UserId { get; set; }
		public ApplicationUser? applicationUser { get; set; }
		public List<CourseDoctor> CourseDoctors { get; set; } = new List<CourseDoctor>();

	
		public List<Material> Materials { get; set; } = new List<Material>();
	
		public List<Exam> Exams { get; set; } = new List<Exam>();
        public ICollection<userDevice> userDevices { get; set; } = new List<userDevice>();

    }
}
