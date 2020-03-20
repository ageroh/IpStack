using System.ComponentModel.DataAnnotations;

namespace Novibet.IpStack.Business.Models
{
    public class Continent
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
