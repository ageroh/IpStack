using System.ComponentModel.DataAnnotations;

namespace Novibet.IpStack.Business.Models
{
    public class Ip
    {
        [Key]
        public int Id { get; set; }

        public string IpAddress { get; set; }

        public int CityId { get; set; }

        public int CountryId { get; internal set; }

        public int ContinentId { get; internal set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public City City { get; set; }

        public Country Country { get; set; }

        public Continent Continent { get; set; }
    }
}