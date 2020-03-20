using System.ComponentModel.DataAnnotations;

namespace Novibet.IpStack.Business.Models
{
    public class Country
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
