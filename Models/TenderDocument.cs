using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class TenderDocument
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tender ID")]
        public int TenderId { get; set; }

        [Required(ErrorMessage = "Document name is required")]
        [StringLength(255, ErrorMessage = "Document name cannot exceed 255 characters")]
        [Display(Name = "Document Name")]
        public string DocumentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "File name is required")]
        [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters")]
        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;

        [Required(ErrorMessage = "File path is required")]
        [StringLength(500, ErrorMessage = "File path cannot exceed 500 characters")]
        [Display(Name = "File Path")]
        public string FilePath { get; set; } = string.Empty;

        [Required(ErrorMessage = "File size is required")]
        [Display(Name = "File Size")]
        public long FileSize { get; set; }

        [Required(ErrorMessage = "Content type is required")]
        [StringLength(100, ErrorMessage = "Content type cannot exceed 100 characters")]
        [Display(Name = "Content Type")]
        public string ContentType { get; set; } = string.Empty;

        [Display(Name = "Document Type")]
        public TenderDocumentType DocumentType { get; set; } = TenderDocumentType.General;

        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; } = false;

        [Display(Name = "Uploaded By")]
        public int UploadedBy { get; set; }

        [Display(Name = "Uploaded At")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Tender Tender { get; set; } = null!;
        public virtual User UploadedByUser { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public string FileSizeDisplay => FormatFileSize(FileSize);

        [NotMapped]
        public string DocumentTypeDisplayName => DocumentType.GetDisplayName();

        [NotMapped]
        public string FileExtension => Path.GetExtension(FileName).ToLower();

        [NotMapped]
        public bool IsPdf => FileExtension == ".pdf";

        [NotMapped]
        public bool IsImage => new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }.Contains(FileExtension);

        [NotMapped]
        public bool IsDocument => new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" }.Contains(FileExtension);

        private static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 Bytes";
            string[] sizes = { "Bytes", "KB", "MB", "GB" };
            int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(1024));
            return Math.Round(bytes / Math.Pow(1024, i), 2) + " " + sizes[i];
        }
    }

    public enum TenderDocumentType
    {
        [Display(Name = "General")]
        General = 1,

        [Display(Name = "Technical Specification")]
        TechnicalSpecification = 2,

        [Display(Name = "Terms and Conditions")]
        TermsAndConditions = 3,

        [Display(Name = "Bid Form")]
        BidForm = 4,

        [Display(Name = "Price Schedule")]
        PriceSchedule = 5,

        [Display(Name = "Drawings")]
        Drawings = 6,

        [Display(Name = "Other")]
        Other = 7
    }

    public static class TenderDocumentTypeExtensions
    {
        public static string GetDisplayName(this TenderDocumentType documentType)
        {
            return documentType switch
            {
                TenderDocumentType.General => "General",
                TenderDocumentType.TechnicalSpecification => "Technical Specification",
                TenderDocumentType.TermsAndConditions => "Terms and Conditions",
                TenderDocumentType.BidForm => "Bid Form",
                TenderDocumentType.PriceSchedule => "Price Schedule",
                TenderDocumentType.Drawings => "Drawings",
                TenderDocumentType.Other => "Other",
                _ => "Unknown"
            };
        }
    }
}
