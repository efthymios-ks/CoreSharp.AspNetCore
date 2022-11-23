using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Server.Controllers.Common;

[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public abstract class AppControllerBase : ControllerBase
{
}