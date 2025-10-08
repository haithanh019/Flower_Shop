using AutoMapper;
using DataAccess.Entities;
using Ultitity.Extensions;

namespace BusinessLogic.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() { }

        // ------------------------
        // Helper method for enums
        // ------------------------
        private void MapEnumsToString()
        {
            CreateMap<OrderStatus, string>().ConvertUsing(src => src.GetDisplayName());
            CreateMap<PaymentMethod, string>().ConvertUsing(src => src.GetDisplayName());
            CreateMap<PaymentStatus, string>().ConvertUsing(src => src.GetDisplayName());
            CreateMap<UserRole, string>().ConvertUsing(src => src.GetDisplayName());
        }

        // ------------------------
        // Helper method for ImageUrls
        // ------------------------
        private static List<string> ImagesToUrls(IEnumerable<ProductImage>? images)
        {
            return images?.Select(i => i.Url).ToList() ?? new List<string>();
        }
    }
}
