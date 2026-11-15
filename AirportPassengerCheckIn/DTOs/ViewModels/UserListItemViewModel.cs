namespace AirportPassengerCheckIn.DTOs.ViewModels
{
    public class UserListItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsLocked { get; set; }
        public string? EmployeeFullName { get; set; }
        public string? Position { get; set; }
    }
}
