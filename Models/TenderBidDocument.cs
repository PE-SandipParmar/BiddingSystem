using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiddingSystem.Models
{
    public class TenderBidDocument
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tender Bid ID")]
        public int TenderBidId { get; set; }

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
        public BidDocumentType DocumentType { get; set; } = BidDocumentType.General;

        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; } = false;

        [Display(Name = "Uploaded At")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual TenderBid TenderBid { get; set; } = null!;

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

    public enum BidDocumentType
    {
        [Display(Name = "General")]
        General = 1,

        [Display(Name = "Technical Proposal")]
        TechnicalProposal = 2,

        [Display(Name = "Financial Proposal")]
        FinancialProposal = 3,

        [Display(Name = "Company Profile")]
        CompanyProfile = 4,

        [Display(Name = "Experience Certificate")]
        ExperienceCertificate = 5,

        [Display(Name = "Financial Statement")]
        FinancialStatement = 6,

        [Display(Name = "Other")]
        Other = 7
    }

    public static class BidDocumentTypeExtensions
    {
        public static string GetDisplayName(this BidDocumentType documentType)
        {
            return documentType switch
            {
                BidDocumentType.General => "General",
                BidDocumentType.TechnicalProposal => "Technical Proposal",
                BidDocumentType.FinancialProposal => "Financial Proposal",
                BidDocumentType.CompanyProfile => "Company Profile",
                BidDocumentType.ExperienceCertificate => "Experience Certificate",
                BidDocumentType.FinancialStatement => "Financial Statement",
                BidDocumentType.Other => "Other",
                _ => "Unknown"
            };
        }
    }
}
