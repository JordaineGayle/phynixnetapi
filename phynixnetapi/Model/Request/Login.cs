using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phynixnetapi.Model.Request
{
	public class Login
	{
		public string Username { get; set; }

		public string Password { get; set; }

		public string Token { get; set; }
	}
}
