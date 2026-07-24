using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GroceryOrderingApp.Backend.Services;
using GroceryOrderingApp.Backend.DTOs;

namespace GroceryOrderingApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if ((string.IsNullOrWhiteSpace(request.UserId) && string.IsNullOrWhiteSpace(request.MobileNumber)) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("MobileNumber (or UserId) and Password are required");
            }

            var result = await _authService.LoginAsync(request);
            if (result == null)
                return Unauthorized("Invalid dealer/admin credentials");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.MobileNumber) ||
                string.IsNullOrWhiteSpace(request.Address))
            {
                return BadRequest("Password, FullName, MobileNumber, and Address are required");
            }

            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("update-fcm-token")]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequestDto request)
        {
            _logger.LogInformation("Received FCM token update request.");

            if (string.IsNullOrWhiteSpace(request.FcmToken))
            {
                _logger.LogWarning("Received empty FCM token update request.");
                return BadRequest("FCM token is required");
            }

            _logger.LogInformation("Processing FCM token update. TokenLength={TokenLength}", request.FcmToken.Length);

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("FCM token update rejected because the userId claim was missing or invalid.");
                return Unauthorized("User not authenticated");
            }

            _logger.LogInformation("Updating FCM token for UserId={UserId}", userId);

            var result = await _authService.UpdateFcmTokenAsync(userId, request.FcmToken);
            if (!result)
            {
                _logger.LogWarning("Failed to update FCM token for UserId={UserId}", userId);
                return BadRequest("Failed to update FCM token");
            }

            _logger.LogInformation("FCM token update successful for UserId={UserId}", userId);
            return Ok(new { Success = true, Message = "FCM token updated successfully" });
        }
    }
}
