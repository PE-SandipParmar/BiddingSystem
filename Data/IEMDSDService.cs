using BiddingSystem.Models;

namespace BiddingSystem.Data
{
    public interface IEMDSDService
    {
        Task<IEnumerable<EMDSDDeposit>> GetAllDepositsAsync();
        Task<EMDSDDeposit?> GetDepositByIdAsync(int id);
        Task<EMDSDDeposit?> GetDepositByDepositIdAsync(string depositId);
        Task<IEnumerable<EMDSDDeposit>> SearchDepositsAsync(string? depositId, string? tenderName, string? status, string? type);
        Task<EMDSDDeposit> CreateDepositAsync(EMDSDDeposit deposit);
        Task<EMDSDDeposit> UpdateDepositAsync(EMDSDDeposit deposit);
        Task<bool> DeleteDepositAsync(int id);
        Task<IEnumerable<EMDSDDeposit>> GetDepositsByTenderIdAsync(int tenderId);
        Task<IEnumerable<EMDSDDeposit>> GetDepositsByStatusAsync(string status);
        Task<IEnumerable<EMDSDDeposit>> GetDepositsByTypeAsync(string type);
        Task<(IEnumerable<EMDSDDeposit> deposits, int totalCount)> GetDepositsPagedAsync(int page, int pageSize, string? searchTerm = null, string? status = null, string? type = null);
        
        // Transaction methods
        Task<IEnumerable<EMDSDTransaction>> GetTransactionsByDepositIdAsync(int depositId);
        Task<EMDSDTransaction> CreateTransactionAsync(EMDSDTransaction transaction);
        Task<EMDSDTransaction> UpdateTransactionAsync(EMDSDTransaction transaction);
        Task<bool> DeleteTransactionAsync(int id);
        
        // Business logic methods
        Task<string> GenerateDepositIdAsync();
        Task<bool> ValidateDepositAsync(EMDSDDeposit deposit);
        Task<EMDSDDeposit> ProcessPaymentAsync(int depositId, string transactionId, string bankReference);
        Task<EMDSDDeposit> ProcessRefundAsync(int depositId, string reason);
    }
}
