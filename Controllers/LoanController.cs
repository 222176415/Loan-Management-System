using LMS.Database;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        //inject DB Context to interact with the DB
        public LoanController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        // GET: api/<LoanController>
        [HttpGet]
        public IActionResult GetAllLoans()
        {
            try
            {
                var ALlLoans = dbContext.Loans.ToListAsync();
                return Ok(ALlLoans);
            }
            catch
            {
                return BadRequest();
            }
        }

        // GET api/Loan/5
        [HttpGet("{ClientId}")]
        public IActionResult GetLoanByCustomerId(string ClientId)
        {
            var loan = dbContext.Loans.FindAsync(ClientId);

            if (loan != null)
            {
                return Ok(loan);
            }

            return NotFound();
        }

        // POST api/<LoanApplication1>
        [HttpPost]
        public IActionResult AddLoanApplication(AddLoanApplicationDTO AddLoanApplication)
        {

            var NewLoanApplication = new Loan()
            {
                LoanAmount = AddLoanApplication.LoanAmount,
                LoanType = AddLoanApplication.LoanType,
                InterestRate = AddLoanApplication.InterestRate,
                TermMonths = AddLoanApplication.TermMonths,
                Status = AddLoanApplication.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Client = AddLoanApplication.Client

            };


            dbContext.Loans.Add(NewLoanApplication);
            dbContext.SaveChanges();
            return Ok(NewLoanApplication);
        }

        // PUT api/<LoanController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LoanController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
