using GroceryOrderingApp.Backend.Models;
using GroceryOrderingApp.Backend.Repositories;
using GroceryOrderingApp.Backend.DTOs;
using Microsoft.AspNetCore.Identity;

namespace GroceryOrderingApp.Backend.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
        string GenerateToken(User user);
    }
}
