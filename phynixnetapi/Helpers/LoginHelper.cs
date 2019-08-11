
using phynixnetapi.Data;
using phynixnetapi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phynixnetapi.Helpers
{
	public static class LoginHelper
	{
		public static async Task<User> CreateUser(User user)
		{
			if(user == null)
			{
				return null;
			}

			if (string.IsNullOrEmpty(user.Email))
			{
				return (dynamic)"An email address is needed for this request";
			}

			if (string.IsNullOrEmpty(user.Password))
			{
				return (dynamic)"A Password is needed for this request";
			}

			try
			{
				dynamic u = await UserRepository<User>.CreateItemAsync(user);

				return u;
			}
			catch (Exception ex)
			{
				return null;
			}
		}
	}
}
