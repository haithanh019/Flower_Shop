using AutoMapper;
using BusinessLogic.DTOs.Cart;
using BusinessLogic.DTOs.Categories;
using BusinessLogic.DTOs.Orders;
using BusinessLogic.DTOs.Payments;
using BusinessLogic.DTOs.Products;
using BusinessLogic.DTOs.Users;
using DataAccess.Entities;
using Ultitity.Extensions;

namespace BusinessLogic.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ------------------------
            // Enum -> string (dựa trên helper có sẵn)
            // ------------------------
            MapEnumsToString();

            // ========================
            // Users
            // ========================
            CreateMap<User, UserDto>();

            CreateMap<UserCreateRequest, User>()
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());

            CreateMap<UserUpdateRequest, User>()
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val is not null));

            // ========================
            // Categories
            // ========================
            CreateMap<Category, CategoryDto>();

            CreateMap<CategoryCreateRequest, Category>()
                .ForMember(d => d.CategoryId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore());

            CreateMap<CategoryUpdateRequest, Category>()
                .ForMember(d => d.CategoryId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val is not null));

            // ========================
            // Products (+ ProductImage)
            // ========================
            // Entity -> DTO (theo yêu cầu mới: ImageUrls & ImagePublicIds trên ProductDto)
            CreateMap<Product, ProductDto>()
                .ForMember(
                    d => d.CategoryName,
                    opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null)
                )
                .ForMember(d => d.ImageUrls, opt => opt.MapFrom(s => ImagesToUrls(s.Images)))
                .ForMember(
                    d => d.ImagePublicIds,
                    opt =>
                        opt.MapFrom(s =>
                            s.Images != null
                                ? s.Images.Select(i => i.PublicId ?? string.Empty).ToList()
                                : new List<string>()
                        )
                );

            // Create DTO -> Entity (ảnh xử lý ở Service từ ImageUrls/ImagePublicIds -> ProductImage)
            CreateMap<ProductCreateRequest, Product>()
                .ForMember(d => d.ProductId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.Images, opt => opt.Ignore());

            // Update DTO -> Entity (ảnh thay thế ở Service)
            CreateMap<ProductUpdateRequest, Product>()
                .ForMember(d => d.ProductId, opt => opt.Ignore())
                .ForMember(d => d.Images, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val is not null));

            // ========================
            // Cart
            // ========================
            CreateMap<CartItem, CartItemDto>()
                .ForMember(
                    d => d.ProductName,
                    opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : null)
                )
                .ForMember(
                    d => d.ProductThumbnailUrl,
                    opt =>
                        opt.MapFrom(s =>
                            s.Product != null && s.Product.Images != null
                                ? s.Product.Images.Select(i => i.Url).FirstOrDefault()
                                : null
                        )
                );

            CreateMap<Cart, CartDto>().ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));

            // ========================
            // Orders
            // ========================
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(
                    d => d.ProductName,
                    opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : null)
                );

            CreateMap<Order, OrderDto>()
                .ForMember(
                    d => d.CustomerEmail,
                    opt => opt.MapFrom(s => s.User != null ? s.User.Email : null)
                )
                .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items))
                // summary từ Payment (nếu có)
                .ForMember(
                    d => d.TransactionId,
                    opt => opt.MapFrom(s => s.Payment != null ? s.Payment.TransactionId : null)
                );

            CreateMap<OrderCreateRequest, Order>()
                .ForMember(d => d.OrderId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.Items, opt => opt.Ignore())
                .ForMember(d => d.Payment, opt => opt.Ignore());

            CreateMap<OrderUpdateStatusRequest, Order>()
                .ForMember(d => d.OrderId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val is not null));

            // ========================
            // Payments
            // ========================
            CreateMap<Payment, PaymentDto>();

            CreateMap<PaymentCreateRequest, Payment>()
                .ForMember(d => d.PaymentId, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.TransactionId, opt => opt.Ignore());

            CreateMap<PaymentUpdateStatusRequest, Payment>()
                .ForMember(d => d.PaymentId, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, val) => val is not null));
        }

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
