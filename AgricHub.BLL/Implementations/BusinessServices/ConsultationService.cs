// AgricHub.BLL/Implementations/BusinessServices/ConsultationService.cs

using AgricHub.BLL.Interfaces.ChatServices;
using AgricHub.BLL.Interfaces.IBusinessServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgricHub.BLL.Implementations.BusinessServices
{
    public class ConsultationService : IConsultationService
    {
        private readonly IRepository<Consultation> _consultationRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Consultant> _consultantRepo;
        private readonly IRepository<Service> _servicesRepo;
        private readonly IRepository<ServicePackage> _servicePackageRepo;
        private readonly IRepository<Business> _businessRepo;
        private readonly IRepository<ChatSession> _chatSessionRepo;
        private readonly IRepository<Wallet> _walletRepo;
        private readonly IRepository<PendingTransaction> _pendingTransactionRepo;
        private readonly IRepository<WalletTransaction> _walletTransactionRepo;
        private readonly ISendbirdService _sendbirdService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        private const decimal CustomerNoShowPayoutPercentage = 0.5m;
        private const int GracePeriodMinutes = 15;

        public ConsultationService(
            IUnitOfWork unitOfWork,
            ISendbirdService sendbirdService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _unitOfWork             = unitOfWork;
            _consultationRepo       = _unitOfWork.GetRepository<Consultation>();
            _customerRepo           = _unitOfWork.GetRepository<Customer>();
            _consultantRepo         = _unitOfWork.GetRepository<Consultant>();
            _servicesRepo           = _unitOfWork.GetRepository<Service>();
            _servicePackageRepo     = _unitOfWork.GetRepository<ServicePackage>();
            _businessRepo           = _unitOfWork.GetRepository<Business>();
            _chatSessionRepo        = _unitOfWork.GetRepository<ChatSession>();
            _walletRepo             = _unitOfWork.GetRepository<Wallet>();
            _pendingTransactionRepo = _unitOfWork.GetRepository<PendingTransaction>();
            _walletTransactionRepo  = _unitOfWork.GetRepository<WalletTransaction>();
            _sendbirdService        = sendbirdService;
            _httpContextAccessor    = httpContextAccessor;
            _mapper                 = mapper;
        }

        // ── Auth helpers ───────────────────────────────────────────────────────

        private string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated.");
            return userId;
        }

        private async Task EnsureConsultantOwnershipAsync(Consultation consultation, string userId)
        {
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId);
            if (consultant == null || consultant.Id != consultation.ConsultantId)
                throw new UnauthorizedAccessException("You are not authorized to manage this consultation.");
        }

        private async Task EnsureCustomerOrConsultantAsync(Consultation consultation, string userId)
        {
            var full = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultation.Id,
                include: q => q.Include(c => c.Customer).Include(c => c.Consultant))
                ?? throw new KeyNotFoundException("Consultation not found.");

            if (full.Customer?.UserId != userId && full.Consultant?.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to perform this action.");
        }

        // ── Shared: find escrow (always "Held") ───────────────────────────────
        private Task<PendingTransaction?> FindEscrowAsync(Guid consultationId) =>
            _pendingTransactionRepo.GetSingleByAsync(
                pt => pt.ConsultationId == consultationId && pt.Status == "Held");

        // ── Book ───────────────────────────────────────────────────────────────

        public async Task<ConsultationResponse> BookConsultationAsync(ConsultationBookingRequest dto)
        {
            var userId = GetUserId();
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId)
                           ?? throw new UnauthorizedAccessException("Customer not found.");

            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == dto.ConsultantUserId)
                             ?? throw new KeyNotFoundException("Consultant not found.");

            var service = await _servicesRepo.GetSingleByAsync(s => s.Id == dto.ServiceId,
                include: q => q.Include(s => s.Business).Include(s => s.Packages))
                ?? throw new KeyNotFoundException("Service not found.");

            var package = service.Packages.FirstOrDefault(p => p.Id == dto.ServicePackageId)
                          ?? throw new KeyNotFoundException("Service package not found.");

            var business = await _businessRepo.GetSingleByAsync(
                b => b.Id == service.BusinessId && b.ConsultantId == consultant.Id)
                ?? throw new UnauthorizedAccessException("This service does not belong to the specified consultant.");

            var isSlotTaken = await _consultationRepo.AnyAsync(c =>
                c.ConsultantId == consultant.Id && c.ScheduledAt == dto.ScheduledAt);
            if (isSlotTaken)
                throw new InvalidOperationException("This time slot is already booked.");

            var customerWallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == customer.Id)
                                 ?? throw new InvalidOperationException("Customer wallet not found.");
            if (customerWallet.Balance < package.Price)
                throw new InvalidOperationException("Insufficient wallet balance. Please top up your wallet.");

            // Deduct from wallet
            customerWallet.Balance    -= package.Price;
            customerWallet.LastUpdated = DateTime.UtcNow;
            _walletRepo.Update(customerWallet);

            // Build consultation
            var consultation = new Consultation
            {
                Id                    = Guid.NewGuid(),
                CustomerId            = customer.Id,
                ConsultantId          = consultant.Id,
                ServiceId             = dto.ServiceId,
                ServicePackageId      = dto.ServicePackageId,
                ScheduledAt           = dto.ScheduledAt,
                EndAt                 = dto.ScheduledAt.AddMinutes(package.DurationMinutes),
                Status                = "Pending",
                CreatedAt             = DateTime.UtcNow,
                IsCustomOffer         = false,
                CustomPrice           = null,
                CustomDurationMinutes = null
            };

            // Chat session
            var existingChat = await _chatSessionRepo.GetSingleByAsync(cs =>
                cs.CustomerId   == customer.Id &&
                cs.ConsultantId == consultant.Id &&
                cs.ServiceId    == dto.ServiceId);

            if (existingChat != null)
            {
                consultation.SendbirdChannelUrl = existingChat.SendbirdChannelUrl;
            }
            else
            {
                await _sendbirdService.EnsureSendbirdUserAsync(customer.UserId, $"{customer.FirstName} {customer.LastName}");
                await _sendbirdService.EnsureSendbirdUserAsync(consultant.UserId, $"{consultant.FirstName} {consultant.LastName}");
                var channelUrl = await _sendbirdService.CreateGroupChannelAsync(customer.UserId, consultant.UserId);
                consultation.SendbirdChannelUrl = channelUrl;

                await _chatSessionRepo.AddAsync(new ChatSession
                {
                    Id                 = Guid.NewGuid(),
                    CustomerId         = customer.Id,
                    ConsultantId       = consultant.Id,
                    ServiceId          = dto.ServiceId,
                    SendbirdChannelUrl = channelUrl,
                    CreatedAt          = DateTime.UtcNow
                });
            }

            await _consultationRepo.AddAsync(consultation);

            // Escrow — Status = "Held"
            await _pendingTransactionRepo.AddAsync(new PendingTransaction
            {
                Id             = Guid.NewGuid(),
                CustomerId     = customer.Id,
                ConsultationId = consultation.Id,
                Amount         = package.Price,
                Status         = "Held",          // ← always "Held"
                CreatedAt      = DateTime.UtcNow
            });

            // Wallet transaction record
            await _walletTransactionRepo.AddAsync(new WalletTransaction
            {
                CustomerId                   = customer.Id,
                ConsultantId                 = null,
                Amount                       = -package.Price,
                PaystackTransactionReference = null,
                TransactionType              = "ConsultationPayment",
                Status                       = "Completed",
                CreatedAt                    = DateTime.UtcNow,
                CompletedAt                  = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();

            // Reload with navigation properties
            var saved = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultation.Id,
                include: q => q.Include(c => c.Customer).Include(c => c.Consultant)
                               .Include(c => c.Service).Include(c => c.ServicePackage));

            var escrow = await FindEscrowAsync(consultation.Id);
            var response = _mapper.Map<ConsultationResponse>(saved);
            response.PendingAmount = escrow?.Amount ?? 0;
            response.Notes         = dto.Notes;

            var message = string.IsNullOrWhiteSpace(dto.Notes)
                ? $"Booking request for service: {service.ServiceName} ({package.PackageName}). Price: ₦{package.Price} (paid from wallet, held in escrow)."
                : $"Booking request for service: {service.ServiceName} ({package.PackageName}). Price: ₦{package.Price} (paid from wallet, held in escrow). Notes: {dto.Notes}";

            await _sendbirdService.SendMessageAsync(
                saved!.SendbirdChannelUrl, customer.UserId, message, false,
                new
                {
                    ServiceId = service.Id,
                    ServiceName = service.ServiceName,
                    PackageId = package.Id,
                    PackageName = package.PackageName,
                    Price = package.Price
                });

            return response;
        }

        // ── Approve ────────────────────────────────────────────────────────────

        public async Task<ConsultationResponse> ApproveConsultationAsync(Guid consultationId, string? notes = null)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultationId,
                include: q => q.Include(c => c.Service).Include(c => c.ServicePackage)
                               .Include(c => c.Customer).Include(c => c.Consultant))
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);
            if (consultation.Status != "Pending")
                throw new InvalidOperationException("Only pending consultations can be approved.");

            var escrow = await FindEscrowAsync(consultationId)
                ?? throw new InvalidOperationException("No escrow found for this consultation.");

            if (string.IsNullOrEmpty(consultation.SendbirdChannelUrl))
            {
                await _sendbirdService.EnsureSendbirdUserAsync(consultation.Customer.UserId, $"{consultation.Customer.FirstName} {consultation.Customer.LastName}");
                await _sendbirdService.EnsureSendbirdUserAsync(consultation.Consultant.UserId, $"{consultation.Consultant.FirstName} {consultation.Consultant.LastName}");
                consultation.SendbirdChannelUrl = await _sendbirdService.CreateGroupChannelAsync(
                    consultation.Customer.UserId, consultation.Consultant.UserId);
            }

            consultation.Status = "Approved";
            await _consultationRepo.UpdateAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            var amount = consultation.IsCustomOffer ? consultation.CustomPrice!.Value : consultation.ServicePackage.Price;
            var message = string.IsNullOrEmpty(notes)
                ? $"✅ Consultation approved for service: {consultation.Service.ServiceName} ({consultation.ServicePackage.PackageName}). ₦{amount} held in escrow."
                : $"✅ Consultation approved for service: {consultation.Service.ServiceName} ({consultation.ServicePackage.PackageName}). ₦{amount} held in escrow. Notes: {notes}";
            await _sendbirdService.SendAdminMessageAsync(consultation.SendbirdChannelUrl, message);

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        // ── Reject ─────────────────────────────────────────────────────────────

        public async Task<ConsultationResponse> RejectConsultationAsync(Guid consultationId, string reason)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultationId,
                include: q => q.Include(c => c.Service).Include(c => c.ServicePackage).Include(c => c.Customer))
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);
            if (consultation.Status != "Pending")
                throw new InvalidOperationException("Only pending consultations can be rejected.");

            var escrow = await FindEscrowAsync(consultationId);
            if (escrow != null)
            {
                var wallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == consultation.CustomerId)
                             ?? throw new InvalidOperationException("Customer wallet not found.");
                wallet.Balance    += escrow.Amount;
                wallet.LastUpdated = DateTime.UtcNow;
                _walletRepo.Update(wallet);

                escrow.Status     = "Refunded";
                escrow.ResolvedAt = DateTime.UtcNow;
                _pendingTransactionRepo.Update(escrow);

                await _walletTransactionRepo.AddAsync(new WalletTransaction
                {
                    CustomerId = consultation.CustomerId,
                    ConsultantId = null,
                    Amount = escrow.Amount,
                    TransactionType = "ConsultationRefund",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
            }

            consultation.Status = "Rejected";
            await _consultationRepo.UpdateAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            var saved = await _consultationRepo.GetSingleByAsync(c => c.Id == consultation.Id,
                include: q => q.Include(c => c.Customer).Include(c => c.Consultant)
                               .Include(c => c.Service).Include(c => c.ServicePackage));
            var response = _mapper.Map<ConsultationResponse>(saved);
            response.PendingAmount = escrow?.Amount ?? 0;

            await _sendbirdService.SendAdminMessageAsync(consultation.SendbirdChannelUrl,
                $"❌ Consultation rejected. Reason: {reason}. Refunded: ₦{escrow?.Amount ?? 0}.");

            return response;
        }

        // ── Start ──────────────────────────────────────────────────────────────

        public async Task<ConsultationResponse> StartConsultationAsync(Guid consultationId)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultationId,
                include: q => q.Include(c => c.Service).Include(c => c.ServicePackage))
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);
            if (consultation.Status != "Approved")
                throw new InvalidOperationException("Only approved consultations can be started.");

            var escrow = await FindEscrowAsync(consultationId)
                ?? throw new InvalidOperationException("No escrow found for this consultation.");

            consultation.Status = "In Progress";
            await _consultationRepo.UpdateAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            var amount = consultation.IsCustomOffer ? consultation.CustomPrice!.Value : consultation.ServicePackage.Price;
            await _sendbirdService.SendAdminMessageAsync(consultation.SendbirdChannelUrl,
                $"🚀 Consultation started for service: {consultation.Service.ServiceName} ({consultation.ServicePackage.PackageName}). ₦{amount} held in escrow.");

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        // ── Complete ───────────────────────────────────────────────────────────

        public async Task<ConsultationResponse> CompleteConsultationAsync(Guid consultationId)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultationId,
                include: q => q.Include(c => c.Service).Include(c => c.ServicePackage)
                               .Include(c => c.Consultant).Include(c => c.Customer))
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);
            if (consultation.Status != "In Progress")
                throw new InvalidOperationException("Only in-progress consultations can be completed.");

            var escrow = await FindEscrowAsync(consultationId)
                ?? throw new InvalidOperationException("No escrow found for this consultation.");

            // Release funds to consultant
            var consultantWallet = await _walletRepo.GetSingleByAsync(w => w.ConsultantId == consultation.ConsultantId)
                                   ?? throw new InvalidOperationException("Consultant wallet not found.");
            consultantWallet.Balance    += escrow.Amount;
            consultantWallet.LastUpdated = DateTime.UtcNow;
            _walletRepo.Update(consultantWallet);

            escrow.Status     = "Released";
            escrow.ResolvedAt = DateTime.UtcNow;
            _pendingTransactionRepo.Update(escrow);

            await _walletTransactionRepo.AddAsync(new WalletTransaction
            {
                CustomerId = null,
                ConsultantId = consultation.ConsultantId,
                Amount = escrow.Amount,
                TransactionType = "ConsultationPayout",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            });

            consultation.Status      = "Completed";
            consultation.CompletedAt = DateTime.UtcNow;
            await _consultationRepo.UpdateAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            var amount = consultation.IsCustomOffer ? consultation.CustomPrice!.Value : consultation.ServicePackage.Price;
            await _sendbirdService.SendAdminMessageAsync(consultation.SendbirdChannelUrl,
                $"✅ Consultation completed for service: {consultation.Service.ServiceName} ({consultation.ServicePackage.PackageName}). ₦{amount} released to consultant wallet.");

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        // ── Cancel ─────────────────────────────────────────────────────────────

        public async Task<ConsultationResponse> CancelConsultationAsync(Guid consultationId, string? reason = null)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultationId,
                include: q => q.Include(c => c.Service).Include(c => c.ServicePackage).Include(c => c.Customer))
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureCustomerOrConsultantAsync(consultation, userId);
            if (consultation.Status is "Completed" or "Rejected")
                throw new InvalidOperationException("Cannot cancel a completed or rejected consultation.");

            var escrow = await FindEscrowAsync(consultationId);
            if (escrow != null)
            {
                var wallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == consultation.CustomerId)
                             ?? throw new InvalidOperationException("Customer wallet not found.");
                wallet.Balance    += escrow.Amount;
                wallet.LastUpdated = DateTime.UtcNow;
                _walletRepo.Update(wallet);

                escrow.Status     = "Refunded";
                escrow.ResolvedAt = DateTime.UtcNow;
                _pendingTransactionRepo.Update(escrow);

                await _walletTransactionRepo.AddAsync(new WalletTransaction
                {
                    CustomerId = consultation.CustomerId,
                    ConsultantId = null,
                    Amount = escrow.Amount,
                    TransactionType = "ConsultationRefund",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
            }

            consultation.Status = "Cancelled";
            await _consultationRepo.UpdateAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            var msg = string.IsNullOrEmpty(reason)
                ? $"⚠️ Consultation cancelled. Refunded: ₦{escrow?.Amount ?? 0}."
                : $"⚠️ Consultation cancelled. Reason: {reason}. Refunded: ₦{escrow?.Amount ?? 0}.";
            await _sendbirdService.SendAdminMessageAsync(consultation.SendbirdChannelUrl, msg);

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        // ── Consultant No-show ─────────────────────────────────────────────────

        public async Task<ConsultationResponse> ReportConsultantNoShowAsync(Guid consultationId)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultationId,
                include: q => q.Include(c => c.Service).Include(c => c.ServicePackage)
                               .Include(c => c.Consultant).Include(c => c.Customer))
                ?? throw new KeyNotFoundException("Consultation not found.");

            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId)
                           ?? throw new UnauthorizedAccessException("Customer not found.");
            if (consultation.CustomerId != customer.Id)
                throw new UnauthorizedAccessException("You are not authorized to report no-show for this consultation.");

            var gracePeriodEnd = consultation.ScheduledAt.AddMinutes(GracePeriodMinutes);
            if (DateTime.UtcNow < gracePeriodEnd)
                throw new InvalidOperationException($"Please wait until the grace period ends ({gracePeriodEnd:yyyy-MM-dd HH:mm}).");

            if (consultation.Status is not ("Approved" or "In Progress"))
                throw new InvalidOperationException("Cannot report no-show for this consultation status.");

            // Find escrow — gracefully handle missing
            var escrow = await FindEscrowAsync(consultationId);

            consultation.ConsultantNoShowReported = true;
            consultation.Status                   = "Missed";
            consultation.Consultant.NoShowCount   = (consultation.Consultant.NoShowCount ?? 0) + 1;
            _consultantRepo.Update(consultation.Consultant);

            if (escrow != null)
            {
                var wallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == consultation.CustomerId)
                             ?? throw new InvalidOperationException("Customer wallet not found.");
                wallet.Balance    += escrow.Amount;
                wallet.LastUpdated = DateTime.UtcNow;
                _walletRepo.Update(wallet);

                escrow.Status     = "Refunded";
                escrow.ResolvedAt = DateTime.UtcNow;
                _pendingTransactionRepo.Update(escrow);

                await _walletTransactionRepo.AddAsync(new WalletTransaction
                {
                    CustomerId = consultation.CustomerId,
                    ConsultantId = null,
                    Amount = escrow.Amount,
                    TransactionType = "ConsultantNoShowRefund",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
            }

            await _consultationRepo.UpdateAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            await _sendbirdService.SendAdminMessageAsync(consultation.SendbirdChannelUrl,
                $"❌ Consultant no-show reported. ₦{escrow?.Amount ?? 0} refunded to customer wallet.");

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        // ── Customer No-show ───────────────────────────────────────────────────

        public async Task<ConsultationResponse> ReportCustomerNoShowAsync(Guid consultationId)
        {
            var userId = GetUserId();
            var consultation = await _consultationRepo.GetSingleByAsync(
                c => c.Id == consultationId,
                include: q => q.Include(c => c.Service).Include(c => c.ServicePackage)
                               .Include(c => c.Consultant).Include(c => c.Customer))
                ?? throw new KeyNotFoundException("Consultation not found.");

            await EnsureConsultantOwnershipAsync(consultation, userId);

            var gracePeriodEnd = consultation.ScheduledAt.AddMinutes(GracePeriodMinutes);
            if (DateTime.UtcNow < gracePeriodEnd)
                throw new InvalidOperationException($"Please wait until the grace period ends ({gracePeriodEnd:yyyy-MM-dd HH:mm}).");

            if (consultation.Status is not ("Approved" or "In Progress"))
                throw new InvalidOperationException("Cannot report no-show for this consultation status.");

            var escrow = await FindEscrowAsync(consultationId);

            consultation.CustomerNoShowReported = true;
            consultation.Status                 = "Missed";
            consultation.Customer.NoShowCount   = (consultation.Customer.NoShowCount ?? 0) + 1;
            _customerRepo.Update(consultation.Customer);

            if (escrow != null)
            {
                var amount = consultation.IsCustomOffer ? consultation.CustomPrice!.Value : consultation.ServicePackage.Price;
                var consultantPayout = amount * CustomerNoShowPayoutPercentage;
                var customerRefund = amount - consultantPayout;

                var customerWallet = await _walletRepo.GetSingleByAsync(w => w.CustomerId == consultation.CustomerId)
                                     ?? throw new InvalidOperationException("Customer wallet not found.");
                customerWallet.Balance    += customerRefund;
                customerWallet.LastUpdated = DateTime.UtcNow;
                _walletRepo.Update(customerWallet);

                var consultantWallet = await _walletRepo.GetSingleByAsync(w => w.ConsultantId == consultation.ConsultantId)
                                       ?? throw new InvalidOperationException("Consultant wallet not found.");
                consultantWallet.Balance    += consultantPayout;
                consultantWallet.LastUpdated = DateTime.UtcNow;
                _walletRepo.Update(consultantWallet);

                escrow.Status     = "PartiallyRefunded";
                escrow.ResolvedAt = DateTime.UtcNow;
                _pendingTransactionRepo.Update(escrow);

                await _walletTransactionRepo.AddAsync(new WalletTransaction
                {
                    CustomerId = consultation.CustomerId,
                    ConsultantId = null,
                    Amount = customerRefund,
                    TransactionType = "CustomerNoShowRefund",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });
                await _walletTransactionRepo.AddAsync(new WalletTransaction
                {
                    CustomerId = null,
                    ConsultantId = consultation.ConsultantId,
                    Amount = consultantPayout,
                    TransactionType = "CustomerNoShowPayout",
                    Status = "Completed",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow
                });

                await _sendbirdService.SendAdminMessageAsync(consultation.SendbirdChannelUrl,
                    $"❌ Customer no-show reported. ₦{consultantPayout} credited to consultant, ₦{customerRefund} refunded to customer.");
            }

            await _consultationRepo.UpdateAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ConsultationResponse>(consultation);
        }

        // ── Read ───────────────────────────────────────────────────────────────

        public async Task<IEnumerable<ConsultationResponse>> GetMyConsultationsAsync()
        {
            var userId = GetUserId();
            var customer = await _customerRepo.GetSingleByAsync(c => c.UserId == userId)
                           ?? throw new UnauthorizedAccessException("Customer not found.");

            var consultations = await _consultationRepo.GetAllAsync(
                c => c.CustomerId == customer.Id,
                include: q => q.Include(c => c.Consultant).Include(c => c.Service).Include(c => c.ServicePackage));

            var responses = _mapper.Map<IEnumerable<ConsultationResponse>>(consultations).ToList();
            foreach (var r in responses)
            {
                var escrow = await FindEscrowAsync(r.Id);
                r.PendingAmount  = escrow?.Amount ?? 0;
            }
            return responses;
        }

        public async Task<IEnumerable<ConsultationResponse>> GetConsultantConsultationsAsync()
        {
            var userId = GetUserId();
            var consultant = await _consultantRepo.GetSingleByAsync(c => c.UserId == userId)
                             ?? throw new UnauthorizedAccessException("Consultant not found.");

            var consultations = await _consultationRepo.GetAllAsync(
                c => c.ConsultantId == consultant.Id,
                include: q => q.Include(c => c.Customer).Include(c => c.Service).Include(c => c.ServicePackage));

            var responses = _mapper.Map<IEnumerable<ConsultationResponse>>(consultations).ToList();
            foreach (var r in responses)
            {
                var escrow = await FindEscrowAsync(r.Id);
                r.PendingAmount = escrow?.Amount ?? 0;
            }
            return responses;
        }
    }
}