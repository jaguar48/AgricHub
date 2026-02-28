using AgricHub.BLL.Interfaces.IPaystackService;
using AgricHub.Shared.DTO_s.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AgricHub.BLL.Implementations.PaystackService
{
    public class PaystackService : IPaystackService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;
        private readonly ILogger<PaystackService> _logger;

        public PaystackService(
            IConfiguration configuration,
            HttpClient httpClient,
            ILogger<PaystackService> logger)
        {
            _secretKey = configuration["Paystack:SecretKey"];
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.paystack.co/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
            _logger = logger;
        }

        public async Task<(string accessCode, string reference)> InitializeTransactionAsync(
            string email,
            decimal amount,
            string callbackUrl)
        {
            try
            {
                var request = new
                {
                    email,
                    amount = (int)(amount * 100),
                    callback_url = callbackUrl,
                    metadata = new
                    {
                        custom_fields = new[]
                        {
                            new { display_name = "Transaction Type", variable_name = "transaction_type", value = "Wallet Top-Up" }
                        }
                    }
                };

                _logger.LogInformation($"Initializing Paystack transaction for {email}, amount: {amount}");

                var response = await _httpClient.PostAsJsonAsync("transaction/initialize", request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Paystack Response Status: {response.StatusCode}");
                _logger.LogInformation($"Paystack Response Body: {responseContent}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Paystack initialization failed: {responseContent}");
                    throw new InvalidOperationException($"Failed to initialize transaction: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PaystackInitializeResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                _logger.LogInformation($"Parsed Paystack Result - Status: {result?.Status}, AccessCode: {result?.Data?.AccessCode}, Reference: {result?.Data?.Reference}");

                if (result?.Status != true || result?.Data == null)
                {
                    throw new InvalidOperationException($"Invalid response from Paystack. Full response: {responseContent}");
                }

                if (string.IsNullOrEmpty(result.Data.AccessCode))
                {
                    _logger.LogError($"AccessCode is null or empty! Full response: {responseContent}");
                    throw new InvalidOperationException("Paystack returned null AccessCode");
                }

                _logger.LogInformation($"Transaction initialized successfully. Reference: {result.Data.Reference}, AccessCode: {result.Data.AccessCode}");

                return (result.Data.AccessCode, result.Data.Reference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Paystack transaction");
                throw;
            }
        }

        public async Task<PaystackVerificationResponse> VerifyTransactionAsync(string reference)
        {
            try
            {
                var response = await _httpClient.GetAsync($"transaction/verify/{reference}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Paystack verification failed: {responseContent}");
                    throw new InvalidOperationException($"Failed to verify transaction: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PaystackVerificationResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (result?.Status != true)
                {
                    throw new InvalidOperationException("Transaction verification failed");
                }

                _logger.LogInformation($"Transaction verified successfully. Reference: {reference}, Status: {result.Data?.Status}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying Paystack transaction: {reference}");
                throw;
            }
        }

        public async Task InitiateConsultantPayoutAsync(string reference, string recipientCode, decimal amount)
        {
            try
            {
                var request = new
                {
                    source = "balance",
                    reason = $"Payout for consultation {reference}",
                    amount = (int)(amount * 100),
                    recipient = recipientCode
                };

                var response = await _httpClient.PostAsJsonAsync("transfer", request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Paystack transfer failed: {responseContent}");
                    throw new InvalidOperationException($"Failed to initiate payout: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PaystackTransferResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (result?.Status != true)
                {
                    throw new InvalidOperationException("Transfer initiation failed");
                }

                _logger.LogInformation($"Payout initiated successfully. Reference: {reference}, Transfer Code: {result.Data?.TransferCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initiating Paystack payout: {reference}");
                throw;
            }
        }

        public async Task<string> CreateTransferRecipientAsync(
            string accountNumber,
            string accountName,
            string bankCode)
        {
            try
            {
                var request = new
                {
                    type = "nuban",
                    name = accountName,
                    account_number = accountNumber,
                    bank_code = bankCode,
                    currency = "NGN"
                };

                var response = await _httpClient.PostAsJsonAsync("transferrecipient", request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to create transfer recipient: {responseContent}");
                    throw new InvalidOperationException($"Failed to create transfer recipient: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PaystackRecipientResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (result?.Status != true || result?.Data == null)
                {
                    throw new InvalidOperationException("Failed to create transfer recipient");
                }

                _logger.LogInformation($"Transfer recipient created successfully. Recipient Code: {result.Data.RecipientCode}");
                return result.Data.RecipientCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transfer recipient");
                throw;
            }
        }

        // ✅ NEW METHOD 1: Get list of Nigerian banks
        public async Task<List<BankInfo>> GetBanksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("bank?country=nigeria");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to get banks: {responseContent}");
                    throw new InvalidOperationException($"Failed to get banks: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<PaystackBanksResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (result?.Status != true || result?.Data == null)
                {
                    throw new InvalidOperationException("Failed to get banks list");
                }

                _logger.LogInformation($"Retrieved {result.Data.Count} banks from Paystack");

                return result.Data.Select(b => new BankInfo
                {
                    Code = b.Code,
                    Name = b.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banks from Paystack");
                throw;
            }
        }

  
        public async Task<BankAccountDetails> ResolveAccountNumberAsync(string accountNumber, string bankCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"bank/resolve?account_number={accountNumber}&bank_code={bankCode}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to resolve account: {responseContent}");
                    throw new InvalidOperationException($"Invalid bank account details");
                }

                var result = JsonSerializer.Deserialize<PaystackResolveAccountResponse>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (result?.Status != true || result?.Data == null)
                {
                    throw new InvalidOperationException("Invalid bank account details");
                }

                _logger.LogInformation($"Bank account resolved successfully. Account Name: {result.Data.AccountName}");

                return new BankAccountDetails
                {
                    AccountNumber = result.Data.AccountNumber,
                    AccountName = result.Data.AccountName,
                    BankCode = bankCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resolving account number: {accountNumber}");
                throw;
            }
        }
    }

    // ============= EXISTING RESPONSE MODELS =============

    public class PaystackInitializeResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public PaystackInitializeData Data { get; set; }
    }

    public class PaystackInitializeData
    {
        [JsonPropertyName("authorization_url")]
        public string AuthorizationUrl { get; set; }

        [JsonPropertyName("access_code")]
        public string AccessCode { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }
    }

    public class PaystackVerificationResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public PaystackVerificationData Data { get; set; }
    }

    public class PaystackVerificationData
    {
        public string Reference { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime TransactionDate { get; set; }
        public PaystackCustomer Customer { get; set; }
    }

    public class PaystackCustomer
    {
        public string Email { get; set; }
        public string CustomerCode { get; set; }
    }

    public class PaystackTransferResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public PaystackTransferData Data { get; set; }
    }

    public class PaystackTransferData
    {
        public string TransferCode { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaystackRecipientResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public PaystackRecipientData Data { get; set; }
    }

    public class PaystackRecipientData
    {
        [JsonPropertyName("recipient_code")]
        public string RecipientCode { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
    }

    // ✅ NEW RESPONSE MODELS FOR BANK OPERATIONS

    public class PaystackBanksResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<PaystackBank> Data { get; set; }
    }

    public class PaystackBank
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Longcode { get; set; }
        public string Gateway { get; set; }
    }

    public class PaystackResolveAccountResponse
    {

        public bool Status { get; set; }
        public string Message { get; set; }
        public PaystackAccountData Data { get; set; }
    }

    public class PaystackAccountData
    {
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; }

        [JsonPropertyName("account_name")]
        public string AccountName { get; set; }

        [JsonPropertyName("bank_id")]
        public int BankId { get; set; }
    }
}