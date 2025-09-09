using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public class EMDSDService : IEMDSDService
    {
        private readonly IEMDSDRepository _repository;

        public EMDSDService(IEMDSDRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<EMDSDDeposit>> GetAllDepositsAsync()
        {
            return await _repository.GetAllDepositsAsync();
        }

        public async Task<EMDSDDeposit?> GetDepositByIdAsync(int id)
        {
            return await _repository.GetDepositByIdAsync(id);
        }

        public async Task<EMDSDDeposit?> GetDepositByDepositIdAsync(string depositId)
        {
            return await _repository.GetDepositByDepositIdAsync(depositId);
        }

        public async Task<IEnumerable<EMDSDDeposit>> SearchDepositsAsync(string? depositId, string? tenderName, string? status, string? type)
        {
            return await _repository.SearchDepositsAsync(depositId, tenderName, status, type);
        }

        public async Task<EMDSDDeposit> CreateDepositAsync(EMDSDDeposit deposit)
        {
            if (!await ValidateDepositAsync(deposit))
            {
                throw new ArgumentException("Invalid deposit data");
            }

            if (string.IsNullOrEmpty(deposit.DepositId))
            {
                deposit.DepositId = await GenerateDepositIdAsync();
            }

            return await _repository.CreateDepositAsync(deposit);
        }

        public async Task<EMDSDDeposit> UpdateDepositAsync(EMDSDDeposit deposit)
        {
            if (!await ValidateDepositAsync(deposit))
            {
                throw new ArgumentException("Invalid deposit data");
            }

            return await _repository.UpdateDepositAsync(deposit);
        }

        public async Task<bool> DeleteDepositAsync(int id)
        {
            return await _repository.DeleteDepositAsync(id);
        }

        public async Task<IEnumerable<EMDSDDeposit>> GetDepositsByTenderIdAsync(int tenderId)
        {
            return await _repository.GetDepositsByTenderIdAsync(tenderId);
        }

        public async Task<IEnumerable<EMDSDDeposit>> GetDepositsByStatusAsync(string status)
        {
            return await _repository.GetDepositsByStatusAsync(status);
        }

        public async Task<IEnumerable<EMDSDDeposit>> GetDepositsByTypeAsync(string type)
        {
            return await _repository.GetDepositsByTypeAsync(type);
        }

        public async Task<(IEnumerable<EMDSDDeposit> deposits, int totalCount)> GetDepositsPagedAsync(int page, int pageSize, string? searchTerm = null, string? status = null, string? type = null)
        {
            return await _repository.GetDepositsPagedAsync(page, pageSize, searchTerm, status, type);
        }

        public async Task<IEnumerable<EMDSDTransaction>> GetTransactionsByDepositIdAsync(int depositId)
        {
            return await _repository.GetTransactionsByDepositIdAsync(depositId);
        }

        public async Task<EMDSDTransaction> CreateTransactionAsync(EMDSDTransaction transaction)
        {
            return await _repository.CreateTransactionAsync(transaction);
        }

        public async Task<EMDSDTransaction> UpdateTransactionAsync(EMDSDTransaction transaction)
        {
            return await _repository.UpdateTransactionAsync(transaction);
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            return await _repository.DeleteTransactionAsync(id);
        }

        public async Task<string> GenerateDepositIdAsync()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");
            
            // Get the count of deposits created today
            var todayDeposits = await _repository.GetAllDepositsAsync();
            var todayCount = todayDeposits.Count(d => d.CreatedAt.Date == DateTime.Today);
            
            var sequence = (todayCount + 1).ToString("D4");
            return $"EMD{year}{month}{day}{sequence}";
        }

        public async Task<bool> ValidateDepositAsync(EMDSDDeposit deposit)
        {
            if (deposit == null) return false;
            if (deposit.TenderId <= 0) return false;
            if (deposit.TenderBidId <= 0) return false;
            if (deposit.Amount <= 0) return false;
            if (string.IsNullOrEmpty(deposit.BidderName)) return false;
            if (string.IsNullOrEmpty(deposit.CompanyName)) return false;
            if (string.IsNullOrEmpty(deposit.BankName)) return false;
            if (string.IsNullOrEmpty(deposit.FSSAIBranchName)) return false;
            if (string.IsNullOrEmpty(deposit.Type) || (deposit.Type != "EMD" && deposit.Type != "SD")) return false;

            return true;
        }

        public async Task<EMDSDDeposit> ProcessPaymentAsync(int depositId, string transactionId, string bankReference)
        {
            var deposit = await _repository.GetDepositByIdAsync(depositId);
            if (deposit == null)
            {
                throw new ArgumentException("Deposit not found");
            }

            // Update deposit status
            deposit.Status = "Paid";
            deposit.TransactionId = transactionId;
            deposit.UpdatedAt = DateTime.UtcNow;

            // Create transaction record
            var transaction = new EMDSDTransaction
            {
                EMDSDDepositId = depositId,
                TransactionReference = transactionId,
                Amount = deposit.Amount,
                TransactionType = "Payment",
                Status = "Success",
                TransactionDate = DateTime.UtcNow,
                BankReference = bankReference,
                CreatedBy = "System"
            };

            await _repository.CreateTransactionAsync(transaction);
            return await _repository.UpdateDepositAsync(deposit);
        }

        public async Task<EMDSDDeposit> ProcessRefundAsync(int depositId, string reason)
        {
            var deposit = await _repository.GetDepositByIdAsync(depositId);
            if (deposit == null)
            {
                throw new ArgumentException("Deposit not found");
            }

            // Update deposit status
            deposit.Status = "Refunded";
            deposit.Remarks = reason;
            deposit.UpdatedAt = DateTime.UtcNow;

            // Create transaction record
            var transaction = new EMDSDTransaction
            {
                EMDSDDepositId = depositId,
                TransactionReference = $"REF-{DateTime.Now:yyyyMMddHHmmss}",
                Amount = deposit.Amount,
                TransactionType = "Refund",
                Status = "Success",
                TransactionDate = DateTime.UtcNow,
                Remarks = reason,
                CreatedBy = "System"
            };

            await _repository.CreateTransactionAsync(transaction);
            return await _repository.UpdateDepositAsync(deposit);
        }
    }
}
