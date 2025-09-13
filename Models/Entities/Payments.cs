namespace LMS.Models.Entities
{
    public class Payments
    {
        public string Id { get; set; }
        public required int AmountPaid { get; set; }

        public required string PaymentMethod { set; get; }

        public required string TransactionReference { set; get; }
        public required DateTime PaymentDate { get; set; }
        public ICollection<RepaymentsSchedule> RepaymentSchedule { set; get; }
    }
}
