using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using phynixnetapi.Data;
using phynixnetapi.Helpers;
using phynixnetapi.Model;

namespace phynixapi.UserFunc
{
    public static class CreateAdmin
    {

        [FunctionName("CreateAdmin")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
			User user = await req.Content.ReadAsAsync<User>();

			await UserRepository<User>.Initialize();


			if (user == null)
			{
				return req.CreateResponse(HttpStatusCode.OK,"User Cannot Be Null");
			}

			if (string.IsNullOrEmpty(user.Email))
			{
				return req.CreateResponse(HttpStatusCode.OK, "An email address is needed for this request");
			}

			var udb = UserRepository<User>.GetItems($"Select * from UserData u where u.Email = '{user.Email}'");

			if(udb != null && udb.Count() > 0)
			{
				return req.CreateResponse(HttpStatusCode.OK, "User already exist, please login or activate account to continue");
			}

			if (string.IsNullOrEmpty(user.Password))
			{
				return req.CreateResponse(HttpStatusCode.OK, "A Password is needed for this request");
			}


			user.Password = user.Password.EncodeString();

			user.LastModified = DateTime.Now;

			user.IsActivated = false;

			try
			{

				dynamic u = await UserRepository<User>.CreateItemAsync(user);

				//Rider rider = u as Rider;

				return req.CreateResponse(HttpStatusCode.OK, "Your account has been created. Admin will contact via the number provided to complete the activation process.");
			}
			catch (Exception ex)
			{
				return req.CreateResponse(HttpStatusCode.OK, "An error has occured");
			}
		}

        //[FunctionName("GetAllDrivers")]
        //public static async Task<HttpResponseMessage> GetADrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req)
        //{
        //    try
        //    {
        //        //User user = await req.Content.ReadAsAsync<User>();

        //        await DriverRepository<Driver>.Initialize();

        //        var drivers = await DriverRepository<Driver>.GetItemsAsync();

        //        if (drivers == null || drivers.Count() <= 0)
        //        {
        //            return req.CreateResponse(HttpStatusCode.NoContent, "No Avilable Drivrs In The System.");
        //        }

        //        return req.CreateResponse(HttpStatusCode.OK, drivers);
        //    }
        //    catch (Exception ex)
        //    {
        //        return req.CreateResponse(HttpStatusCode.InternalServerError,ex);
        //    }
        //}

        //[FunctionName("GetConnectedDriver")]
        //public static async Task<HttpResponseMessage> GetConDrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req)
        //{
        //    try
        //    {
        //       // User user = await req.Content.ReadAsAsync<Driver>();

        //        await DriverRepository<Driver>.Initialize();

        //        var drivers = DriverRepository<Driver>.GetItems("Select * from DriverData d where d.Status <> 'busy' or d.Status <> 'offline' ");

        //        if (drivers == null || drivers.Count() <= 0)
        //        {
        //            return req.CreateResponse(HttpStatusCode.NoContent, "No Avilable Drivrs In The System.");
        //        }

        //        return req.CreateResponse(HttpStatusCode.OK, drivers);
        //    }
        //    catch (Exception ex)
        //    {
        //        return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}


        //[FunctionName("GetInactiveDrivers")]
        //public static async Task<HttpResponseMessage> GetInactiveDrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req)
        //{
        //    try
        //    {
        //        // User user = await req.Content.ReadAsAsync<Driver>();

        //        await DriverRepository<Driver>.Initialize();

        //        var drivers = DriverRepository<Driver>.GetItems("Select * from DriverData d where d.IsActivated = false");

        //        if (drivers == null || drivers.Count() <= 0)
        //        {
        //            return req.CreateResponse(HttpStatusCode.NoContent, "No Avilable Drivrs In The System.");
        //        }

        //        return req.CreateResponse(HttpStatusCode.OK, drivers);
        //    }
        //    catch (Exception ex)
        //    {
        //        return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
        //    }
        //}


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

        //[FunctionName("GetDriversOnTrip")]
        //public static async Task<IActionResult> GetDriversOnTrip([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req)
        //{
        //    try
        //    {

        //        await DriverRepository<Driver>.Initialize();

        //        var drivers = DriverRepository<Driver>.GetItems("Select * from DriverData d where d.Status = 'intransit'");

        //        if (drivers == null || drivers.Count() <= 0)
        //        {
        //            return (ActionResult)new OkObjectResult("No Avilable Drivers In The System.");
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
