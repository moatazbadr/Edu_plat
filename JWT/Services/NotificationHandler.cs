using Edu_plat.DTO.Notification;
using FirebaseAdmin.Messaging;
using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Edu_plat.Model;

namespace Edu_plat.Services
{
    public class NotificationHandler : INotificationHandler
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(int successCount, int failureCount)> AdminAnnouncement(MessageRequest request, string userType)
        {
            if (string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.Body)) 
                return (0, 0);

            var messaging = FirebaseMessaging.DefaultInstance;
            int successCount = 0, failureCount = 0;

            if (userType == "students" || userType == "both")
            {
                var students = await _context.Students
                    .Include(s => s.userDevices)
                    .ToListAsync();

                foreach (var student in students)
                {
                    foreach (var device in student.userDevices)
                    {
                        if (string.IsNullOrWhiteSpace(device.DeviceToken))
                            continue;

                        var message = new Message()
                        {
                            Notification = new Notification
                            {
                                Title = request.Title,
                                Body = request.Body
                            },
                            Token = device.DeviceToken,
                            Data = new Dictionary<string, string>
                            {
                                ["CourseCode"] = request.CourseCode!,
                                ["StudentId"] = student.StudentId.ToString()
                            }
                        };

                        try
                        {
                            var result = await messaging.SendAsync(message);
                            if (!string.IsNullOrEmpty(result))
                                successCount++;
                            else
                                failureCount++;
                        }
                        catch
                        {
                            failureCount++;
                        }

                        _context.UserNotifications.Add(new UserNotification
                        {
                            Title = request.Title!,
                            Body = request.Body!,
                            StudentId = student.StudentId,
                            SentAt = DateOnly.FromDateTime(DateTime.UtcNow),
                        });
                    }
                }
            }

            if (userType == "doctors" || userType == "both")
            {
                var doctors = await _context.Doctors
                    .Include(d => d.userDevices)
                    .ToListAsync();

                foreach (var doctor in doctors)
                {
                    foreach (var device in doctor.userDevices)
                    {
                        if (string.IsNullOrWhiteSpace(device.DeviceToken))
                            continue;

                        var message = new Message()
                        {
                            Notification = new Notification
                            {
                                Title = request.Title,
                                Body = request.Body
                            },
                            Token = device.DeviceToken,
                            Data = new Dictionary<string, string>
                            {
                                ["CourseCode"] = request.CourseCode!,
                                ["DoctorId"] = doctor.DoctorId.ToString()
                            }
                        };

                        try
                        {
                            var result = await messaging.SendAsync(message);
                            if (!string.IsNullOrEmpty(result))
                                successCount++;
                            else
                                failureCount++;
                        }
                        catch
                        {
                            failureCount++;
                        }

                        _context.UserNotifications.Add(new UserNotification
                        {
                            Title = request.Title!,
                            Body = request.Body!,
                            DoctorId = doctor.DoctorId,
                            SentAt = DateOnly.FromDateTime(DateTime.UtcNow),
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return (successCount, failureCount);
        }


        public async Task<(int successCount, int failureCount)> SendMessageAsync(MessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.Body) ||
                string.IsNullOrWhiteSpace(request.CourseCode))
                return (0, 0);

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == request.UserId);
            if (doctor == null)
                return (0, 0);

            var doctorInCourse = await _context.CourseDoctors
                .Include(cd => cd.Course)
                .FirstOrDefaultAsync(cd => cd.DoctorId == doctor.DoctorId && cd.Course.CourseCode == request.CourseCode);

            if (doctorInCourse == null)
                return (0, 0);

            var course = await _context.Courses
                .Include(c => c.Students)
                    .ThenInclude(s => s.userDevices)
                .FirstOrDefaultAsync(c => c.CourseCode == request.CourseCode);

            if (course == null)
                return (0, 0);

            var students = course.Students;

            var messaging = FirebaseMessaging.DefaultInstance;
            int successCount = 0, failureCount = 0;

            foreach (var student in students)
            {
                foreach (var device in student.userDevices)
                {
                    if (string.IsNullOrWhiteSpace(device.DeviceToken))
                        continue;

                    var message = new Message()
                    {
                        Notification = new Notification
                        {
                            Title = request.CourseCode,
                            Body = request.Body
                        },
                        Token = device.DeviceToken,
                        Data = new Dictionary<string, string>
                        {
                            ["CourseCode"] = request.CourseCode!,
                            ["StudentId"] = student.StudentId.ToString()
                        }
                    };
                    try
                    {
                        var result = await messaging.SendAsync(message);
                        if (!string.IsNullOrEmpty(result))
                            successCount++;
                        else
                            failureCount++;
                    }
                    catch
                    {
                        failureCount++;
                    }

                    // Store in notification history
                    _context.UserNotifications.Add(new UserNotification
                    {
                        Title = request.CourseCode!,
                        Body = request.Body!,
                        StudentId = student.StudentId,
                        SentAt = DateOnly.Parse(DateTime.UtcNow.ToString("yyyy-MM-dd")),
                    });
                }
            }

            await _context.SaveChangesAsync();

            return (successCount, failureCount);
        }

       
    }
}
