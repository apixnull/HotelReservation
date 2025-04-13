namespace HotelReservation.ViewModels
{
    public class UserProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public string ProfilePictureUrl { get; set; } = string.Empty;    
    }
}
