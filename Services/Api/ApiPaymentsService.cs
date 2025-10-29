using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiPaymentsService : ApiClient, IApiPaymentsService
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

        // Implementação da interface IApiPaymentsService
        public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetAllAsync()
        {
            return await GetAsync<IEnumerable<PaymentDto>>("payments");
        }

        public async Task<ApiResponse<PaymentDto>> GetByIdAsync(int id)
        {
            return await GetAsync<PaymentDto>($"payments/{id}");
        }

        public async Task<ApiResponse<PaymentDto>> CreateAsync(PaymentDto payment)
        {
            return await CreatePaymentAsync(payment);
        }

        public async Task<ApiResponse<PaymentDto>> UpdateAsync(int id, PaymentDto payment)
        {
            return await PutAsync<PaymentDto>($"payments/{id}", payment);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"payments/{id}");
        }

        public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetByClientIdAsync(int clientId)
        {
            return await GetAsync<IEnumerable<PaymentDto>>($"payments/client/{clientId}");
        }

        public async Task<ApiResponse<IEnumerable<PaymentDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAsync<IEnumerable<PaymentDto>>($"payments/range/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}");
        }
    }
}
