using Edu_plat.Model.Course_registeration;

namespace Edu_plat.Model.Exams
{
	public class Exam
	{
		public int Id { get; set; }
		public string ExamTitle { get; set; }  
		public DateTime StartTime { get; set; }  
		public double TotalMarks { get; set; } 
		public bool IsOnline { get; set; } 
		public int? QusetionsNumber { get; set; }
		public int DurationInMin { get; set; }
		public string CourseCode { get; set; }
		public string? Location { get; set; }
			public bool IsExamFinished()
			{
				var endTime = DateTime.SpecifyKind(StartTime, DateTimeKind.Utc).AddMinutes(DurationInMin);
				return DateTime.UtcNow > endTime;
			}


        public TimeSpan GetRemainingTime()
		{
			DateTime calculatedEndTime = StartTime.AddMinutes(DurationInMin);
			return calculatedEndTime - DateTime.UtcNow;
		}




		#region RelationShips

	
		public int CourseId { get; set; }  
		public Course Course { get; set; }  

		
		public List<Question>? Questions { get; set; } = new List<Question>();

	
		public int DoctorId { get; set; }
		public Doctor Doctor { get; set; }
		
		public List<ExamStudent> ExamStudents { get; set; } = new List<ExamStudent>(); 
		#endregion

	}
}
