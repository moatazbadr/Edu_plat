using Edu_plat.DTO.FileRequester;
using Edu_plat.DTO.Notification;
using Edu_plat.DTO.UploadFiles;
using Edu_plat.Model;
using Edu_plat.Model.Course_registeration;
using Edu_plat.Model.Exams;
using Edu_plat.Requests;
using Edu_plat.Responses;
using Edu_plat.Services;
using JWT;
using JWT.DATA;
using JWT.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Edu_plat.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MaterialsController : ControllerBase
	{
        private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationHandler _notificationHandler;
        public MaterialsController
			(
			ApplicationDbContext context,
			IWebHostEnvironment hostingEnvironment
			, UserManager<ApplicationUser> userManager
			,INotificationHandler notificationHandler
			)
		{
			_context = context;
			_hostingEnvironment = hostingEnvironment;
			_userManager = userManager;
            _notificationHandler = notificationHandler;

        } 
		
        #region UploadFile
		[HttpPost("UploadFile/Doctors")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UploadMaterial([FromForm] UploadMatarielDto uploadMaterialDto)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new { success = false, message = "Invalid data provided." });
			}
			if (uploadMaterialDto.File == null || uploadMaterialDto.File.Length == 0)
			{
				return Ok(new { success = false, message = "No file uploaded." });
			}
			var maxFileSize = 10 * 1024 * 1024; // 10 MB
			if (uploadMaterialDto.File.Length > maxFileSize)
			{
				return Ok(new { success = false, message = "File size exceeds the maximum limit (10 MB)." });
			}
			var contentType = uploadMaterialDto.File.ContentType.ToLower();
			var fileExtension = Path.GetExtension(uploadMaterialDto.File.FileName).ToLower();

			var allowedContentTypes = new Dictionary<string, string>
			 {
		       {  ".pdf", "application/pdf" },
		       { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
		       { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
	         };

			if (!allowedContentTypes.ContainsKey(fileExtension))
			{
				return Ok(new { success = false, message = "Only PDF, Word, and PowerPoint files are allowed." });
			}

			var course = await _context.Courses
				.Where(c => c.CourseCode == uploadMaterialDto.CourseCode)
				.FirstOrDefaultAsync();

			if (course == null)
			{
				return Ok(new { success = false, message = "Course not found." });
			}

			var userId = User.FindFirstValue("ApplicationUserId");
			if (string.IsNullOrEmpty(userId))
			{
				return Ok(new { success = false, message = "user not found" });

			}
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Ok(new { success = false, message = "User not found." });
			}

			bool isDoctorEnrolled = await _context.CourseDoctors
				.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

			if (!isDoctorEnrolled)
			{
				return Ok( new { success = false, message = "Doctor is not enrolled in the course, so file upload is forbidden." });
			}

			var doctor = await _context.CourseDoctors
				.Where(cd => cd.CourseId == course.Id && cd.Doctor.UserId == userId)
				.FirstOrDefaultAsync();

			if (doctor == null)
			{
				return Ok(new { success=false ,message="doctor doesn't exist"});
			}
			var uploadsDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", course.CourseCode);

			
			if (!Directory.Exists(uploadsDirectory))
			{
				Directory.CreateDirectory(uploadsDirectory);
			}

			
			var fileBaseName = Path.GetFileNameWithoutExtension(uploadMaterialDto.File.FileName);	
			var existingFiles = Directory.GetFiles(uploadsDirectory, $"{fileBaseName}*");
			var uniqueFileName = existingFiles.Length > 0
				? $"{fileBaseName}_{existingFiles.Length + 1}{fileExtension}"
				: $"{fileBaseName}{fileExtension}"; 

			var filePath = Path.Combine(uploadsDirectory, uniqueFileName);
			
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await uploadMaterialDto.File.CopyToAsync(stream);
			}			
			string fileSize = "Unknown";

			if (System.IO.File.Exists(filePath))
			{
				long fileSizeBytes = new FileInfo(filePath).Length;
				fileSize = (fileSizeBytes / (1024.0 * 1024.0)).ToString("F2") + " MB"; 
			}
			
			var material = new Material
			{
				CourseId = course.Id,  
				CourseCode = course.CourseCode,
				DoctorId = doctor.DoctorId,   
				FilePath = $"/Uploads/{course.CourseCode}/{uniqueFileName}",  
				FileName = uniqueFileName,
				Description = string.Empty,
				UploadDate = DateTime.Now,
				TypeFile = uploadMaterialDto.Type,
				Size = fileSize

			};

			_context.Materials.Add(material);
			await _context.SaveChangesAsync();

			double fileSizeInMB = uploadMaterialDto.File.Length / (1024.0 * 1024.0);
			Console.WriteLine(fileSizeInMB);


            await _notificationHandler.SendMessageAsync(new MessageRequest
            {
                Title =material.CourseCode,
                Body = $"New {material.TypeFile} uploaded  : {material.FileName} by Dr {user.UserName}",
                CourseCode = material.CourseCode,
                UserId = userId,
                Date = DateOnly.Parse(material.UploadDate.ToString("yyyy-MM-dd")),
            }
			);

            return Ok(
			new
			{
				success = true,
				message = "File uploaded successfully.",
				FileDetails = new
				{
					Id = material.Id,
                    FileName=material.FileName,
                    FilePath = material.FilePath,
                    CourseCode=material.CourseCode,
                    UploadDateFormatted= material.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Size = fileSize,
                    FileExtension = fileExtension,
                    TypeFile = uploadMaterialDto.Type,

                }
			}
			);
		}
		#endregion

		#region RetriveAllFiles 
		[HttpGet("AllFiles")]
		public async Task<IActionResult> GetAllFiles()
		{
			var materials = await _context.Materials.ToListAsync();

			if (materials == null || !materials.Any())
			{
				return NotFound("No materials found.");
			}

			return Ok(materials);
		}
		#endregion

		#region Method Pass MaterialId Return FileName [Gets The Axe]
		private string GetFileNameByMaterialId(int materialId)
		{

			var material = _context.Materials.FirstOrDefault(m => m.Id == materialId);
			if (material != null)
			{
				return material.FileName;
			}
			return null;
		}
		#endregion

        #region Get Doctor Material by type

        [HttpGet("Get-Material-ByType/courseCode/typeFile")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> GetDoctorMaterialsByTypeAndCourse(string courseCode,string typeFile)
		{

			var userId = User.FindFirstValue("ApplicationUserId");

			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Ok(new { success = true, message = "User not found." });
			}

			
			if (string.IsNullOrEmpty(typeFile) || string.IsNullOrEmpty(courseCode))
			{
				return Ok(new { success = true, message = "TypeFile and CourseCode are required." });
			}

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
			if (course == null)
			{
				return Ok(new { success = true, message = "Course not found." });
			}
			bool isDoctorEnrolled = await _context.CourseDoctors
				.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

			if (!isDoctorEnrolled)
			{
				return Ok( new { success = true, message = "Doctor is not enrolled in this course." });
			}

		
			
			var doctorMaterials = await _context.Materials
				.Where(m => m.Doctor.UserId == userId
							&& m.TypeFile.ToLower() == typeFile.ToLower()
							&& m.CourseCode.ToLower() == courseCode.ToLower())
				.Select(m => new
				{
					m.Id,
					m.FileName,
					m.FilePath,
					m.CourseCode,
					UploadDateFormatted = m.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"), // 2025-02-16 11:56:01m.TypeFile,
					FileExtension = Path.GetExtension(m.FileName),
					m.Size,
					m.TypeFile
					
				})
				.ToListAsync();
		
			
			if (!doctorMaterials.Any())
			{
				return Ok(new { success = true, message = $"No {typeFile}  found for this doctor in course {courseCode}.", materials = new List<object>() });
			}

			return Ok(new { success = true,message="Fetching Material Completed", materials = doctorMaterials });
		
			
		}
        #endregion

        // -------------------------------------------------------------------------------------------------------------------------------------------------
        #region Update Files 
        [HttpPut("updateFile")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UpdateMaterial([FromForm] UpdateMaterialDto updateMaterialDto)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new { success = false, message = "Invalid data provided." });
			}

			
			if (updateMaterialDto.File == null || updateMaterialDto.File.Length == 0)
			{
				return Ok(new { success = false, message = "No file uploaded." });
			}

			
			var maxFileSize = 10 * 1024 * 1024; 
			if (updateMaterialDto.File.Length > maxFileSize)
			{
				return Ok(new { success = false, message = "File size exceeds the maximum limit (10 MB)." });
			}

			var fileExtension = Path.GetExtension(updateMaterialDto.File.FileName).ToLower();
			var allowedContentTypes = new Dictionary<string, string>
	        {
		       { ".pdf", "application/pdf" },
		       { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
		       { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
	        };

			var userId = User.FindFirstValue("ApplicationUserId");

			if (string.IsNullOrEmpty(userId))
			{
                return Ok(new { success = false, message = "User not found." });

            }

            var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return Ok(new { success = false, message = "User not found." });
			}

			var material = await _context.Materials.FindAsync(updateMaterialDto.Material_Id);
			if (material == null)
			{
				return Ok(new { success = false, message = "Material not found." });
			}

		
			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == material.CourseCode);
			if (course == null)
			{
				return Ok(new { success = false, message = "Course not found." });
			}

			
			bool isDoctorEnrolled = await _context.CourseDoctors
				.AnyAsync(cd => cd.Doctor.UserId == userId && cd.CourseId == course.Id);

			if (!isDoctorEnrolled)
			{
				return Ok( new { success = false, message = "Doctor is not enrolled in this course." });
			}

		
			var uploadsDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", course.CourseCode);
			if (!Directory.Exists(uploadsDirectory))
			{
				Directory.CreateDirectory(uploadsDirectory);
			}

		
			var fileBaseName = Path.GetFileNameWithoutExtension(updateMaterialDto.File.FileName);
			var existingFiles = Directory.GetFiles(uploadsDirectory, $"{fileBaseName}*{fileExtension}");

			int maxNumber = 0;
			foreach (var existingFile in existingFiles)
			{
				var existingFileName = Path.GetFileNameWithoutExtension(existingFile);
				var match = Regex.Match(existingFileName, @$"{Regex.Escape(fileBaseName)}_(\d+)$");
				if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
				{
					maxNumber = Math.Max(maxNumber, num);
				}
			}

			var uniqueFileName = maxNumber > 0
				? $"{fileBaseName}_{maxNumber + 1}{fileExtension}"
				: $"{fileBaseName}_1{fileExtension}";
			var newFilePath = Path.Combine(uploadsDirectory, uniqueFileName);

			using (var stream = new FileStream(newFilePath, FileMode.Create))
			{
				await updateMaterialDto.File.CopyToAsync(stream);
			}
			var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
			if (System.IO.File.Exists(oldFilePath))
			{
				System.IO.File.Delete(oldFilePath);
			}
			
			long fileSizeBytes = new FileInfo(newFilePath).Length;
			material.FileName = uniqueFileName;
			material.FilePath = $"/Uploads/{course.CourseCode}/{uniqueFileName}";
			material.UploadDate = DateTime.Now;
			material.TypeFile = updateMaterialDto.Type;
			material.Size = (fileSizeBytes / (1024.0 * 1024.0)).ToString("F2") + " MB"; 
			_context.Materials.Update(material);
			await _context.SaveChangesAsync();

            await _notificationHandler.SendMessageAsync(new MessageRequest
            {
                Title = material.CourseCode,
                Body = $"New {material.TypeFile} updated : {material.FileName} by Dr {user.UserName}  ",
                CourseCode = material.CourseCode,
                UserId = userId,
                Date = DateOnly.Parse(material.UploadDate.ToString("yyyy-MM-dd")),
            }
           );

            return Ok(
			new
			{
				success = true,
				message = "File updated successfully.",
			    FileDetails = new
            {
                Id = material.Id,
                FileName = material.FileName,
                FilePath = material.FilePath,
                CourseCode = material.CourseCode,
                UploadDateFormatted = material.UploadDate.ToString("yyyy-MM-dd HH:mm:ss"),
                Size = material.Size,
                FileExtension = fileExtension,
                TypeFile = updateMaterialDto.Type,

            }

        }
            );
		}

        #endregion

        //--------------------------------------------------------------------------------------------------------------------------------------------------

        #region DeleteAllFilesOfCertainDoctor [Gets The Axe]

  //      [HttpDelete("Delete-All-Doctor-Materials")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> DeleteAllMaterials()
		//{
		//	var userId = User.FindFirstValue("ApplicationUserId"); 

		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
		//	if (doctor == null)
		//	{
		//		return NotFound(new { success = false, message = "Doctor not found." });
		//	}

		//	var materials = await _context.Materials.Where(m => m.DoctorId == doctor.DoctorId).ToListAsync();

		//	if (!materials.Any())
		//	{
		//		return NotFound(new { success = false, message = "No materials found for this doctor." });
		//	}
		//	foreach (var material in materials)
		//	{
		//		var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
		//		if (System.IO.File.Exists(filePath))
		//		{
		//			System.IO.File.Delete(filePath);
		//		}
		//	}
  //          _context.Materials.RemoveRange(materials);
		//	await _context.SaveChangesAsync();

		//	// .net 7
		//	//await _context.Materials
		//	//      .Where(m => m.DoctorId == doctor.DoctorId)
		//	//.ExecuteDeleteAsync();


		//	return Ok(new { success = true, message = "All materials deleted successfully." });
		//}
		#endregion

		#region DeleteSomeMaterialOfSpeceficDoctor
		//[HttpDelete("deleteMultipleMaterialOfSpeceficDoctor")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> DeleteMultipleMaterials([FromQuery] List<int> materialIds)
		//{
		//	var userId = User.FindFirstValue("ApplicationUserId");

		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
		//	if (doctor == null)
		//	{
		//		return NotFound(new { success = false, message = "Doctor not found." });
		//	}

		//	var materials = await _context.Materials
		//		.Where(m => m.DoctorId == doctor.DoctorId && materialIds.Contains(m.Id))
		//		.ToListAsync();

		//	if (!materials.Any())
		//	{
		//		return NotFound(new { success = false, message = "Some materials were not found or do not belong to the current doctor." });
		//	}

		//	// delete files from Server
		//	foreach (var material in materials)
		//	{
		//		var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
		//		if (System.IO.File.Exists(filePath))
		//		{
		//			System.IO.File.Delete(filePath);
		//		}
		//	}

		//	_context.Materials.RemoveRange(materials);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { success = true, message = "Selected materials deleted successfully." });
		//}
		#endregion

		#region DeleteOnlyMaterialIdOfDoctor
		[HttpDelete("delete/{materialId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteMaterial(int materialId)
		{
			var userId = User.FindFirstValue("ApplicationUserId");  

			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return NotFound(new { success = false, message = "Doctor not found." });
			}

			var material = await _context.Materials.FirstOrDefaultAsync(m => m.Id == materialId && m.DoctorId == doctor.DoctorId);

			if (material == null)
			{
				return NotFound(new { success = false, message = "Material not found." });
			}

			// حذف الملف من السيرفر
			var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}

			_context.Materials.Remove(material);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, message = "Material deleted successfully." });
		}

		#endregion

		#region DeleteAllLecMaterialsOfDoctorFromToken

		//[HttpDelete("deleteAllLec/Doctor")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> DeleteAllLecMaterialsOfDoctor()
		//{
		//	// Get the UserId from the token
		//	var userId = User.FindFirstValue("ApplicationUserId");

		//	// Search for the doctor using the UserId
		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

		//	if (doctor == null)
		//	{
		//		return NotFound(new { success = false, message = "Doctor not found." });
		//	}

		//	// Get all "Lec" type materials related to the doctor
		//	var lecMaterials = await _context.Materials
		//		.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == "Material") // Specify "Lec" material type
		//		.ToListAsync();

		//	if (!lecMaterials.Any())
		//	{
		//		return NotFound(new { success = false, message = "No Lec materials found for this doctor." });
		//	}

		//	// Delete the files from the server
		//	foreach (var material in lecMaterials)
		//	{
		//		var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
		//		if (System.IO.File.Exists(filePath))
		//		{
		//			System.IO.File.Delete(filePath);
		//		}
		//	}

		//	// Remove materials from the database
		//	_context.Materials.RemoveRange(lecMaterials);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { success = true, message = "All Lec materials deleted successfully." });
		//}

		#endregion

        #region DeleteAllLabMaterialsOfDoctorFromToken

		//[HttpDelete("deleteAllLab/Doctor")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> DeleteAllLabMaterialsOfDoctor()
		//{
		//	// Get the UserId from the token
		//	var userId = User.FindFirstValue("ApplicationUserId");

		//	// Search for the doctor using the UserId
		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

		//	if (doctor == null)
		//	{
		//		return NotFound(new { success = false, message = "Doctor not found." });
		//	}

		//	// Get all "lab" type materials related to the doctor
		//	var labMaterials = await _context.Materials
		//		.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == "Labs") // Specify "lab" material type
		//		.ToListAsync();

		//	if (!labMaterials.Any())
		//	{
		//		return NotFound(new { success = false, message = "No lab materials found for this doctor." });
		//	}

		//	// Delete the files from the server
		//	foreach (var material in labMaterials)
		//	{
		//		var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
		//		if (System.IO.File.Exists(filePath))
		//		{
		//			System.IO.File.Delete(filePath);
		//		}
		//	}

		//	// Remove materials from the database
		//	_context.Materials.RemoveRange(labMaterials);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { success = true, message = "All lab materials deleted successfully." });
		//}

		#endregion

        #region DeleteAllExamMaterialsOfDoctorFromToken

		//[HttpDelete("deleteAllExam/Doctor")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> DeleteAllExamMaterialsOfDoctor()
		//{
		//	// Get the UserId from the token
		//	var userId = User.FindFirstValue("ApplicationUserId");

		//	// Search for the doctor using the UserId
		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

		//	if (doctor == null)
		//	{
		//		return NotFound(new { success = false, message = "Doctor not found." });
		//	}

		//	// Get all "exam" type materials related to the doctor
		//	var examMaterials = await _context.Materials
		//		.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == "Exams") // Specify "exam" material type
		//		.ToListAsync();

		//	if (!examMaterials.Any())
		//	{
		//		return NotFound(new { success = false, message = "No Exam materials found for this doctor." });
		//	}

		//	// Delete the files from the server
		//	foreach (var material in examMaterials)
		//	{
		//		var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
		//		if (System.IO.File.Exists(filePath))
		//		{
		//			System.IO.File.Delete(filePath);
		//		}
		//	}

		//	// Remove materials from the database
		//	_context.Materials.RemoveRange(examMaterials);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { success = true, message = "All exam materials deleted successfully." });
		//}

		#endregion

        #region DeleteAllMaterialsOfTypeForDoctor

		//[HttpDelete("deleteMaterialsByType/Doctor")]
		//[Authorize(Roles = "Doctor")]
		//public async Task<IActionResult> DeleteAllMaterialsOfTypeForDoctor([FromQuery] string typeFile)
		//{
		//	if (string.IsNullOrEmpty(typeFile))
		//	{
		//		return BadRequest(new { success = false, message = "TypeFile is required." });
		//	}

		//	// Get the UserId from the token
		//	var userId = User.FindFirstValue("ApplicationUserId");

		//	// Search for the doctor using the UserId
		//	var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

		//	if (doctor == null)
		//	{
		//		return NotFound(new { success = false, message = "Doctor not found." });
		//	}

		//	// Get all materials of the specified type related to the doctor
		//	var materials = await _context.Materials
		//		.Where(m => m.DoctorId == doctor.DoctorId && m.TypeFile == typeFile)
		//		.ToListAsync();

		//	if (!materials.Any())
		//	{
		//		return NotFound(new { success = false, message = $"No materials of type '{typeFile}' found for this doctor." });
		//	}

		//	// Delete the files from the server
		//	foreach (var material in materials)
		//	{
		//		var filePath = Path.Combine(_hostingEnvironment.WebRootPath, material.FilePath.TrimStart('/'));
		//		if (System.IO.File.Exists(filePath))
		//		{
		//			System.IO.File.Delete(filePath);
		//		}
		//	}

		//	// Remove materials from the database
		//	_context.Materials.RemoveRange(materials);
		//	await _context.SaveChangesAsync();

		//	return Ok(new { success = true, message = $"All {typeFile} materials deleted successfully." });
		//}

		#endregion
 
