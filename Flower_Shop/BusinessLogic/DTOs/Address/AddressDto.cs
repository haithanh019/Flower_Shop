namespace BusinessLogic.DTOs.Address
{
    public class AddressDto
    {
        public Guid AddressId { get; set; }
        public Guid UserId { get; set; }
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
