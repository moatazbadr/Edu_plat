using Edu_plat.Model.Course_registeration;
using Edu_plat.Model.Exams;
using JWT;
using System.ComponentModel.DataAnnotations.Schema;

namespace Edu_plat.Model
{
	public class Student
	{
		
		public int StudentId { get; set; }

		[ForeignKey("applicationUser")]
		public string? UserId { get; set; }
		public ApplicationUser? applicationUser { get; set; }
		
		public double ? GPA { get; set; }


		public ICollection<Course> courses { get; set; } = new List<Course>();
	
		public List<ExamStudent> ExamStudents { get; set; } = new List<ExamStudent>();
        public ICollection<userDevice> userDevices { get; set; } = new List<userDevice>();

    }
}
