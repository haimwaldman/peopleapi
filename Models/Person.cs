using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PeopleAPI.Models
{
    public enum Sex
    {
        Male,
        Female,
        Other
    }
    public class Person
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Email { get; set; }
        [Required]
        public string DateOfBirth { get; set; }
        public Sex Sex { get; set; }
        public int Phone { get; set; }

    }
}
