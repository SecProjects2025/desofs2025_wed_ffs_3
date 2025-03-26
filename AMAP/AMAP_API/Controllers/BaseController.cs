using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AMAP.Api.Controllers
{
    public class BaseController : Controller
    {
        protected string GetLoggedId() => User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
    }
}

