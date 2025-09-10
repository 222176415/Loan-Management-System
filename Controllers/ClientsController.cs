using LMS.Database;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        //private field that will be used for interaction
        private readonly ApplicationDbContext dbContext;
        //inject DB Context to interact with the DB
        public ClientsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        // GET: api/<ClientsController>
        [HttpGet]
        public IActionResult GetAllClients()
        {
            try { 
            var clientsList = dbContext.Client.ToListAsync();
            return Ok(clientsList);
            }catch 
            {
                return BadRequest();
            }
        }

        // GET api/<ClientsController>/5
        [HttpGet("{id}")]
        public IActionResult GetEmployeeById(string id)
        {

            var Client = dbContext.Client.FindAsync(id);

            if (Client != null)
            {
                return Ok(Client);
            }
            return NotFound();
        }

        [HttpGet("{IdNumber}")]
        public IActionResult GetEmployeeByIdNumber(string IdNumber)
        {

            var Client = dbContext.Client.FindAsync(IdNumber);

            if (Client != null)
            {
                return Ok(Client);
            }
            return NotFound();
        }

        // POST api/<ClientsController>
        [HttpPost]
        public IActionResult AddEmployee(AddClientDTO addClient)
        {

            var clientEntity = new Client()
            {
                Name = addClient.Name,
                Surname = addClient.Surname,
                IdNumber = addClient.IdNumber,
                Email = addClient.Email,
                Phone = addClient.Phone,

            };


            dbContext.Client.Add(clientEntity);
            dbContext.SaveChanges();
            return Ok(clientEntity);
        }

        // PUT api/<ClientsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ClientsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
