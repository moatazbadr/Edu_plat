namespace Edu_plat.DTO.Notification
{
    public class MessageRequest
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
    
        // to get all stduent that are resgisterd in this course
        public string ? CourseCode { get; set; }
    }
}
