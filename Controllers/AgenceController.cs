using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gest_Immo_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AgenceController : ControllerBase
    {
        [HttpGet("get-collaborators")]
        public IActionResult Collaborators()
        {
            return Ok(new JsonResult(new { message = "Seuls les administrateurs d'agences peuvent voir la liste des collaborateurs" }));
        }
    }
}
