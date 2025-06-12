namespace Edu_plat.Model.Exams
{
	public class Question
	{
		public int Id { get; set; }
		public string QuestionText { get; set; }  
		public double Marks { get; set; }  

		public int TimeInMin { get; set; }
		
		
		
		public int ExamId { get; set; }
		public Exam Exam { get; set; }
		public List<Choice> Choices { get; set; } = new List<Choice>();

	}
}
