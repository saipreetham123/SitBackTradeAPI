using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SitBackTradeAPI.Model
{
    // Models/User.cs
    public class User : IdentityUser
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        //[Required]
        //public string? UserN { get; set; }

        public Role? Role { get; set; }

        public int? NumberOfOrders { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Wallet { get; set; } = 0;

        public string? StoreName { get; set; }
    }
    public enum Role
    {
        Seller,
        Buyer,
        Admin
    }

}
