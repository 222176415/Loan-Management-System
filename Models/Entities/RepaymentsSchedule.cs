namespace LMS.Models.Entities
{
    public class RepaymentsSchedule
    {
        public string Id { get; set; }
     

        public required string InstallmentNumber { set; get; }


        public required int AmountDue { set; get; }

        public string? Status { set; get; }
        public  DateTime? DueDate { set; get; }

        public ICollection<Loan> Loans { get; set; }
    }
}
