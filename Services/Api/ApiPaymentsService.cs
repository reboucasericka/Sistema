using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiPaymentsService : ApiClient
    {
        public ApiPaymentsService(HttpClient httpClient, ILogger<ApiPaymentsService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetPayablesAsync()
        {
            return await GetAsync<IEnumerable<PaymentDto>>("payments/payables");
        }

        public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetReceivablesAsync()
        {
            return await GetAsync<IEnumerable<PaymentDto>>("payments/receivables");
        }

        public async Task<ApiResponse<PaymentDto>> CreatePaymentAsync(PaymentDto payment)
        {
            return await PostAsync<PaymentDto>("payments", payment);
        }

        public async Task<ApiResponse<bool>> ProcessPaymentAsync(int paymentId)
        {
            return await PostAsync<bool>($"payments/{paymentId}/process", new { });
        }

        public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetPaymentMethodsAsync()
        {
            return await GetAsync<IEnumerable<PaymentDto>>("payments/methods");
        }
    }
}
