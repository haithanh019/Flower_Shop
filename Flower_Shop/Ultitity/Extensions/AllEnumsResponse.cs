namespace Ultitity.Extensions
{
    public class AllEnumsResponse
    {
        public List<EnumDto> OrderStatus { get; set; } = new();
        public List<EnumDto> PaymentMethod { get; set; } = new();
        public List<EnumDto> PaymentStatus { get; set; } = new();
        public List<EnumDto> UserRole { get; set; } = new();
    }
}
