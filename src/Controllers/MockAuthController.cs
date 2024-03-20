using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers;

[Route("mock")]
[ApiController]
public sealed class MockAuthController : ControllerBase {
    private readonly IConfiguration _config;

    public MockAuthController(IConfiguration config) => _config = config;

    [HttpGet("login/{id}")]
    public async Task<IActionResult> Login(string id) {
        var claim = new Claim("id", id);

        var identity = new ClaimsIdentity([claim], _config["Cookies"]);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(principal);

        return Ok("Logged In");
    }
}