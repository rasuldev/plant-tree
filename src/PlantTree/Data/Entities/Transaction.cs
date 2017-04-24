using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace PlantTree.Data.Entities
{
    /// <summary>
    /// When created Transaction has a Pending state. 
    /// While it has Pending state it is considered as opened (waiting state). 
    /// Transaction can be closed with setting to Success or Failed state.
    /// </summary>
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [IgnoreDataMember]
        public virtual ApplicationUser User { get; set; }
        public int ProjectId { get; set; }
        [IgnoreDataMember]
        public virtual Project Project { get; set; }
        public decimal Amount  { get; set; }
        public int TreeCount { get; set; }
        [Required]
        public Currency? Currency { get; set; } = Entities.Currency.Ruble;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime? FinishedDate { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string PaymentMethod { get; set; }
    }

    public enum TransactionStatus
    {
        // Transaction is set to this status when we receive a notification 
        // from one of the payment systems about transfering funds
        Success, 

        // Initial status for transaction: transaction was started by user in client, 
        // but money hadn't yet transfered
        Pending, 

        // Transaction was not completed
        Fail
    }
}