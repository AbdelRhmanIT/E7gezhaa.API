using System;
using System.ComponentModel.DataAnnotations;

namespace E7gezhaa.API.Entities
{
    public class AttireBooking
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public int AttireId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";

        public virtual WeddingAttire? Attire { get; set; }
    }
}