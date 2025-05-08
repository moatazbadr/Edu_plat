using Edu_plat.Model;
using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Edu_plat.DTO.AdminFiles;
using Edu_plat.Services;
using Edu_plat.DTO.Notification;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudentHelpController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationHandler _notificationHandler;

        public StudentHelpController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager,INotificationHandler notificationHandler)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _notificationHandler = notificationHandler;

        }

        #region Upload File
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadFile([FromForm] FileDto fileDto)
        {
            if (fileDto.File == null || fileDto.File.Length == 0)
                return BadRequest(new { success = false, message = "No file uploaded." });

            if (Path.GetExtension(fileDto.File.FileName).ToLower() != ".pdf")
                return BadRequest(new { success = false, message = "Only PDF files are allowed." });

            if (string.IsNullOrWhiteSpace(fileDto.FileName))
                return BadRequest(new { success = false, message = "File name cannot be empty." });
            List<string> types = new List<string> { "StudentGuide", "LabSchedule", "LectureSchedule", "ExamSchedule" };

            var correctType = types.FirstOrDefault(x => x == fileDto.type);

            if (string.IsNullOrEmpty(correctType))
                return BadRequest(new { success = false, message = "Invalid file type." });

            string fileNameWithExtension = Path.HasExtension(fileDto.FileName)
                ? fileDto.FileName
                : $"{fileDto.FileName}.pdf";
            var uploadsDir = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "AdminFiles");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var filePath = Path.Combine(uploadsDir, fileNameWithExtension);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileDto.File.CopyToAsync(stream);
            }

            var fileRecord = new AdminFile
            {
                FileName = fileNameWithExtension,
                FilePath = $"/Uploads/AdminFiles/{fileNameWithExtension}",
                uploadeDate = DateTime.UtcNow,
                size = $"{(fileDto.File.Length / (1024.0 * 1024.0)):F2} MB",
                type = fileDto.type,
                level = 0

            };

            _context.AdminFiles.Add(fileRecord);
            await _context.SaveChangesAsync();

            await _notificationHandler.AdminAnnouncement(
                
                new MessageRequest { 
                    Title= $"📢 Important Announcement from Computer science program",
                    Body= $"{fileDto.FileName} has been uploaded",
                    Date=DateOnly.FromDateTime(DateTime.UtcNow)
                }
            , "both");
            return Ok(new
            {
                success = true,
                message = "File uploaded successfully.",
                fileDetails = new
                {
                    fileRecord.Id,
                    fileRecord.FileName,
                    fileRecord.FilePath,
                    fileRecord.uploadeDate,
                    fileRecord.size,
                    fileRecord.type
                }
            });
        }

        #endregion

        #region Delete By Name
        [HttpDelete("DeleteFileByName/{fileName}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFileByName(string fileName)
        {
            var fileRecord = await _context.AdminFiles.FirstOrDefaultAsync(f => f.FileName == fileName);
            if (fileRecord == null)
                return NotFound(new { success = false, message = "File not found." });

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileRecord.FilePath.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.AdminFiles.Remove(fileRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "File deleted successfully." });
        }
        #endregion

        #region Delete By Id
        [HttpDelete("DeleteFileById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFileById(int id)
        {
            var fileRecord = await _context.AdminFiles.FindAsync(id);
            if (fileRecord == null)
                return NotFound(new { success = false, message = "File not found." });

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileRecord.FilePath.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.AdminFiles.Remove(fileRecord);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "File deleted successfully." });
        }
        #endregion

        #region Get All Files
        [HttpGet]
        [Authorize(Roles = "Student,Doctor,Admin")]
        public async Task<IActionResult> GetAllFiles()
        {
            var files = await _context.AdminFiles
                .Select(f => new
                {
                    //f.Id,
                    f.FileName,
                    f.FilePath,
                    // f.uploadeDate,
                    // f.size,
                    f.type
                })
                .ToListAsync();

            if (files.Count == 0)
                return NotFound(new { success = false, message = "No files found." });

            return Ok(new { files });
        }
        #endregion

        #region getFiles
        [HttpGet("{type:alpha}")]
        [Authorize(Roles = "Student , Doctor,Admin")]

        public async Task<IActionResult> GetFiles(string type)
        {
            var files = await _context.AdminFiles.Where(x => x.type == type).Select(x =>
            new
            {
                x.FileName,
                x.FilePath,
                x.type
            }
            ).ToListAsync();

            if (files.Count == 0)
                return Ok(new { success = true, message = $"No file found in {type}" });



            return Ok(new { success = true, message = "fetch complete", files });

        }
        #endregion

        #region Getting all Files For Admin 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminFiles()
        {
            var files = await _context.AdminFiles
                .Select(f => new
                {
                    f.Id,
                    f.FileName,
                    f.FilePath,
                    f.uploadeDate,
                    f.size,
                    f.type
                })
                .ToListAsync();

            if (files.Count == 0)
                return NotFound(new { success = false, message = "No files found." });

            return Ok(new { files });
        }
        #endregion




    }
}
