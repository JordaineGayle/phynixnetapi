using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using phynixnetapi.Data;
using phynixnetapi.Helpers;
using phynixnetapi.Model;

namespace phynixnetapi.RiderFunc
{
    public static class CreateRider
    {
        [FunctionName("CreateRider")]
        public static async Task<HttpResponseMessage> RiderCreate([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req)
        {
			Rider user = await req.Content.ReadAsAsync<Rider>();
			await RiderRepository<Rider>.Initialize();
			if (user == null)
			{
				req.CreateResponse(HttpStatusCode.OK, "User cannot be null or empty");
			}

			if (string.IsNullOrEmpty(user.Email))
			{
				req.CreateResponse(HttpStatusCode.OK, "An email address is needed for this request");
			}

			var udb = RiderRepository<Rider>.GetItems($"Select * from RiderData u where u.Email = '{user.Email}'");

			if (udb != null && udb.Count() > 0)
			{
				return req.CreateResponse(HttpStatusCode.OK, "Driver already exist, please login or activate account to continue");
			}

			if (string.IsNullOrEmpty(user.Password))
			{
				req.CreateResponse(HttpStatusCode.OK, "A Password is needed for this request");
			}

			user.Password = user.Password.EncodeString();

			user.LastModified = DateTime.Now;

			user.IsActivated = true;

			try
			{
				dynamic u = await RiderRepository<Rider>.CreateItemAsync(user);

				return req.CreateResponse(HttpStatusCode.OK, "Your account has been created. Admin will contact via the number provided to complete the activation process.");
			}
			catch (Exception ex)
			{
				return req.CreateResponse(HttpStatusCode.OK, "An error has occured");
			}
			
		}

        [FunctionName("GetAllRiders")]
        public static async Task<HttpResponseMessage> GetAllRiders([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req)
        {
            try
            {
                //User user = await req.Content.ReadAsAsync<User>();

                await RiderRepository<Rider>.Initialize();

                var riders = await RiderRepository<Rider>.GetItemsAsync();

                if (riders == null || riders.Count() <= 0)
                {
                    return req.CreateResponse(HttpStatusCode.NoContent, "No Avilable Riders In The System.");
                }

                return req.CreateResponse(HttpStatusCode.OK, riders);
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [FunctionName("GetConnectedRiders")]
        public static async Task<HttpResponseMessage> GetConnectedRiders([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req)
        {
            try
            {
                // User user = await req.Content.ReadAsAsync<Driver>();

                await RiderRepository<Rider>.Initialize();

                var drivers = RiderRepository<Rider>.GetItems("Select * from RiderData d where d.Status <> 'busy' or d.Status <> 'offline' ");

                if (drivers == null || drivers.Count() <= 0)
                {
                    return req.CreateResponse(HttpStatusCode.NoContent, "No Avilable Riders In The System.");
                }

                return req.CreateResponse(HttpStatusCode.OK, drivers);
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
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
