using System.ComponentModel.DataAnnotations;

namespace HeyImIn.WebApplication.FrontendModels
{
	public class SetUserDataDto
	{
		public string NewFullName { get; set; }

		[EmailAddress]
		public string NewEmail { get; set; }
	}
}