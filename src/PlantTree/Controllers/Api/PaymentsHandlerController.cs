using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlantTree.Data.Entities;
using PlantTree.Infrastructure.Payments;

namespace PlantTree.Controllers.Api
{
    [Area("api")]
    [Produces("application/json")]
    [Route("api/PaymentsHandler")]
    public class PaymentsHandlerController : Controller
    {
        private readonly ILogger<PaymentsHandlerController> _logger;
        private readonly IProcessor _processor;

        public PaymentsHandlerController(ILogger<PaymentsHandlerController> logger, IProcessor processor)
        {
            _logger = logger;
            _processor = processor;
        }

        [HttpPost("webmoney")]
        public async Task<IActionResult> WebMoney(int transactionId, decimal amount, Currency currency)
        {
            // TODO: save all notifications 
            await _processor.CompleteTransaction(transactionId, amount, currency);
            return Ok();
        }
    }
}