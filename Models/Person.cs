using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PeopleAPI.Models
{
    public class Person
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Sex { get; set; }
        public int? Phone { get; set; }

    }
}
