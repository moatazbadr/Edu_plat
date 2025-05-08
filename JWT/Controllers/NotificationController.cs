using Edu_plat.DTO.Notification;
using Edu_plat.Model;
using FirebaseAdmin.Messaging;
using JWT;
using JWT.DATA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
         private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager = null)
        {
            _context = context;
            _userManager = userManager;
        }

        #region Notification
        //[HttpPost]
        //[Authorize(Roles = "Doctor,Admin")]
        //public async Task<IActionResult> SendMessageAsync([FromBody] MessageRequest request)
        //{
        //    var userId = User.FindFirstValue("ApplicationUserId");
        //    if (string.IsNullOrEmpty(userId))
        //        return Ok(new { success = false, message = "User not found" });

        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //        return Ok(new { success = false, message = "User not found" });


        //    if (request == null || string.IsNullOrEmpty(request.Title) ||
        //        string.IsNullOrEmpty(request.Body) || string.IsNullOrEmpty(request.CourseCode))
        //    {
        //        return Ok(new { success = false, message = "Invalid request: Title, Body and CourseCode are required" });
        //    }


        //    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        //    if (doctor == null)
        //        return Ok(new { success = false, message = "Doctor not found" });

        //    var doctorInCourse = await _context.CourseDoctors
        //        .Include(cd => cd.Course)
        //        .FirstOrDefaultAsync(cd => cd.DoctorId == doctor.DoctorId && cd.Course.CourseCode == request.CourseCode);

        //    if (doctorInCourse == null)
        //        return Ok(new { success = false, message = "Doctor is not registered in this course" });


        //    var course = await _context.Courses
        //        .Include(c => c.Students)
        //        .ThenInclude(s => s.userDevices)
        //        .FirstOrDefaultAsync(c => c.CourseCode == request.CourseCode);

        //    if (course == null)
        //        return Ok(new { success = false, message = "Course not found" });

        //    var students = course.Students;
        //    if (students == null || !students.Any())
        //        return Ok(new { success = false, message = "No students found in this course" });
        //var messaging = FirebaseMessaging.DefaultInstance;
        //    int successCount = 0, failureCount = 0;

        //    foreach (var student in students)
        //    {
        //        foreach (var device in student.userDevices)
        //        {
        //            if (string.IsNullOrWhiteSpace(device.DeviceToken))
        //                continue;

        //            var message = new Message()
        //            {
        //                Notification = new Notification
        //                {
        //                    Title = request.Title,
        //                    Body = request.Body
        //                },
        //                Token = device.DeviceToken,
        //                Data = new Dictionary<string, string>
        //                {
        //                    ["CourseCode"] = request.CourseCode,
        //                    ["StudentId"] = student.StudentId.ToString()
        //                }
        //            };

        //            try
        //            {
        //                var result = await messaging.SendAsync(message);
        //                if (!string.IsNullOrEmpty(result))
        //                    successCount++;
        //                else
        //                    failureCount++;
        //            }
        //            catch
        //            {
        //                failureCount++;

        //            }
        //        }
        //    }

        //    return Ok(new
        //    {
        //        success = true,
        //        message = $"Notifications sent: {successCount} succeeded, {failureCount} failed."
        //    });
        //}

        #endregion

        #region Tyring Notification

        //[HttpPost]
        //public async Task<IActionResult> SendMessageAsync([FromBody] MessageRequest request)
        //{
        //    var message = new Message()
        //    {
        //        Notification = new Notification
        //        {
        //            Title = request.Title,
        //            Body = request.Body,
        //        },
        //        Data = new Dictionary<string, string>()
        //        {
        //            ["FirstName"] = "John",
        //            ["LastName"] = "Doe"
        //        },
        //    };

        //    var messaging = FirebaseMessaging.DefaultInstance;
        //    var result = await messaging.SendAsync(message);

        //    if (!string.IsNullOrEmpty(result))
        //    {

        //        return Ok(new { success=true,message="Message sent successfully!" });
        //    }
        //    else
        //    {

        //        return Ok(new { success = false ,message="Not sent"});
        //    }
        //}

        #endregion

        #region getting studentNotification
        [HttpGet("notifications")]
        [Authorize(Roles = "Student,Doctor")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue("ApplicationUserId");

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { success = false, message = "User not found" });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return BadRequest(new { success = false, message = "User not found" });

            List<UserNotification> notifications;

            if (User.IsInRole("Student"))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                if (student == null)
                    return BadRequest(new { success = false, message = "Student not found" });

                notifications = await _context.UserNotifications
                    .Where(n => n.StudentId == student.StudentId)
                    .OrderByDescending(n => n.SentAt)
                    .ToListAsync();
            }
            else if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null)
                    return BadRequest(new { success = false, message = "Doctor not found" });

                notifications = await _context.UserNotifications
                    .Where(n => n.DoctorId == doctor.DoctorId)
                    .OrderByDescending(n => n.SentAt)
                    .ToListAsync();
            }
            else
            {
                return Forbid();
            }

            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Title = n.Title,
                Body = n.Body,
                SentAt = n.SentAt
            }).ToList();

            return Ok(new
            {
                success = true,
                message = "Fetched successfully",
                NotificationHistory = notificationDtos
            });
        }


        #endregion



    }
}
