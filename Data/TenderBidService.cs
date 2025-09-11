using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public class TenderBidService : ITenderBidService
    {
        private readonly ITenderBidRepository _tenderBidRepository;
        private readonly ITenderRepository _tenderRepository;
        private readonly ILogger<TenderBidService> _logger;

        public TenderBidService(
            ITenderBidRepository tenderBidRepository,
            ITenderRepository tenderRepository,
            ILogger<TenderBidService> logger)
        {
            _tenderBidRepository = tenderBidRepository;
            _tenderRepository = tenderRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<TenderBid>> GetAllBidsAsync()
        {
            try
            {
                return await _tenderBidRepository.GetAllBidsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tender bids");
                throw;
            }
        }

        public async Task<TenderBid?> GetBidByIdAsync(int id)
        {
            try
            {
                return await _tenderBidRepository.GetBidByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender bid by ID: {Id}", id);
                throw;
            }
        }

        public async Task<TenderBid?> GetBidByPaymentReferenceAsync(string paymentReference)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentReference))
                    return null;

                return await _tenderBidRepository.GetBidByPaymentReferenceAsync(paymentReference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender bid by payment reference: {PaymentReference}", paymentReference);
                throw;
            }
        }

        public async Task<IEnumerable<TenderBid>> SearchBidsAsync(string? bidderName, string? companyName, string? status, string? paymentStatus)
        {
            try
            {
                return await _tenderBidRepository.SearchBidsAsync(bidderName, companyName, status, paymentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tender bids");
                throw;
            }
        }

        public async Task<TenderBid> CreateBidAsync(TenderBid bid)
        {
            try
            {
                // Validate bid
                if (!await ValidateBidAsync(bid))
                {
                    throw new ArgumentException("Invalid bid data");
                }

                // Calculate total amount
                bid.TotalAmount = await CalculateTotalAmountAsync(Convert.ToDecimal(bid.BidAmount), Convert.ToDecimal(bid.EmdAmount), Convert.ToDecimal(bid.ProcessingFee));

                // Generate payment reference if not provided
                if (string.IsNullOrEmpty(bid.PaymentReference))
                {
                    bid.PaymentReference = await GenerateUniquePaymentReferenceAsync();
                }

                // Set default values
                bid.SubmittedAt = DateTime.UtcNow;
                bid.CreatedAt = DateTime.UtcNow;
                bid.IsActive = true;

                return await _tenderBidRepository.CreateBidAsync(bid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tender bid");
                throw;
            }
        }

        public async Task<TenderBid> UpdateBidAsync(TenderBid bid)
        {
            try
            {
                // Validate bid
                if (!await ValidateBidAsync(bid))
                {
                    throw new ArgumentException("Invalid bid data");
                }

                // Calculate total amount
                bid.TotalAmount = await CalculateTotalAmountAsync(bid.BidAmount, bid.EmdAmount, bid.ProcessingFee);

                // Set update timestamp
                bid.UpdatedAt = DateTime.UtcNow;

                return await _tenderBidRepository.UpdateBidAsync(bid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tender bid: {Id}", bid.Id);
                throw;
            }
        }

        public async Task<bool> DeleteBidAsync(int id)
        {
            try
            {
                return await _tenderBidRepository.DeleteBidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tender bid: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<TenderBid>> GetBidsByTenderIdAsync(int tenderId)
        {
            try
            {
                return await _tenderBidRepository.GetBidsByTenderIdAsync(tenderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender bids by tender ID: {TenderId}", tenderId);
                throw;
            }
        }

        public async Task<IEnumerable<TenderBid>> GetBidsByStatusAsync(string status)
        {
            try
            {
                return await _tenderBidRepository.GetBidsByStatusAsync(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender bids by status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<TenderBid>> GetBidsByPaymentStatusAsync(string paymentStatus)
        {
            try
            {
                return await _tenderBidRepository.GetBidsByPaymentStatusAsync(paymentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tender bids by payment status: {PaymentStatus}", paymentStatus);
                throw;
            }
        }

        public async Task<(IEnumerable<TenderBid> bids, int totalCount)> GetBidsPagedAsync(int page, int pageSize, string? searchTerm = null, string? status = null, string? paymentStatus = null)
        {
            try
            {
                return await _tenderBidRepository.GetBidsPagedAsync(page, pageSize, searchTerm, status, paymentStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged tender bids");
                throw;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(int id, string paymentStatus, string? paymentReference = null, DateTime? paymentDate = null)
        {
            try
            {
                return await _tenderBidRepository.UpdatePaymentStatusAsync(id, paymentStatus, paymentReference, paymentDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for tender bid: {Id}", id);
                throw;
            }
        }

        public async Task<string> GenerateUniquePaymentReferenceAsync()
        {
            try
            {
                return await _tenderBidRepository.GenerateUniquePaymentReferenceAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating unique payment reference");
                throw;
            }
        }

        public async Task<bool> ValidateBidAsync(TenderBid bid)
        {
            try
            {
                // Check if tender exists and is active
                var tender = await _tenderRepository.GetByIdAsync(bid.TenderId);
                if (tender == null || !tender.IsActive)
                {
                    return false;
                }

                // Check if tender is still accepting bids
                if (tender.TenderClosingDate < DateTime.UtcNow)
                {
                    return false;
                }

                // Check if tender is published
                if (tender.Status != TenderStatus.Published)
                {
                    return false;
                }

                // Validate bid amounts
                if (bid.BidAmount <= 0 || bid.EmdAmount < 0 || bid.ProcessingFee < 0)
                {
                    return false;
                }

                // Validate bid amount is reasonable (not more than 10x estimated value)
                //if (tender.EstimatedValue > 0 && bid.BidAmount > tender.EstimatedValue * 10)
                //{
                //    return false;
                //}



                // Validate EMD amount is reasonable (typically 1-5% of bid amount)
                //if (bid.EmdAmount > bid.BidAmount * 0.1m) // More than 10% of bid amount
                //{
                //    return false;
                //}

                // Validate required fields
                if (string.IsNullOrWhiteSpace(bid.BidderName) ||
                    string.IsNullOrWhiteSpace(bid.BidderEmail) ||
                    string.IsNullOrWhiteSpace(bid.BidderPhone) ||
                    string.IsNullOrWhiteSpace(bid.CompanyName) ||
                    string.IsNullOrWhiteSpace(bid.CompanyAddress))
                {
                    return false;
                }

                // Validate email format
                if (!IsValidEmail(bid.BidderEmail))
                {
                    return false;
                }

                // Validate phone number format (basic validation)
                if (bid.BidderPhone.Length < 10 || bid.BidderPhone.Length > 15)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating tender bid");
                return false;
            }
        }

        public async Task<bool> CheckDuplicateBidAsync(int tenderId, string bidderEmail)
        {
            try
            {
                return await _tenderBidRepository.CheckDuplicateBidAsync(tenderId, bidderEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking duplicate bid for tender: {TenderId}, email: {Email}", tenderId, bidderEmail);
                throw;
            }
        }

        public async Task<decimal> CalculateTotalAmountAsync(decimal? bidAmount, decimal? emdAmount, decimal? processingFee)
        {
            try
            {
                return Convert.ToDecimal(bidAmount) + Convert.ToDecimal(emdAmount) + Convert.ToDecimal(processingFee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total amount");
                throw;
            }
        }

        public async Task<IEnumerable<TenderBidDocument>> GetBidDocumentsAsync(int bidId)
        {
            try
            {
                return await _tenderBidRepository.GetBidDocumentsAsync(bidId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bid documents for bid ID: {BidId}", bidId);
                throw;
            }
        }

        public async Task<TenderBidDocument> AddBidDocumentAsync(int bidId, IFormFile file, BidDocumentType documentType)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("No file provided");
                }

                // Validate file size (10MB limit)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                {
                    throw new ArgumentException("File size exceeds 10MB limit");
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException("File type not allowed. Allowed types: PDF, DOC, DOCX, XLS, XLSX, JPG, JPEG, PNG");
                }

                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine("wwwroot", "uploads", "tender-bids", bidId.ToString());
                Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create document record
                var document = new TenderBidDocument
                {
                    TenderBidId = bidId,
                    DocumentName = Path.GetFileNameWithoutExtension(file.FileName),
                    FileName = fileName,
                    FilePath = filePath.Replace("wwwroot", "").Replace("\\", "/"),
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    DocumentType = documentType,
                    IsRequired = false,
                    UploadedAt = DateTime.UtcNow,
                    IsActive = true
                };

                return await _tenderBidRepository.AddBidDocumentAsync(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding document to bid ID: {BidId}", bidId);
                throw;
            }
        }

        public async Task<bool> DeleteBidDocumentAsync(int documentId)
        {
            try
            {
                return await _tenderBidRepository.DeleteBidDocumentAsync(documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document ID: {DocumentId}", documentId);
                throw;
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
