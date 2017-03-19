using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthTokenServer.Common;
using Common.Async;
using Common.Errors;
using Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Payments;

namespace PlantTree.Controllers.Api
{
    [Produces("application/json")]
    [Route("api/Payments")]
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IProcessor _processor;

        public PaymentsController(ILogger<PaymentsController> logger, IProcessor processor)
        {
            _logger = logger;
            _processor = processor;
        }

        [HttpPost("webmoney")]
        public async Task<IActionResult> WebMoney(int projectId, decimal amount, Currency? currency)
        {
            var result = await StartWebmoneyPay(projectId, amount, currency);
            if (result.Succeeded)
                return Ok(result.Result);
            return new ApiErrorResult(result.Errors.ToArray());
        }

        [NonAction]
        public async Task<OperationResult<int>> StartWebmoneyPay(int projectId, decimal amount, Currency? currency)
        {
            var transaction = new Transaction()
            {
                ProjectId = projectId,
                Amount = amount,
                Currency = currency,
                PaymentMethod = PaymentMethods.WebMoney.ToString(),
                UserId = SecurityRoutines.GetUserId(HttpContext)
            };
            var errors = VerifyTransaction(transaction);
            if (errors.Any())
                return OperationResult<int>.Failed(errors.ToArray());
            try
            {
                var transactionId = await _processor.StartTransaction(transaction);
                return OperationResult<int>.Success(transactionId);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return OperationResult<int>.Failed(new ApiError("Create transaction error: " + e.Message));
            }

        }

        /// <summary>
        /// Checks for necessary fields
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private List<ApiError> VerifyTransaction(Transaction transaction)
        {
            var errors = new List<ApiError>();
            if (transaction.ProjectId == 0)
                errors.Add(new ApiError("Create transaction error: ProjectId is missing"));
            if (transaction.Amount < 0.01M)
                errors.Add(new ApiError("Create transaction error: wrong value for amount: it should be >= 0.01"));
            if (!transaction.Currency.HasValue)
                errors.Add(new ApiError("Create transaction error: currency is missing"));
            return errors;
        }

    }
}