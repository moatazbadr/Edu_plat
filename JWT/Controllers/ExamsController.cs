using Azure;
using Edu_plat.DTO.ExamDto;
using Edu_plat.DTO.Notification;
using Edu_plat.Model.Exams;
using Edu_plat.Services;
using JWT;
using JWT.DATA;
using JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Edu_plat.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ExamsController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationHandler _notificationHandler;
        public ExamsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationHandler notificationHandler )
		{
			_context = context;
			_userManager = userManager;
            _notificationHandler = notificationHandler;
        }

        #region CreateExamOnline&Offline

        [HttpPost("CreateExamOnline&Offline")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDto examDto)
            {
            if (!ModelState.IsValid)
            {
                return Ok(new
                {
                    succes = false,
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

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

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
            {
                return Ok(new { success = false, message = "Doctor profile not found." });
            }

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode.ToLower() == examDto.CourseCode.ToLower());
            if (course == null)
            {
                return Ok(new { success = false, message = "Course not found." });
            }

            var isDoctorAssigned = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
            if (!isDoctorAssigned)
            {
                return Ok(new { success = false, message = "Doctor is not assigned to this course." });
            }

            var utcNow = DateTime.UtcNow;
            if (examDto.StartTime <= utcNow)
            {
                return Ok(new { success = false, message = "Exam must start in the future" });
            }

            var exam = new Exam
            {
                ExamTitle = examDto.ExamTitle,
                StartTime = examDto.StartTime,
                TotalMarks = examDto.TotalMarks,
                IsOnline = examDto.IsOnline,
                DurationInMin = examDto.DurationInMin,
                QusetionsNumber = examDto.IsOnline ? examDto.QuestionsNumber : null,
                CourseId = course.Id,
                DoctorId = doctor.DoctorId,
                CourseCode = examDto.CourseCode,
                Location = examDto.IsOnline ? "Online" : examDto.LocationExam,
                Questions = examDto.IsOnline ? new List<Question>() : null
            };

            if (examDto.IsOnline)
            {
                if (examDto.Questions == null || examDto.Questions.Count == 0 || examDto.QuestionsNumber == null)
                {
                    return Ok(new { success = false, message = "Online exams must have at least one question." });
                }
                if (examDto.Questions.Count != examDto.QuestionsNumber)
                {
                    return Ok($"Number of questions must be exactly {examDto.QuestionsNumber}.");
                }
                if (examDto.Questions.Sum(q => q.TimeInMin) != examDto.DurationInMin)
                {
                    return Ok(new { succces = false, message = $"Total exam time should be exactly {examDto.DurationInMin} minutes." });
                }
                if (examDto.Questions.Sum(q => q.Marks) != examDto.TotalMarks)
                {
                    return Ok(new { succes = false, message = $"Total marks should be exactly {examDto.TotalMarks}." });
                }

                foreach (var qDto in examDto.Questions)
                {
                    if (qDto.Choices == null || qDto.Choices.Count < 2 || qDto.Choices.Count > 5)
                    {
                        return Ok(new { succes = false, message = "Each question must have between 2 to 5 choices." });
                    }
                    if (qDto.Choices.Count(c => c.IsCorrect) != 1)
                    {
                        return Ok(new { succes = false, message = "Each question must have exactly one correct answer." });
                    }

                    var question = new Question
                    {
                        QuestionText = qDto.QuestionText,
                        Marks = qDto.Marks,
                        TimeInMin = qDto.TimeInMin,
                        Exam = exam,
                        Choices = qDto.Choices.Select(cDto => new Choice
                        {
                            Text = cDto.ChoiceText,
                            IsCorrect = cDto.IsCorrect
                        }).ToList()
                    };

                    exam.Questions.Add(question);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(examDto.LocationExam))
                {
                    return Ok(new { succes = false, message = "Offline exams must have a location." });
                }
                if (examDto.Questions != null && examDto.Questions.Count > 0)
                {
                    return Ok(new { succes = false, message = "Offline exams should not have questions." });
                }
            }

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            string ExamType = exam.IsOnline ? "online Exam" : "offline Exam";

             await _notificationHandler.SendMessageAsync(new MessageRequest
            {
                Title = exam.CourseCode,
                Body = $"New  {ExamType} {exam.ExamTitle} created by Dr {user.UserName} and is set to start at {exam.StartTime}  ",
                CourseCode = exam.CourseCode,
                UserId = userId,
                Date= DateOnly.FromDateTime(exam.StartTime)
             });



            return Ok(new { success = true, message = "Exam created successfully And notification sent to student", examId = exam.Id });
        }

        #endregion

        #region DeleteExam
        [HttpDelete("DeleteExam/{examId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> DeleteExam(int examId)
		{
			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors
				.AsNoTracking()
				.FirstOrDefaultAsync(d => d.UserId == userId);

			if (doctor == null)
			{
				return Ok(new { success=false,message = "Doctor profile not found." });
			}
			var exam = await _context.Exams
				.Include(e => e.Questions!)
				.ThenInclude(q => q.Choices)
				.FirstOrDefaultAsync(e => e.Id == examId);

			if (exam == null)
			{
				return Ok(new { success=false,message = "Exam not found." });
			}

			bool isDoctorAssigned = await _context.CourseDoctors
				.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == exam.CourseId);

			if (!isDoctorAssigned)
			{
				return Ok(new { success=false,message = "You are not authorized to delete this exam." });
			}
			if (exam.StartTime <= DateTime.UtcNow)
			{
				return Ok(new { success = false, message = "You cannot update an exam that has already started." });
			}


			_context.Exams.Remove(exam);
			await _context.SaveChangesAsync();

			return Ok(new { success =true,message = "Exam deleted successfully." });
		}
		#endregion

		#region GetExam to Update
		[HttpGet]
		[Route("get/examToUpdate/{ExamId}")]
		[Authorize(Roles ="Doctor")]
		public async Task<IActionResult>getExam(int ExamId)
		{
			if (ExamId < 0)
			{
				return Ok(new { success = false, message = "invalid exam id " });
			}
			var userId = User.FindFirstValue("ApplicationUserId");
			if (string.IsNullOrEmpty(userId))
			{
				return Ok(new { success = false, message = "invalid user" });
			}
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
            {
                return Ok(new { success = false, message = "Doctor profile not found." });
            }
			var Exam = await _context.Exams.Include(e => e.Questions!)
                .ThenInclude(q => q.Choices).FirstOrDefaultAsync(x => x.Id == ExamId);
			if (Exam == null)
			{
				return Ok(new { success = false, message = "no exam has been found" });
			}
			var course = await _context.Courses.FirstOrDefaultAsync(x => x.Id == Exam.CourseId);
			if (course == null)
			{
				return Ok(new { success = false, message = "No course found" });
			}

			bool isAssignedToDoctor = await _context.CourseDoctors.AnyAsync(
				d => d.DoctorId == doctor.DoctorId && d.CourseId == course.Id
				);
			if (!isAssignedToDoctor) {
				return Ok(new { success = false, message = "you cannot view this exam " });
			}
			var ExamView= new object();


            if (Exam.IsOnline == false)
			{
				ExamView = new
				{
					examTitle = Exam.ExamTitle,
					startTime = Exam.StartTime,
                    totalMarks=Exam.TotalMarks,
                    isOnline=Exam.IsOnline,
                    durationInMin=Exam.DurationInMin,
                    courseCode=Exam.CourseCode,
                    locationExam=Exam.Location
                };
                return Ok(new { success = true, message = "fetched successfully", ExamView });
            }


			 ExamView = new
			{
                examTitle=Exam.ExamTitle,
                startTime=Exam.StartTime,
                totalMarks=Exam.TotalMarks,
                isOnline=Exam.IsOnline,
                questionsNumber=Exam.QusetionsNumber,
                durationInMin=Exam.DurationInMin,
                courseCode=Exam.CourseCode,
                questions= Exam.Questions?.Select(q => new
                {
                    
                    q.QuestionText,
                    q.Marks,
                    q.TimeInMin,
                    Choices = q.Choices.Select(c => new
                    {

                        choiceText= c.Text,
						c.IsCorrect
                    })
                })



            };

            return Ok(new { success = true , message = "fetched successfully", ExamView });





        }


        #endregion

		#region ModelAnswer
		[HttpGet("GetModelAnswer/{examId}")]
		[Authorize(Roles = "Doctor,Student")]
		public async Task<IActionResult> GetModelAnswer(int examId)
		{
			var exam = await _context.Exams
				.Include(e => e.Questions!)
				.ThenInclude(q => q.Choices)
				.FirstOrDefaultAsync(e => e.Id == examId);

			if (exam == null)
			{
				return Ok(new { success = false, message = "Exam not found." });
			}

			if (!exam.IsOnline)
			{
				return Ok(new { success = false, message = "Model answers are only available for online exams." });
			}

			var modelAnswers = exam.Questions
				.Where(q => q.Choices.Any(c => c.IsCorrect))
				.Select(q => new
				{
					QuestionText = q.QuestionText,
					CorrectAnswer = q.Choices.FirstOrDefault(c => c.IsCorrect)?.Text
				})
				.ToList();

			return Ok(new { examId = exam.Id, examTitle = exam.ExamTitle, modelAnswers });
		}
		#endregion

		#region UpdateExam
		[HttpPut("UpdateExam/{examId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> UpdateExam(int examId, [FromBody] CreateExamDto examDto)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
			}

			var userId = User.FindFirstValue("ApplicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            var doctor = await _context.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.UserId == userId);
			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor profile not found." });
			}

			var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseCode == examDto.CourseCode);
			if (course == null)
			{
				return Ok(new { success = false, message = "Course not found." });
			}

		
			var isDoctorAssigned = await _context.CourseDoctors.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == course.Id);
			if (!isDoctorAssigned)
			{
				return Ok(new { success = false, message = "Doctor is not assigned to this course." });
			}

			var exam = await _context.Exams
				.Include(e => e.Questions!)
				.ThenInclude(q => q.Choices!)
				.FirstOrDefaultAsync(e => e.Id == examId);

			if (exam == null)
			{
				return Ok(new { success = false, message = "Exam not found." });
			}

			if (exam.StartTime <= DateTime.UtcNow)
			{
				return Ok(new { success = false, message = "You cannot update an exam that has already started." });
			}


			exam.ExamTitle = examDto.ExamTitle;
			exam.StartTime = examDto.StartTime;
			exam.TotalMarks = examDto.TotalMarks;
			exam.IsOnline = examDto.IsOnline;
			exam.DurationInMin = examDto.DurationInMin;
			exam.QusetionsNumber = examDto.IsOnline ? examDto.QuestionsNumber : null;

			if (examDto.IsOnline)
			{

				if (examDto.Questions == null || examDto.Questions.Count == 0 || examDto.QuestionsNumber == null)
				{
					return Ok(new { success = false, message = "Online exams must have at least one question." });
				}
				 
				if (examDto.Questions.Count != examDto.QuestionsNumber)
				{
					return Ok($"Number of questions must be exactly {examDto.QuestionsNumber}.");
				}
				int totalTimeFromQuestions = examDto.Questions.Sum(q => q.TimeInMin);
				if (totalTimeFromQuestions != examDto.DurationInMin)
				{
					return Ok(new { succces = false, message = $"Total exam time should be exactly {examDto.DurationInMin} minutes, but got {totalTimeFromQuestions} minutes." });
				}

				int totalMarksFromQuestions = examDto.Questions.Sum(q => q.Marks);
				if (totalMarksFromQuestions != examDto.TotalMarks)
				{
					return Ok(new { succes = false, message = $"Total marks should be exactly {examDto.TotalMarks}, but got {totalMarksFromQuestions}." });
				}


				bool hasExamBeenTaken = await _context.ExamStudents.AnyAsync(se => se.ExamId == exam.Id);
				if (hasExamBeenTaken)
				{
					return Ok(new { success = false, message = "You cannot modify questions for an exam that has already been taken by students." });
				}


				var existingQuestions = exam.Questions.ToList();
				foreach (var oldQuestion in existingQuestions)
				{
					var updatedQuestion = examDto.Questions.FirstOrDefault(q => q.QuestionText == oldQuestion.QuestionText);
					if (updatedQuestion == null)
					{
						_context.Question.Remove(oldQuestion);
					}
					else
					{
						oldQuestion.Marks = updatedQuestion.Marks;
						oldQuestion.TimeInMin = updatedQuestion.TimeInMin;


						var existingChoices = oldQuestion.Choices.ToList();
						foreach (var oldChoice in existingChoices)
						{
							var updatedChoice = updatedQuestion.Choices.FirstOrDefault(c => c.ChoiceText == oldChoice.Text);
							if (updatedChoice == null)
							{
								_context.Choices.Remove(oldChoice);
							}
							else
							{
								oldChoice.IsCorrect = updatedChoice.IsCorrect;
							}
						}


                        var newChoices = updatedQuestion.Choices
    .Where(c => !existingChoices.Any(ec => ec.Text == c.ChoiceText))
    .Select(c => new Choice
    {
        Text = c.ChoiceText,
        IsCorrect = c.IsCorrect,
        QuestionId = oldQuestion.Id
		
    })
    .ToList();

                        oldQuestion.Choices.AddRange(newChoices);
                    }
				}


				var newQuestions = examDto.Questions.Where(q => !existingQuestions.Any(eq => eq.QuestionText == q.QuestionText)).ToList();
				foreach (var newQuestion in newQuestions)
				{
					exam.Questions.Add(new Question
					{
						QuestionText = newQuestion.QuestionText,
						Marks = newQuestion.Marks,
						TimeInMin = newQuestion.TimeInMin,
						Exam = exam,
						Choices = newQuestion.Choices.Select(c => new Choice
						{
							Text = c.ChoiceText,
							IsCorrect = c.IsCorrect
						}).ToList()
					});
				}
			}
			else
			{
				_context.Question.RemoveRange(exam.Questions);
				exam.Questions.Clear();
			}

			await _context.SaveChangesAsync();
            string ExamType = exam.IsOnline ? "online Exam" : "offline Exam";
            await _notificationHandler.SendMessageAsync(new MessageRequest
            {
                Title = exam.CourseCode,
                Body = $"New {ExamType} {exam.ExamTitle} Updated by Dr {user.UserName} and is set to start at {exam.StartTime}  ",
                CourseCode = exam.CourseCode,
                UserId = userId,
                Date = DateOnly.FromDateTime(exam.StartTime)
            });
            return Ok(new {success=true, message = "Exam updated successfully." });
		}
        #endregion

        #region GetUserExams
        [HttpGet("GetUserExams")]
        [Authorize(Roles = "Doctor,Student")]
        public async Task<IActionResult> GetUserExams([FromQuery] GetUserExamsDto userexamdto)
        {
            if (!ModelState.IsValid)
            {
                return Ok(ModelState);
            }

            var userId = User.FindFirstValue("ApplicationUserId");

            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null)
                {
                    return Ok(new { success = false, message = "Doctor profile not found." });
                }

                var exams = await _context.Exams
                    .Where(e => _context.CourseDoctors.Any(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == e.CourseId))
                    .Select(e => new
                    {
                        e.Id,
                        e.ExamTitle,
                        StartTime = e.StartTime.ToUniversalTime().AddHours(3),
                        e.TotalMarks,
                        e.IsOnline,
                        e.DurationInMin,
                        QusetionsNumber = e.IsOnline ? e.QusetionsNumber : 0,
                        e.CourseCode,
                        e.Location,
                        e.DoctorId,
                        IsFinished = e.StartTime.AddMinutes(e.DurationInMin) < DateTime.Now.AddHours(-3)
                    })
                    .ToListAsync();

                var filteredExams = exams
                    .Where(e => e.IsFinished == userexamdto.isFinishedExam)
                    .Select(e => new
                    {
                        e.Id,
                        e.ExamTitle,
                        StartTime = e.StartTime.ToUniversalTime().AddHours(3),
                        e.TotalMarks,
                        e.IsOnline,
                        e.DurationInMin,
                        e.QusetionsNumber,
                        e.CourseCode,
                        e.Location,
                        e.DoctorId,
                        IsFinished = e.IsFinished
                    })
                    .ToList();

                return Ok(new { success = true, message = "fetched successfully", exams = filteredExams });
            }
            else if (User.IsInRole("Student"))
            {
                var student = await _context.Students
                    .Include(s => s.courses)
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (student == null)
                {
                    return Ok(new { success = false, message = "Student profile not found." });
                }

                var studentCourseCodes = student.courses.Select(c => c.CourseCode).ToList();

                var exams = await _context.Exams
                    .Where(e => studentCourseCodes.Contains(e.CourseCode))
                    .Select(e => new
                    {
                        e.Id,
                        e.ExamTitle,
                        StartTime=e.StartTime.ToUniversalTime().AddHours(3),
                        e.TotalMarks,
                        e.IsOnline,
                        e.DurationInMin,
                        QusetionsNumber = e.IsOnline ? e.QusetionsNumber : 0,
                        e.CourseCode,
                        e.Location,
                        e.DoctorId,
                        HasEnded = e.StartTime.AddMinutes(e.DurationInMin) < DateTime.Now.AddHours(-3),
                        StudentExam = e.IsOnline ? _context.ExamStudents
                            .Where(es => es.StudentId == student.StudentId && es.ExamId == e.Id)
                            .Select(es => new
                            {
                                Score = (double?)es.Score,
                                PercentageExam = (double?)es.precentageExam,
                                IsAbsent = (bool?)es.IsAbsent
                            })
                            .FirstOrDefault() : null
                    })
                    .ToListAsync();

                var result = exams
                .Where(e => e.HasEnded == userexamdto.isFinishedExam)
                .Select(e => new
                    {
                        e.Id,
                        e.ExamTitle,
                     StartTime=e.StartTime.ToUniversalTime().AddHours(3),
                     e.TotalMarks,
                     e.IsOnline,
                     e.DurationInMin,
                     e.QusetionsNumber,
                     e.CourseCode,
                     e.Location,
                     // e.DoctorId, // Uncomment this if needed
                     IsFinished = e.HasEnded,
                     Score = e.IsOnline ? (e.HasEnded ? (double ?) e.StudentExam?.Score ?? (double?)0 : null) : null,
                     PercentageExam = e.IsOnline ? (e.HasEnded ? (double ?) e.StudentExam?.PercentageExam ?? (double?)0 : null) : null,
                     IsAbsent = e.IsOnline ? (e.HasEnded ? e.StudentExam?.IsAbsent : false) : false
                 })
                 .ToList();
                            return Ok(new { success = true, message = "fetched successfully", exams = result });}

            return Ok(new { success = false, message = "User role not recognized." });
        }
        #endregion

        #region GetExamStudent
        [HttpGet("GetExamStudent")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> GetExamToStudent(int examId, int doctorId, string courseCode)
		{
			if (examId <= 0 || doctorId <= 0 || string.IsNullOrWhiteSpace(courseCode))
			{
				return Ok(new {success=false, message = "ExamId, DoctorId, and CourseCode are required and must be valid." });
			}
			var userId = User.FindFirstValue("ApplicationUserId");

			var student = await _context.Students
				.Include(s => s.courses)
				.FirstOrDefaultAsync(s => s.UserId == userId);

			if (student == null)
			{
                return Ok(new { success = false, message = "Student profile not found." });
			}

			var isStudentEnrolled = student.courses.Any(c => c.CourseCode == courseCode);
			if (!isStudentEnrolled)
			{
				return Ok(new { success = false, message = "Student is not enrolled in this course." });

			}

			var examDetails = await _context.Exams
				.Where(e => e.Id == examId && e.DoctorId == doctorId && e.CourseCode == courseCode && e.IsOnline)
				.Select(e => new
				{
					e.Id,
					e.ExamTitle,
					StartTime = e.StartTime,
					e.TotalMarks,
					e.IsOnline,
                    QusetionsNumber=e.IsOnline ? e.QusetionsNumber :0,
					e.DurationInMin,
					e.CourseCode,
					e.Location,
					e.DoctorId,
					Questions = e.Questions.OrderBy(q => Guid.NewGuid())
					.Select(q => new
					{
						q.Id,
						q.QuestionText,
						q.Marks,
						q.TimeInMin,
						Choices = q.Choices
						.OrderBy(c => Guid.NewGuid())
						.Select(c => new
						{
							c.Id,
							c.Text,
							c.IsCorrect
						}).ToList()
					}).ToList()
				})
				.FirstOrDefaultAsync();

			if (examDetails == null)
			{
				return Ok(new {success=false, message = "Online exam not found." });
			}

			return Ok(new { success = true, message = "Fetched successfully", examDetails });
		}

		#endregion

		#region SubmitExamScore
		[HttpPost("SubmitExamScore")]
		[Authorize(Roles = "Student")]
		public async Task<IActionResult> SubmitExamScore([FromBody] ExamSubmissionDto submission)
		{
			if (submission == null || submission.ExamId <= 0 || submission.Score < 0)
			{
				return Ok(new {success=false, message = "Invalid data. Please provide a valid ExamId and Score." });
			}
			var userId = User.FindFirstValue("ApplicationUserId");
			var student = await _context.Students
				.Include(s => s.courses)
				.FirstOrDefaultAsync(s => s.UserId == userId);

			if (student == null)
			{
				return Ok(new {success=false, message = "Student profile not found." });
			}

			var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == submission.ExamId);
			if (exam == null)
			{
				return Ok(new {success=false, message = "Exam not found." });
			}

			bool isStudentEnrolled = student.courses.Any(c => c.CourseCode == exam.CourseCode);
			if (!isStudentEnrolled)
			{
				return Ok(new { success=false, message = "Student is not enrolled in this course." });
			}
			if (!exam.IsOnline)
			{
				return Ok(new {success=true, message = "You can only submit scores for online exams." });
			}

			var examRecord = await _context.ExamStudents
				.FirstOrDefaultAsync(es => es.StudentId == student.StudentId && es.ExamId == submission.ExamId);

			if (examRecord != null)
			{
				return Ok(new {sucess=false, message = "Score already submitted for this exam." });
			}

			DateTime calculatedEndTime = exam.StartTime.AddMinutes(exam.DurationInMin);
			if (DateTime.UtcNow <= calculatedEndTime)
			{
				return Ok(new {success=true, message = "Exam has not finished yet. You cannot submit your score now." });
			}

			int percentage = (int)((submission.Score / (double)exam.TotalMarks) * 100);

			var newExamRecord = new ExamStudent
			{
				StudentId = student.StudentId,
				ExamId = submission.ExamId,
				Score = submission.Score,
				IsAbsent = false,
				precentageExam = percentage
			};

			_context.ExamStudents.Add(newExamRecord);
			await _context.SaveChangesAsync();

			return Ok(new {success=true, message = "Exam score submitted successfully." });
		}

		#endregion

		#region GetExamResults
		[HttpGet("GetExamResults/{examId}")]
		[Authorize(Roles = "Doctor")]
		public async Task<IActionResult> GetExamResults(int examId)
		{
			if (examId <= 0)
			{
				return Ok(new {success=false, message = "Invalid Exam ID." });
			}

			var userId = User.FindFirstValue("ApplicationUserId");
			var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

			if (doctor == null)
			{
				return Ok(new { success = false, message = "Doctor profile not found." });
			}

			var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == examId);
			if (exam == null)
			{
				return Ok(new { success = false, message = "Exam not found." });
			}

			bool isDoctorAssigned = await _context.CourseDoctors
				.AnyAsync(cd => cd.DoctorId == doctor.DoctorId && cd.CourseId == exam.CourseId);

			if (!isDoctorAssigned)
			{
				return Ok(new { success = false, message = "You are not authorized to view this exam's results." });
			}

            var examResults = await _context.ExamStudents
                .Where(es => es.ExamId == examId)
                .Select(es => new
                {

                    es.Score,
                    es.IsAbsent,
                    es.precentageExam,
                    StudentName = _context.Students
                        .Where(s => s.StudentId == es.StudentId)
                        .Select(s =>
                        
                            //s.applicationUser.Email,
                            s.applicationUser.UserName
                        )
                        .FirstOrDefault()

                }
                ).ToListAsync();
				

			if (examResults.Count == 0)
			{
				return Ok(new {success=false, message = "No students have taken this exam yet." , students=examResults });
			}

			int totalStudents = examResults.Count;
			int passedStudents = examResults.Count(es => es.Score >= (exam.TotalMarks * 0.5));
			double successRate = (double)passedStudents / totalStudents * 100;

			return Ok(new
			{
                success=true,
                message="Fetched succesfully",
				successRate = Math.Round(successRate, 2),
				students = examResults
			});
		}
        #endregion

        #region GetExamToStudent

        [HttpGet("GetExamForStudent/{examId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetExamForStudent(int examId)
        {
            if (examId <= 0)
            {
                return Ok(new { success = false, message = "Invalid exam ID." });
            }

            var  userId = User.FindFirstValue("ApplicationUserId");
            var student = await _context.Students
                .Include(s => s.courses)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return Ok(new { success = false, message = "Student profile not found."});
            }
            var exam = await _context.Exams
                .Include(e => e.Questions!)
                .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(e => e.Id == examId && e.IsOnline);
            if (exam == null)
            {
                return Ok(new { success = false, message = "Online exam not found." });
            }

            bool isStudentEnrolled = student.courses.Any(c => c.CourseCode == exam.CourseCode);
            if (!isStudentEnrolled)
            {
                return Ok(new { success = false, message = "Student is not enrolled in this course."  });
            }
            var examRecord = await _context.ExamStudents
                .FirstOrDefaultAsync(es => es.StudentId == student.StudentId && es.ExamId == examId);
            if (examRecord != null)
            {
                return Ok(new { success = false, message = "You have already accessed this exam."});
            }
            var currentUtcTime = DateTime.UtcNow;
            var examEndTime = exam.StartTime.AddMinutes(exam.DurationInMin);


            if (currentUtcTime < exam.StartTime)
            {
                return Ok(new { success = false, message = "The exam has not started yet."  });
            }
            if (currentUtcTime > examEndTime)
            {
                return  Ok(new { success = false, message = "The exam has ended."  });
            }
            var newExamRecord = new ExamStudent
            {
                StudentId = student.StudentId,
                ExamId = examId,
                Score = 0,
                IsAbsent = false,
                precentageExam = 0
            };
            _context.ExamStudents.Add(newExamRecord);
            await _context.SaveChangesAsync();
            var examDetails = new
            {
                exam.Id,
                exam.ExamTitle,
                StartTime = exam.StartTime.ToUniversalTime(),
                exam.TotalMarks,
                exam.IsOnline,
                exam.QusetionsNumber,
                exam.DurationInMin,
                exam.CourseCode,
                exam.Location,
                exam.DoctorId,
                Questions = exam.Questions.OrderBy(q => Guid.NewGuid()).Select(q => new
                {
                    q.Id,
                    q.QuestionText,
                    q.Marks,
                    q.TimeInMin,
                    Choices = q.Choices.OrderBy(c => Guid.NewGuid()).Select(c => new
                    {
                        c.Id,
                        c.Text
                    }).ToList()
                }).ToList()
            };

            return Ok(new { success = true, message="fetched successfully" ,exam = examDetails });
        }
        #endregion

        #region SumbitExamToStudent

        [HttpPost("SubmitExam")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitExam([FromBody] SubmitExamDto dto)
        {
            if (dto.ExamId <= 0 || dto.Answers == null || dto.Answers.Count == 0)
            {
                return  Ok(new { success = false, message = "Invalid data." });
            }

            var userId = User.FindFirstValue("ApplicationUserId");
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
                return Ok(new { success = false, message = "Student not found." });

            var exam = await _context.Exams
                .Include(e => e.Questions!)
                    .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(e => e.Id == dto.ExamId && e.IsOnline);

            if (exam == null)
                return Ok(new { success = false, message = "Exam not found." });

            var examEndTime = exam.StartTime.AddMinutes(exam.DurationInMin);
            if (DateTime.UtcNow > examEndTime)
                return Ok(new { success = false, message = "Time is up. Exam already ended." });

            var record = await _context.ExamStudents.FirstOrDefaultAsync(es => es.ExamId == dto.ExamId && es.StudentId == student.StudentId);
            if (record == null)
                return Ok(new { success = false, message = "Exam not accessed or not allowed." });

            // Prevent double submission
            if (record.Score > 0 || record.IsAbsent)
                return Ok(new { success = false, message = "Exam already submitted." });

            int totalScore = 0;

            foreach (var answer in dto.Answers)
            {
                var question = exam.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question == null) continue;

                var selectedChoice = question.Choices.FirstOrDefault(c => c.Id == answer.SelectedChoiceId);
                if (selectedChoice != null && selectedChoice.IsCorrect)
                {
                    totalScore += question.Marks;
                }
            }

            // Save the score
            record.Score = totalScore;
            record.precentageExam = (int)((double)totalScore / exam.TotalMarks * 100);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Exam submitted and corrected successfully.",
                score = totalScore,
                percentage = record.precentageExam
            });
        }
        #endregion


    }
}
