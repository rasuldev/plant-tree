using System.Threading.Tasks;
using PlantTree.Data.Entities;

namespace PlantTree.Infrastructure.Payments
{
    public interface IProcessor
    {
        Task CompleteTransaction(int transactionId, decimal amount, Currency currency);
        Task<int> StartTransaction(Transaction transaction);
    }
}