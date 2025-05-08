using Edu_plat.DTO.Notification;

namespace Edu_plat.Services
{
    public interface INotificationHandler
    {
        Task<(int successCount, int failureCount)> SendMessageAsync(MessageRequest request);

        //[userType=> students,Doctors,both]
        Task<(int successCount, int failureCount)> AdminAnnouncement(MessageRequest request, string userType); 


    }
}
