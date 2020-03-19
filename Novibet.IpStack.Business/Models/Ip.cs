using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Novibet.IpStack.Abstractions;

namespace Novibet.IpStack.Business.Models
{
    public class Ip
    {
        public string IpAddress { get; set; }

        public City City { get; set; }

        public Country Country { get; set; }

        public Continent Continent { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class Country
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class City
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Continent
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

}
