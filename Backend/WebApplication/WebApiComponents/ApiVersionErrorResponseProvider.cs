using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace HeyImIn.WebApplication.WebApiComponents
{
	public class ApiVersionErrorResponseProvider : DefaultErrorResponseProvider
	{
		public override IActionResult CreateResponse(ErrorResponseContext context)
		{
			return new ObjectResult(CreateErrorContent(context))
			{
				StatusCode = (int)HttpStatusCode.Gone
			};
		}
	}
}
