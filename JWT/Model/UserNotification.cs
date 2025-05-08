namespace Edu_plat.Model
{
    public class UserNotification

    {
        public int Id { get; set; }
        public string Body { get; set; }
        public string Title { get; set; }
        public DateOnly ? SentAt { get; set; } 

        public int? StudentId { get; set; }
        public Student? Student { get; set; }
        public int? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
