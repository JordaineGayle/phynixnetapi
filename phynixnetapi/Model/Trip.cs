using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phynixnetapi.Model
{
    public class Trip
    {
        public string id { get; set; }

        public string DriverId { get; set; }

        public string UserId { get; set; }

        public double TotalTripPrice { get; set; }

        public Location Origin { get; set; }

        public Location Destination { get; set; }

        public DateTime DateOfTrip {get;set;}

        public double Rating { get; set; }

        public string Comment { get; set; }

        public double PhynixCharge { get; set; }

        public bool IsPayed { get; set; }

        public string AddtionalInfo { get; set; }
	}
}