//--------------------------------------------------------------------------------------------------------------------------------------------------	
		
		

		#region GetDoctorMaterialsForCourse

		[HttpGet("getDoctorMaterials/{courseCode}/{userId}")]
		[AllowAnonymous]
		public async Task<IActionResult> GetDoctorMaterialsForCourse(string courseCode, string userId)
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
				.Where(m => m.CourseCode == courseCode && m.DoctorId == doctor.DoctorId)
				.ToListAsync();

			if (!materials.Any())
			{
				return Ok(new { success = false, message = "No materials found for this doctor in the specified course.",materials });
			}
			
			return Ok(new
			{
				success = true,
				message = "Materials retrieved successfully.",
				materials = materials
			});
		}

		#endregion
		
		#region GetDoctorMaterialsForCourseBasedOnType

		[HttpGet("getDoctorMaterials/{courseCode}/{userId}/{typeFile}")]
		[Authorize(Roles ="Student")]
		public async Task<IActionResult> GetDoctorMaterialsForCourse(string courseCode, string userId , string typeFile)
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
				.Where(m => m.CourseCode == courseCode && m.DoctorId == doctor.DoctorId && m.TypeFile==typeFile)
				.ToListAsync();

			List<StudentMaterialResponse> studentMaterials= new List<StudentMaterialResponse>();

			foreach (var mat in materials)
			{
				StudentMaterialResponse studentMaterialResponse = new StudentMaterialResponse()
				{
					Id = mat.Id,
					FilePath = mat.FilePath,
					FileName = mat.FileName,
					TypeFile = mat.TypeFile,
					UploadDate = mat.UploadDate,
					Size = mat.Size,
					DoctorId = mat.DoctorId,
					CourseCode = mat.CourseCode,
					CourseId = mat.CourseId,
					FileExtension = Path.GetExtension(mat.FileName).ToLower()

				};


                studentMaterials.Add(studentMaterialResponse);



            }


			if (!materials.Any())
			{
				return Ok(new { success = true , message = "No materials found for this doctor in the specified course." , materials = studentMaterials.ToArray() });
			}


			return Ok(new
			{
				success = true,
				message = "Materials retrieved successfully.",
				materials = studentMaterials.ToArray()
			});
		}

		#endregion

	}
}