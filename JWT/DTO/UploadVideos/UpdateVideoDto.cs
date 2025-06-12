using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.UploadVideos
{
	public class UpdateVideoDto
	{
		[Required(ErrorMessage = "Video Id is required.")]
		public int VideoId { get; set; } 

		[Required(ErrorMessage = "Video is required.")]
		
		public IFormFile Video { get; set; } // The new video file


		

		
		[RegularExpression("^(Videos)$", ErrorMessage = "Type must be 'Videos'")]
		[StringLength(6, ErrorMessage = "Type cannot exceed 6 characters.")]
		public string Type { get; set; } // The updated type of video (Lec or Lab)
	}
}
