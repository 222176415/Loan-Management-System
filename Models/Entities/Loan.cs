namespace LMS.Models.Entities
{
    public class Loan
    {

        public string Id { get; set; }

        public required int LoanAmount { get; set; }
        public required string LoanType { get; set; }

        public required string InterestRate { set; get; }

        public required string TermMonths { set; get; }

        public required string Status { set; get; }

        public DateTime? CreatedAt { set; get; }

        public DateTime? UpdatedAt { set; get; }
        public Client Client { set; get; }
    }

    public class AddLoanApplicationDTO{
        public required int LoanAmount { get; set; }
        public required string LoanType { get; set; }

        public required string InterestRate { set; get; }

        public required string TermMonths { set; get; }

        public required string Status { set; get; }
        public Client Client { set; get; }
        //public DateTime? CreatedAt { set; get; }
    }

}
