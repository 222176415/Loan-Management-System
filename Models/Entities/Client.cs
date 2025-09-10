using System.ComponentModel.DataAnnotations;

namespace LMS.Models.Entities
{
    public class Client
    {
        public string Id { get; set; }
        public required string IdNumber { set; get; }
        public required string Name {  get; set; }
        public required string Surname { get; set; }

        public required string Email { set; get; }
       
        public required string Phone { set; get; }
    }

    public class AddClientDTO
    {
        public required string IdNumber { set; get; }
        public required string Name { get; set; }
        public required string Surname { get; set; }
        public required string Email { set; get; }

        public required string Phone { set; get; }
    }
}
