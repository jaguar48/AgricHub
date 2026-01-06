using AgricHub.BLL.Interfaces.ChatServices;
using AgricHub.BLL.Interfaces.IPaystackService;
using AgricHub.BLL.Interfaces.IWalletService;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgricHub.BLL.Implementations.WalletService
{
    public class WalletService : IWalletService
    {
        private readonly IRepository<Wallet> _walletRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IRepository<WalletTransaction> _walletTransactionRepo;
        private readonly IPaystackService _paystackService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISendbirdService _sendbirdService;

        public WalletService(
            IUnitOfWork unitOfWork,
            IPaystackService paystackService,
            IHttpContextAccessor httpContextAccessor,
            ISendbirdService sendbirdService)
        {
            _unitOfWork = unitOfWork;
            _walletRepo = unitOfWork.GetRepository<Wallet>();
            _customerRepo = unitOfWork.GetRepository<Customer>();
            _consultantRepo = unitOfWork.GetRepository<Consultant>();
            _walletTransactionRepo = unitOfWork.GetRepository<WalletTransaction>();
            _paystackService = paystackService;
            _httpContextAccessor = httpContextAccessor;
            _sendbirdService = sendbirdService;
        }

        private string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated.");
            return userId;
        }

        public async Task<WalletResponse> GetMyWalletAsync()
        {
            var userId = GetUserId();

            // Try customer first
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            if (customer != null)
            {
                var wallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == customer.Id)
                    ?? throw new KeyNotFoundException("Wallet not found.");

                return new WalletResponse
                {
                    UserId = customer.UserId,
                    UserName = $"{customer.FirstName} {customer.LastName}",
                    UserType = "Customer",
                    Balance = wallet.Balance,
                    IsActive = wallet.IsActive,
                    LastUpdated = wallet.LastUpdated
                };
            }

            // Try consultant
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
            if (consultant != null)
            {
                var wallet = await _walletRepo.GetSingleByAsync(w => w.ConsultantId == consultant.Id)
                    ?? throw new KeyNotFoundException("Wallet not found.");

                return new WalletResponse
                {
                    UserId = consultant.UserId,
                    UserName = $"{consultant.FirstName} {consultant.LastName}",
                    UserType = "Consultant",
                    Balance = wallet.Balance,
                    IsActive = wallet.IsActive,
                    LastUpdated = wallet.LastUpdated
                };
            }

            throw new UnauthorizedAccessException("User not found.");
        }

        public async Task<IEnumerable<WalletTransactionResponse>> GetMyTransactionsAsync()
        {
            var userId = GetUserId();

            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId);
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);

            IEnumerable<WalletTransaction> transactions;

            if (customer != null)
            {
                transactions = await _walletTransactionRepo.GetAllAsync(
                    wt => wt.CustomerId == customer.Id);
            }
            else if (consultant != null)
            {
                transactions = await _walletTransactionRepo.GetAllAsync(
                    wt => wt.ConsultantId == consultant.Id);
            }
            else
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            return transactions.Select(t => new WalletTransactionResponse
            {
                Id = t.Id,
                Amount = t.Amount,
                TransactionType = t.TransactionType,
                Status = t.Status,
                PaystackTransactionReference = t.PaystackTransactionReference,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt
            }).OrderByDescending(t => t.CreatedAt);
        }

        public async Task<WalletTopUpResponse> TopUpWalletAsync(decimal amount)
        {
            var userId = GetUserId();

            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId)
                ?? throw new UnauthorizedAccessException("Customer not found.");

            var wallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == customer.Id)
                ?? throw new InvalidOperationException("Wallet not found.");

            var callbackUrl = "http://localhost:3000/payment-success";

            var (accessCode, reference) = await _paystackService.InitializeTransactionAsync(
                customer.Email,
                amount,
                callbackUrl
            );

            var walletTransaction = new WalletTransaction
            {
                CustomerId = customer.Id,
                ConsultantId = null,
                Amount = amount,
                PaystackTransactionReference = reference,
                TransactionType = "WalletTopUp",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = null
            };

            await _walletTransactionRepo.AddAsync(walletTransaction);
            await _unitOfWork.SaveChangesAsync();

            return new WalletTopUpResponse
            {
                AccessCode = accessCode,
                Reference = reference,
                PaymentUrl = $"https://checkout.paystack.com/{accessCode}",
                Message = "Complete wallet top-up using Paystack.",
                Amount = amount,
                Balance = wallet.Balance
            };
        }

        public async Task<WalletTopUpResponse> VerifyPaymentAsync(string reference)
        {
            try
            {
                var userId = GetUserId();

                var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId)
                    ?? throw new UnauthorizedAccessException("Customer not found.");

                var walletTransaction = await _walletTransactionRepo.GetSingleByAsync(
                    wt => wt.PaystackTransactionReference == reference &&
                          wt.CustomerId == customer.Id);

                if (walletTransaction == null)
                    throw new KeyNotFoundException($"Transaction with reference {reference} not found.");

                // Check if already completed
                if (walletTransaction.Status == "Completed")
                {
                    var wallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == customer.Id);
                    return new WalletTopUpResponse
                    {
                        Reference = reference,
                        Message = "Payment already verified and wallet updated.",
                        Amount = walletTransaction.Amount,
                        Balance = wallet?.Balance ?? 0
                    };
                }

                // Verify with Paystack
                var verificationResult = await _paystackService.VerifyTransactionAsync(reference);

                if (verificationResult.Data.Status != "success")
                    throw new InvalidOperationException($"Payment verification failed. Status: {verificationResult.Data.Status}");

                await _unitOfWork.BeginTransactionAsync();

                var customerWallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == customer.Id)
                    ?? throw new InvalidOperationException("Wallet not found.");

                var amount = verificationResult.Data.Amount / 100m;

                customerWallet.Balance += amount;
                customerWallet.LastUpdated = DateTime.UtcNow;
                _walletRepo.Update(customerWallet);

                walletTransaction.Status = "Completed";
                walletTransaction.CompletedAt = DateTime.UtcNow;
                _walletTransactionRepo.Update(walletTransaction);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                if (!string.IsNullOrEmpty(customer.SendbirdChannelUrl))
                {
                    try
                    {
                        await _sendbirdService.SendAdminMessageAsync(
                            customer.SendbirdChannelUrl,
                            $"💳 Wallet topped up with ₦{amount:N2}. New balance: ₦{customerWallet.Balance:N2}."
                        );
                    }
                    catch { }
                }

                return new WalletTopUpResponse
                {
                    Reference = reference,
                    Message = "Payment verified successfully!",
                    Amount = amount,
                    Balance = customerWallet.Balance
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Payment verification failed: {ex.Message}", ex);
            }
        }

        public async Task RequestPayoutAsync(decimal amount)
        {
            try
            {
                var userId = GetUserId();
                var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
                if (consultant == null)
                    throw new UnauthorizedAccessException("Consultant not found.");

                var wallet = await _walletRepo.GetSingleByAsync(w => w.ConsultantId == consultant.Id);
                if (wallet == null || wallet.Balance < amount)
                    throw new InvalidOperationException("Insufficient wallet balance.");

                if (string.IsNullOrEmpty(consultant.PaystackRecipientCode))
                    throw new InvalidOperationException("Please provide bank details for payout.");

                await _unitOfWork.BeginTransactionAsync();

                wallet.Balance -= amount;
                wallet.LastUpdated = DateTime.UtcNow;
                _walletRepo.Update(wallet);

                var walletTransaction = new WalletTransaction
                {
                    ConsultantId = consultant.Id,
                    CustomerId = null,
                    Amount = -amount,
                    TransactionType = "Payout",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepo.AddAsync(walletTransaction);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                try
                {
                    await _paystackService.InitiateConsultantPayoutAsync(
                        Guid.NewGuid().ToString(),
                        consultant.PaystackRecipientCode,
                        amount
                    );

                    walletTransaction.Status = "Completed";
                    walletTransaction.CompletedAt = DateTime.UtcNow;
                    _walletTransactionRepo.Update(walletTransaction);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception paystackEx)
                {
                    walletTransaction.Status = "Failed";
                    walletTransaction.CompletedAt = DateTime.UtcNow;
                    _walletTransactionRepo.Update(walletTransaction);
                    await _unitOfWork.SaveChangesAsync();

                    throw new Exception($"Payout initiated but Paystack transfer failed: {paystackEx.Message}. Please contact support.", paystackEx);
                }

                if (!string.IsNullOrEmpty(consultant.SendbirdChannelUrl))
                {
                    try
                    {
                        await _sendbirdService.SendAdminMessageAsync(consultant.SendbirdChannelUrl,
                            $"💸 Payout of ₦{amount} initiated. New wallet balance: ₦{wallet.Balance}.");
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new Exception($"Payout request failed: {ex.Message}", ex);
            }
        }
    }
}