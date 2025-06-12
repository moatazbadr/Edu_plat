using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.UploadFiles
{
	public class UpdateMaterialDto
	{
		[Required(ErrorMessage = "MaterialId is required.")]
		public int Material_Id { get; set; }  

		[Required(ErrorMessage = "File is required.")]
		public IFormFile File { get; set; }  

	    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
      

		[Required(ErrorMessage = "Type is required.")]
		[RegularExpression("^(Lectures|Labs|Exams|Videos)$", ErrorMessage = "Type must be either 'Lectures', 'Labs', 'Exams'.")]
		public string Type { get; set; } 
	}
}
