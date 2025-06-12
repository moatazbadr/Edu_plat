using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Edu_plat.DTO.UploadVideos;
using Edu_plat.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Edu_plat.DTO.UploadFiles;
using Edu_plat.DTO.Notification;
using Edu_plat.Services;

namespace Edu_plat.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class Videos : ControllerBase
	{

		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly INotificationHandler _notificationHandler;
        public Videos(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager ,
			INotificationHandler notificationHandler
			)
		{
			_context = context;
			_hostingEnvironment = hostingEnvironment;
			_userManager = userManager;
            _notificationHandler = notificationHandler;
        }

		#region UploadVideo 
		[HttpPost("UploadVideo")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UploadVideo([FromForm] UploadVideoDto uploadVideoDto)
		{
			
			if (!ModelState.IsValid)
			{
				return Ok(new { success = false, message = "Invalid data provided." });
			}

			
			if (uploadVideoDto.Video == null || uploadVideoDto.Video.Length == 0)
			{
				return	Ok(new { success = false, message = "No video uploaded." });
			}

			
			var maxFileSize = 100 * 1024 * 1024; // 100MB
			if (uploadVideoDto.Video.Length > maxFileSize)
			{
				return Ok(new { success = false, message = "Video size exceeds the maximum limit (100MB)." });
			}


				
				var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv" };
			var fileExtension = Path.GetExtension(uploadVideoDto.Video.FileName).ToLower();
			if (!allowedExtensions.Contains(fileExtension))
			{
				return Ok(new { success = false, message = "Only MP4, AVI, MOV, and MKV videos are allowed." });
			}
			
			var allowedMimeTypes = new Dictionary<string, string>
			{
					{ ".mp4", "video/mp4" },
					{ ".avi", "video/x-msvideo" },
					{ ".mov", "video/quicktime" },
					{ ".mkv", "video/x-matroska" }
			};

			
			var contentType = uploadVideoDto.Video.ContentType.ToLower();

			
			if (!allowedMimeTypes.ContainsKey(fileExtension))
			{
				return Ok(new { success = false, message = "Only MP4, AVI, MOV, and MKV videos are allowed." });
			}

			// Check if the content type matches the expected MIME type
			//if (allowedMimeTypes[fileExtension] != contentType)
			//{
			//	return Ok(new { success = false, message = "Invalid video file format." });
			//}


			
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == uploadVideoDto.CourseCode);
			if (course == null)
			{
				return Ok(new { success = false, message = "Course not found." });
			}

				
			var userId = User.FindFirstValue("ApplicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor not found." });
			}

			 
			bool isDoctorEnrolled = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
			if (!isDoctorEnrolled)
			{
				return Ok( new { success = false, message = "Doctor is not enrolled in this course." });
			}

			
			var videoDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Videos", course.CourseCode);
			if (!Directory.Exists(videoDirectory))
			{
				Directory.CreateDirectory(videoDirectory);
			}

			

			var fileName = Path.GetFileName(uploadVideoDto.Video.FileName);


			var filePath = Path.Combine(videoDirectory, fileName);

			
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await uploadVideoDto.Video.CopyToAsync(stream);
			}

			
			string fileSize = "Unknown";

			
			if (System.IO.File.Exists(filePath))
			{
				long fileSizeBytes = new FileInfo(filePath).Length;
				fileSize = (fileSizeBytes / (1024.0 * 1024.0)).ToString("F2") + " MB"; 
			}

			
			var videoMaterial = new Material
			{
				CourseId = course.Id, 
				CourseCode = course.CourseCode,
				DoctorId = doctor.DoctorId, 
				FilePath = $"/Videos/{course.CourseCode}/{fileName}", 
				FileName = fileName, 
				Description = "", 
				UploadDate = DateTime.Now, 
				TypeFile = "Videos",
				Size = fileSize

			};

			_context.Materials.Add(videoMaterial);
			await _context.SaveChangesAsync();


          


            await _notificationHandler.SendMessageAsync(new MessageRequest
            {
                Title = videoMaterial.CourseCode,
                Body = $"New {videoMaterial.FileName} uploaded by Dr {user.UserName}  : {videoMaterial.FileName} ",
                CourseCode = videoMaterial.CourseCode,
                UserId = userId,
                Date = DateOnly.Parse(videoMaterial.UploadDate.ToString("yyyy-MM-dd")),
            });
            return Ok(
            new
            {
                success = true,
                message = "File uploaded successfully.",
                FileDetails = new
                {
                    Id = videoMaterial.Id,
                    FileName = videoMaterial.FileName,
                    FilePath = videoMaterial.FilePath,
                    CourseCode = videoMaterial.CourseCode,
                    UploadDateFormatted = videoMaterial.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Size = fileSize,
                    FileExtension = fileExtension,
                    TypeFile = videoMaterial.TypeFile,

                }

            }
            );

        }
		#endregion

		#region UpdateVideo
		[HttpPut("UpdateVideo")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UpdateVideo([FromForm] UpdateVideoDto updateVideoDto)
		{
			
			if (!ModelState.IsValid)
			{
				return Ok(new { success = false, message = "Invalid data provided." });
			}

			
			if (updateVideoDto.Video == null || updateVideoDto.Video.Length == 0)
			{
				return Ok(new { success = false, message = "No video uploaded." });
			}

			
			var maxFileSize = 100 * 1024 * 1024;
			if (updateVideoDto.Video.Length > maxFileSize)
			{
				return Ok(new { success = false, message = "Video size exceeds the maximum limit (100MB)." });
			}

			
	   		var allowedMimeTypes = new Dictionary<string, string>
	        {
		       { ".mp4", "video/mp4" },
		       { ".avi", "video/x-msvideo" },
		       { ".mov", "video/quicktime" },
		       { ".mkv", "video/x-matroska" }
	        };

			var fileExtension = Path.GetExtension(updateVideoDto.Video.FileName).ToLower();
			var contentType = updateVideoDto.Video.ContentType.ToLower();

			if (!allowedMimeTypes.ContainsKey(fileExtension) || allowedMimeTypes[fileExtension] != contentType)
			{
				return Ok(new { success = false, message = "Invalid video file format." });
			}

			
			var videoMaterial = await _context.Materials.FindAsync(updateVideoDto.VideoId);
			if (videoMaterial == null)
			{
				return Ok(new { success = false, message = "Video not found." });
			}

			
			var userId = User.FindFirstValue("ApplicationUserId");
			var user = await _userManager.FindByIdAsync(userId);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor not found." });
			}

			
			if (videoMaterial.DoctorId != doctor.DoctorId)
			{
				return Ok( new { success = false, message = "You are not authorized to update this video." });
			}

			
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == videoMaterial.CourseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			
			bool isDoctorEnrolled = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
			if (!isDoctorEnrolled)
			{
				return StatusCode(403, new { success = false, message = "Doctor is not enrolled in this course." });
			}



			
			var videoDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Videos", course.CourseCode);
			if (!Directory.Exists(videoDirectory))
			{
				Directory.CreateDirectory(videoDirectory);
			}

		
			var fileName = Path.GetFileName(updateVideoDto.Video.FileName);


			var filePath = Path.Combine(videoDirectory, fileName);

			
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}

			
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await updateVideoDto.Video.CopyToAsync(stream);
			}

			

			videoMaterial.FilePath = $"/Videos/{course.CourseCode}/{fileName}";
			videoMaterial.FileName = fileName;
			videoMaterial.Description ="";
			videoMaterial.UploadDate = DateTime.Now;

            await _notificationHandler.SendMessageAsync(new MessageRequest
            {
                Title = videoMaterial.CourseCode,
                Body = $"New {videoMaterial.FileName} Updated by Dr {user.UserName}  : {videoMaterial.FileName} ",
                CourseCode = videoMaterial.CourseCode,
                UserId = userId,
                Date = DateOnly.Parse(videoMaterial.UploadDate.ToString("yyyy-MM-dd")),
            });
            await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Video updated successfully.", filePath = videoMaterial.FilePath });
		}
		#endregion

		#region DownloadByPath
		[HttpGet("DownloadByPath")]
		public IActionResult DownloadByPath([FromQuery] string pathVideo)
		{
			if (string.IsNullOrWhiteSpace(pathVideo))
			{
				return Ok(new { success = false, message = "Path video is required." });
			}

			var filePath = Path.Combine(_hostingEnvironment.WebRootPath, pathVideo.TrimStart('/'));

			if (!System.IO.File.Exists(filePath))
			{
				return Ok(new { success = false, message = "Video not found." });
			}

			var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var fileName = Path.GetFileName(filePath);
			return File(fileStream, "application/octet-stream", fileName);
		}


		#endregion

		#region DownloadById&CourseCode

		[HttpGet("DownloadById")]
		public async Task<IActionResult> DownloadById([FromQuery] int videoId, [FromQuery] string courseCode)
		{
			if (videoId <= 0 || string.IsNullOrWhiteSpace(courseCode))
			{
				return Ok(new { success = false, message = "VideoId and CourseCode are required." });
			}

			var videoMaterial = await _context.Materials
				.FirstOrDefaultAsync(m => m.Id == videoId && m.CourseCode == courseCode);

			if (videoMaterial == null)
			{
				return Ok(new { success = false, message = "Video not found." });
			}

			var filePath = Path.Combine(_hostingEnvironment.WebRootPath, videoMaterial.FilePath.TrimStart('/'));

			if (!System.IO.File.Exists(filePath))
			{
				return Ok(new { success = false, message = "Video file not found on server." });
			}

			var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var fileName = Path.GetFileName(filePath);
			return File(fileStream, "application/octet-stream", fileName);
		}

		#endregion

		#region Delete
		[HttpDelete("DeleteAllDoctorVideos")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllDoctorVideos()
		{
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor not found." });
			}

			var videos = await _context.Materials
				.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == "Video")
				.ToListAsync();

			if (!videos.Any())
			{
				return Ok(new { success = false, message = "No videos found." });
			}

			foreach (var video in videos)
			{
				if (System.IO.File.Exists(video.FilePath))
				{
					System.IO.File.Delete(video.FilePath);
				}
				_context.Materials.Remove(video);
			}
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "All videos deleted successfully." });
		}

		[HttpDelete("DeleteAllVideosInCourse/{courseCode}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteAllVideosInCourse(string courseCode)
		{
			if (string.IsNullOrWhiteSpace(courseCode))
			{
				return Ok(new { success = false, message = "CourseCode is required." });
			}
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return Ok(new { success = false, message = "Course not found." });
			}

			var videos = await _context.Materials
				.Where(m => m.CourseId == course.Id && m.TypeFile == "Video")
				.ToListAsync();

			if (!videos.Any())
			{
				return Ok(new { success = false, message = "No videos found in this course." });
			}

			foreach (var video in videos)
			{
				if (System.IO.File.Exists(video.FilePath))
				{
					System.IO.File.Delete(video.FilePath);
				}
				_context.Materials.Remove(video);
			}
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "All videos in course deleted successfully." });
		}

		[HttpDelete("DeleteVideoById/{videoId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteVideoById(int videoId)
		{
			if (videoId <= 0)
			{
				return Ok(new { success = false, message = "VideoId is required." });
			}
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor not found." });
			}

			var video = await _context.Materials.FirstOrDefaultAsync(m => m.Id == videoId && m.TypeFile == "Video");
			if (video == null)
			{
				return Ok(new { success = false, message = "Video not found." });
			}

			if (video.DoctorId != doctor.DoctorId)
			{
				return Ok( new { success = false, message = "You can only delete your own videos." });
			}

			if (System.IO.File.Exists(video.FilePath))
			{
				System.IO.File.Delete(video.FilePath);
			}

			_context.Materials.Remove(video);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Video deleted successfully." });
		}

		[HttpDelete("DeleteMultipleVideos")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteMultipleVideos([FromBody] List<int> videoIds)
		{
			if (videoIds == null || !videoIds.Any())
			{
				return Ok(new { success = false, message = "At least one VideoId is required." });
			}
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor not found." });
			}

			var videos = await _context.Materials
				.Where(m => videoIds.Contains(m.Id) && m.TypeFile == "Video")
				.ToListAsync();

			if (!videos.Any())
			{
				return Ok(new { success = false, message = "No videos found." });
			}

			foreach (var video in videos)
			{
				if (video.DoctorId != doctor.DoctorId)
				{
					return StatusCode(403, new { success = false, message = "You can only delete your own videos." });
				}
				if (System.IO.File.Exists(video.FilePath))
				{
					System.IO.File.Delete(video.FilePath);
				}
				_context.Materials.Remove(video);
			}
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Selected videos deleted successfully." });
		}

		#endregion

		#region GetAllVideosOfSpecificDoctor
		[HttpGet("getAllVideosOfDoctor/{courseCode}/{doctorId}")]
		public async Task<IActionResult> GetAllVideosOfSpecificDoctor(string courseCode, int doctorId)
		{
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == doctorId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor not found." });
			}

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return Ok(new { success = false, message = "Course not found." });
			}

			var videos = await _context.Materials
				.Where(m => m.DoctorId == doctorId && m.CourseCode == courseCode && m.TypeFile == "video")
				.ToListAsync();

			if (!videos.Any())
			{
				return Ok(new { success = false, message = "No videos found for this doctor in the specified course." });
			}

			return Ok(new { success = true, message = "Videos retrieved successfully.", videos });
		}
		#endregion

		#region GetAllVideosOfCourse
		[HttpGet("getAllVideosOfCourse/{courseCode}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> GetAllVideosOfCourse(string courseCode)
		{

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return NotFound(new { success = false, message = "Course not found." });
			}

			var videos = await _context.Materials
				.Where(m => m.CourseCode == courseCode && m.TypeFile == "video")
				.ToListAsync();

			if (!videos.Any())
			{
				return NotFound(new { success = false, message = "No videos found for this course." });
			}

			return Ok(new { success = true, videos });
		}

		#endregion

		#region GetDoctorVideosForCourseBasedOnType

		[HttpGet("getDoctorMaterials/{courseCode}/{userId}/{typeFile}")]
		[AllowAnonymous]
		
		public async Task<IActionResult> GetDoctorMaterialsForCourse(string courseCode, string userId, string typeFile)
		{
			
			if (string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(userId))
			{
				return Ok(new { success = false, message = "CourseCode and UserId are required." });
			}

			
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor not found for the provided UserId." });
			}

			
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return Ok(new { success = false, message = "Course not found." });
			}

			
			var materials = await _context.Materials
				.Where(m => m.CourseCode == courseCode && m.DoctorId == doctor.DoctorId && m.TypeFile == "Video")
				.ToListAsync();

			if (!materials.Any())
			{
				return Ok(new { success = false, message = "No Videos found for this doctor in the specified course." });
			}

			return Ok(new
			{
				success = true,
				message = "Videos retrieved successfully.",
				materials = materials
			});
		}

		#endregion

	}
}
		

