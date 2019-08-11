using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phynixnetapi.Model
{
	public class Driver
	{
		public string id { get; set; }

		public string Firstname { get; set; }

		public string Lastname { get; set; }

		public string Email { get; set; }

		public string Phone { get; set; }

		public string Status { get; set; } //lunch , intransit, active, offline

		public string Password { get; set; }

		public bool IsLoggedIn { get; set; }

		public bool IsActivated { get; set; }

		public DateTime DateCreated { get; set; } = DateTime.Now;

		public DateTime? LastModified { get; set; }


	}
}
