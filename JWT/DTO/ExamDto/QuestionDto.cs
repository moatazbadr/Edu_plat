namespace Edu_plat.DTO.ExamDto
{
	public class QuestionDto
	{
		public string QuestionText { get; set; }  
		public double Marks { get; set; }  

		public int TimeInMin { get; set; }
		public List<ChoiceDto> Choices { get; set; }
	}
}
