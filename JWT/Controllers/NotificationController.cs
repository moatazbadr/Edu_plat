using Edu_plat.DTO.Notification;
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

        [HttpPost]
        [Authorize(Roles ="Doctor,Admin")]
        public async Task<IActionResult> SendMessageAsync([FromBody] MessageRequest request)
        {

            #region validating user
            var userId = User.FindFirstValue("ApplicationUserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { success = false, message = "User not found" });
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Ok(new { success = false, message = "User not found" });
            } 
            #endregion


            #region validating request

            if (request == null)
            {
                return Ok(new { success = false, message = "Request cannot be null" });
            }
            if (string.IsNullOrEmpty(request.Title))
            {
                return Ok(new { success = false, message = "Title cannot be null" });
            }

            if (string.IsNullOrEmpty(request.Body))
            {
                return Ok(new { success = false, message = "Body cannot be null" });
            }

            if (string.IsNullOrEmpty(request.CourseCode))
            {
                return Ok(new { success = false, message = "CourseCode cannot be null" });
            }
            #endregion


            //check if the doctor is registered in this course
            #region checking if the doctor is registered to this course 
            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
            {
                return Ok(new { success = false, message = "Doctor not found" });
            }
            var doctorInCourse = _context.CourseDoctors.Include(cd => cd.Course)
                                                .FirstOrDefault(cd => cd.DoctorId == doctor.DoctorId && cd.Course.CourseCode == request.CourseCode);

            if (doctorInCourse==null)
            {
                return Ok(new { success = false, message = "Doctor is not registered in this course" });
            }
            #endregion

            #region validating Course

            var requiredCourse = _context.Courses.FirstOrDefault(c => c.CourseCode == request.CourseCode);

            if (requiredCourse == null)
            {

                return Ok(new { success = false, message = "Course not found" });
            }
           // return Ok(requiredCourse.CourseCode);
            #endregion

            #region Getting the students in that course
            var student =_context.Courses.Include(sc=>sc.Students).Where(s=>s.CourseCode== request.CourseCode)
                .SelectMany(s => s.Students).ToList();
            if (student == null || student.Count == 0)
            { return Ok(new { success = false, message = "No students found in this course" }); }
            return Ok(student);

            #endregion

            //var messaging = FirebaseMessaging.DefaultInstance;
            //var result = await messaging.SendAsync(message);

            //if (!string.IsNullOrEmpty(result))
            //{
            //    // Message was sent successfully
            //    return Ok("Message sent successfully!");
            //}
            //else
            //{
            //    // There was an error sending the message
            //    throw new Exception("Error sending the message.");
            //}
        }
    }
}
