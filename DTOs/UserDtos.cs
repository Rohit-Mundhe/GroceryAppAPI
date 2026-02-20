namespace GroceryOrderingApp.Backend.DTOs
{
    public class CreateUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin" or "Customer"
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
    }
}
