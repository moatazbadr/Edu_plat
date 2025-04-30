namespace Edu_plat.DTO.Course_Registration
{
    public class ViewCourseDto
    {
        public string CourseCode { get; set; }
        public string CourseDescription { get; set; }
        public int Course_hours { get; set; }
        public bool has_Lab { get; set; }
        public int MidTerm { get; set; }
        public int Oral { get; set; }
        public int FinalExam { get; set; }
        public int Lab { get; set; }
        public int TotalMark { get; set; }
        public int Course_level { get; set; }
        public int Course_semster { get; set; }
    }
}
