using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using phynixnetapi.Data;
using phynixnetapi.Helpers;
using phynixnetapi.Model;

namespace phynixnetapi.RiderFunc
{
    public static class CreateRider
    {
        [FunctionName("CreateRider")]
        public static async Task<IActionResult> RiderCreate([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req)
        {
           // req.IsValidToken();

            //Rider user = await req.Content.ReadAsAsync<Rider>();

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            Rider user = JsonConvert.DeserializeObject<Rider>(requestBody);

			await RiderRepository<Rider>.Initialize();
			if (user == null)
			{
				return (ActionResult)new OkObjectResult("User cannot be null or empty");
			}

			if (string.IsNullOrEmpty(user.Email))
			{
                return (ActionResult)new OkObjectResult("An email address is needed for this request");
			}

			var udb = RiderRepository<Rider>.GetItems($"Select * from RiderData u where u.Email = '{user.Email}'");

			if (udb != null && udb.Count() > 0)
			{
                return (ActionResult)new OkObjectResult("Rider already exist, please login or activate account to continue");
			}

			if (string.IsNullOrEmpty(user.Password))
			{
                return (ActionResult)new OkObjectResult("A Password is needed for this request");
			}

			user.Password = user.Password.EncodeString();

			user.LastModified = DateTime.Now;

			user.IsActivated = true;

			try
			{
				dynamic u = await RiderRepository<Rider>.CreateItemAsync(user);

                return (ActionResult)new OkObjectResult("Your account has been created. Admin will contact via the number provided to complete the activation process.");
			}
			catch (Exception ex)
			{
                return (ActionResult)new OkObjectResult("An error has occured");
			}
			
		}

        [FunctionName("GetAllRiders")]
        public static async Task<IActionResult> GetAllRiders([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req)
        {
            req.IsValidToken();
            try
            {
                //User user = await req.Content.ReadAsAsync<User>();

                await RiderRepository<Rider>.Initialize();

                var riders = await RiderRepository<Rider>.GetItemsAsync();

                if (riders == null || riders.Count() <= 0)
                {
                    return (ActionResult)new OkObjectResult("No Avilable Riders In The System.");
                }

                return (ActionResult)new OkObjectResult(riders);
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult(ex);
            }
        }

        [FunctionName("GetConnectedRiders")]
        public static async Task<IActionResult> GetConnectedRiders([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req)
        {
            req.IsValidToken();
            try
            {
                // User user = await req.Content.ReadAsAsync<Driver>();

                await RiderRepository<Rider>.Initialize();

                var drivers = RiderRepository<Rider>.GetItems("Select * from RiderData d where d.Status <> 'busy' or d.Status <> 'offline' ");

                if (drivers == null || drivers.Count() <= 0)
                {
                    return (ActionResult)new OkObjectResult("No Avilable Riders In The System.");
                }

                return (ActionResult)new OkObjectResult(drivers);
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult(ex);
            }
        }



        //[FunctionName("GetDriverById")]
        //public static async Task<IActionResult> GetInactiveDrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req)
        //{
        //    try
        //    {
        //        string id = req.Query["id"];

        //        await DriverRepository<Driver>.Initialize();

        //        var drivers = await DriverRepository<Driver>.GetItemAsync(id);

        //        if (drivers == null)
        //        {
        //            return (ActionResult)new OkObjectResult("No Avilable Drivrs In The System.");
        //        }

        //        return (ActionResult)new OkObjectResult(drivers);
        //    }
        //    catch (Exception ex)
        //    {
        //        return (ActionResult)new OkObjectResult(ex);
        //    }
        //}


    }
}
