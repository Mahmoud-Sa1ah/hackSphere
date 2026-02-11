using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PentestHub.API.Data.Models;

public enum VerificationStatus
{
    Pending,
    Verified,
    Rejected
}

public class DomainVerification
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string DomainName { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string ProofPath { get; set; } = null!;

    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? VerifiedAt { get; set; }

    [MaxLength(1000)]
    public string? AdminComments { get; set; }
}
