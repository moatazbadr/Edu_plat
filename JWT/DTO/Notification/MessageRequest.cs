namespace Edu_plat.DTO.Notification
{
    public class MessageRequest
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string ? CourseCode { get; set; }
        public string? UserId { get; set; }
        public DateOnly Date { get; set; }

    }
}
