using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlantTree.Data;
using PlantTree.Data.Entities;

namespace PlantTree.Infrastructure.Payments
{
    public class Processor : IProcessor
    {
        private readonly AppDbContext _context;
        private readonly ILogger<Processor> _logger;

        public Processor(AppDbContext context, ILogger<Processor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> StartTransaction(Transaction transaction)
        {
            _context.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction.Id;
        }
        
        public async Task<int> StartTransaction(int projectId, string userId, decimal amount, int treeCount, string paymentMethod)
        {
            var transaction = new Transaction()
            {
                ProjectId = projectId,
                UserId = userId,
                Amount = amount,
                TreeCount =  treeCount,
                PaymentMethod = paymentMethod
            };
            return await StartTransaction(transaction);
        }

        public async Task CompleteTransaction(int transactionId, decimal amount, Currency currency)
        {
            var transaction = await _context.Transactions.SingleOrDefaultAsync(t => t.Id == transactionId);
            if (transaction == null)
            {
                _logger.LogCritical($"Closing non-existing transaction with id={transactionId}");
                return;
            }

            VerifyCorrespondence(transaction, amount, currency);
            transaction.Status = TransactionStatus.Success;
            
            var project = await _context.Projects.SingleOrDefaultAsync(p => p.Id == transaction.ProjectId);
            if (project == null)
            {
                _logger.LogCritical($"Received funds for non-existing project with id={transaction.ProjectId}");
                return;
            }

            project.AddTrees(transaction.TreeCount);
            project.DonatorsCount = await _context.Transactions.Select(t => t.UserId).Distinct().CountAsync();
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Verifies that amount and currency of transaction is equal to <paramref name="amount"/> and <paramref name="currency"/>
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="amount"></param>
        /// <param name="currency"></param>
        private void VerifyCorrespondence(Transaction transaction, decimal amount, Currency currency)
        {
            if (transaction.Amount == amount && transaction.Currency == currency) return;

            _logger.LogCritical($"Suspicious transaction completion: was {transaction.Amount}{transaction.Currency}, but received {amount}{currency}.");
            transaction.Amount = amount;
            transaction.Currency = currency;
        }
    }
}