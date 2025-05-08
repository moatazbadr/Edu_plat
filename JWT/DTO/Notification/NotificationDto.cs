namespace Edu_plat.DTO.Notification
{
    public class NotificationDto
    {
        public string Body { get; set; }
        public string Title { get; set; }
        public DateOnly? SentAt { get; set; }
    }
}
