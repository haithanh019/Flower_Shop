using System;
using System.ComponentModel.DataAnnotations;

namespace FlowerShop_WebApp.Models.Profile
{
    public class AddressViewModel
    {
        public Guid AddressId { get; set; }

        [Required]
        public required string City { get; set; }

        [Required]
        public required string District { get; set; }

        [Required]
        public required string Ward { get; set; }

        [Required]
        public required string Detail { get; set; }
    }
}
