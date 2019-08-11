using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.CosmosDB.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using phynixnetapi.Data;
using phynixnetapi.Helpers;
using phynixnetapi.Model;
using phynixnetapi.Model.TableModels;

namespace phynixnetapi.DriverFunc
{
	public static class CreateDriver
	{
		[FunctionName("CreateDriver")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req)
		{
			Driver user = await req.Content.ReadAsAsync<Driver>();

			await DriverRepository<Driver>.Initialize();

			if (user == null)
			{
				req.CreateResponse(HttpStatusCode.OK, "User cannot be null or empty");
			}

			if (string.IsNullOrEmpty(user.Email))
			{
				req.CreateResponse(HttpStatusCode.OK, "An email address is needed for this request");
			}

			try
			{
				var udb = DriverRepository<Driver>.GetItems($"Select * from DriverData u where u.Email = '{user.Email}'");

				if (udb != null && udb.Count() > 0)
				{
					return req.CreateResponse(HttpStatusCode.OK, "Driver already exist, please login or activate account to continue");
				}
			}
			catch (Exception ex)
			{
				return req.CreateResponse(HttpStatusCode.OK, ex);
			}



			if (string.IsNullOrEmpty(user.Password))
			{
				req.CreateResponse(HttpStatusCode.OK, "A Password is needed for this request");
			}
			user.Password = user.Password.EncodeString();

			user.LastModified = DateTime.Now;

			user.IsActivated = false;
			try
			{
				dynamic u = await DriverRepository<Driver>.CreateItemAsync(user);

				//Rider rider = u as Rider;

				return req.CreateResponse(HttpStatusCode.OK, "Your account has been created. Admin will contact via the number provided to complete the activation process.");
			}
			catch (Exception ex)
			{
				return req.CreateResponse(HttpStatusCode.OK, "An error has occured");
			}
		}

        [FunctionName("GetAllDrivers")]
        public static async Task<HttpResponseMessage> GetADrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req)
        {
            try
            {
                //User user = await req.Content.ReadAsAsync<User>();

                await DriverRepository<Driver>.Initialize();

                var drivers = await DriverRepository<Driver>.GetItemsAsync();

                if (drivers == null || drivers.Count() <= 0)
                {
                    return req.CreateResponse(HttpStatusCode.NoContent, "No Avilable Drivrs In The System.");
                }

                return req.CreateResponse(HttpStatusCode.OK, drivers);
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [FunctionName("GetConnectedDriver")]
        public static async Task<HttpResponseMessage> GetConDrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req)
        {
            try
            {
                // User user = await req.Content.ReadAsAsync<Driver>();

                await DriverRepository<Driver>.Initialize();

                var drivers = DriverRepository<Driver>.GetItems("Select * from DriverData d where d.Status <> 'busy' or d.Status <> 'offline' ");

                if (drivers == null || drivers.Count() <= 0)
                {
                    return req.CreateResponse(HttpStatusCode.NoContent, "No Avilable Drivrs In The System.");
                }

                return req.CreateResponse(HttpStatusCode.OK, drivers);
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [FunctionName("GetInactiveDrivers")]
        public static async Task<HttpResponseMessage> GetInactiveDrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequestMessage req)
        {
            try
            {
                // User user = await req.Content.ReadAsAsync<Driver>();

                await DriverRepository<Driver>.Initialize();

                var drivers = DriverRepository<Driver>.GetItems("Select * from DriverData d where d.IsActivated = false");

                if (drivers == null || drivers.Count() <= 0)
                {
                    return req.CreateResponse(HttpStatusCode.NoContent, "No Avilable Drivrs In The System.");
                }

                return req.CreateResponse(HttpStatusCode.OK, drivers);
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [FunctionName("GetDriverById")]
        public static async Task<IActionResult> GetDriverById([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req)
        {
            try
            {
                string id = req.Query["id"];

                await DriverRepository<Driver>.Initialize();

                var drivers = await DriverRepository<Driver>.GetItemAsync(id);

                if (drivers == null)
                {
                    return (ActionResult)new OkObjectResult("No Avilable Drivrs In The System.");
                }

                return (ActionResult)new OkObjectResult(drivers);
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult(ex);
            }
        }

        [FunctionName("GetDriversOnTrip")]
        public static async Task<IActionResult> GetDriversOnTrip([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req)
        {
            try
            {

                await DriverRepository<Driver>.Initialize();

                var drivers = DriverRepository<Driver>.GetItems("Select * from DriverData d where d.Status = 'intransit'");

                if (drivers == null || drivers.Count() <= 0)
                {
                    return (ActionResult)new OkObjectResult("No Avilable Drivers In The System.");
                }

                return (ActionResult)new OkObjectResult(drivers);
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult(ex);
            }
        }

        [FunctionName("ActivateDriver")]
        public static async Task<IActionResult> ActivateDriver([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req)
        {
            try
            {

                await DriverRepository<Driver>.Initialize();

                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic dr = JsonConvert.DeserializeObject(requestBody);

                

                if (dr == null)
                {
                    return (ActionResult)new OkObjectResult("Driver Cannot Be Null");
                }

                var updDr = await DriverRepository<Driver>.UpdateItemAsync(dr.id,dr);

                if(updDr != null)
                {
                    return (ActionResult)new OkObjectResult(updDr);
                }

                return (ActionResult)new OkObjectResult(null);
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult(ex);
            }
        }


        [FunctionName("CollectDriverInfo")]
        public static async Task<IActionResult> CollectDriverInfo([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req)
        {
            //await DriverRepository<Driver>.Initialize();

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverLocation dl = JsonConvert.DeserializeObject<DriverLocation>(requestBody);

                if (dl == null)
                {
                    return (ActionResult)new OkObjectResult("Driver Cannot be Null");
                }

                string tableName = "DriverLocation";

                if (string.IsNullOrEmpty(dl.DriverId))
                {
                    return (ActionResult)new OkObjectResult("A DriverId id needed to process this request.");
                }

                dl.RowKey = dl.DriverId;

                dl.PartitionKey = "location";

                
                CloudStorageAccount storageAccount= CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=jayappstorage;AccountKey=7W2/D8mqXslm8RXIfNSAv3lT0AtxvIbwFz2At+HBlFEyduECnb3Le3NVaHlR4LrP1luhvkbDtk59Lq8qrVXLbQ==;BlobEndpoint=https://jayappstorage.blob.core.windows.net/;TableEndpoint=https://jayappstorage.table.core.windows.net/;QueueEndpoint=https://jayappstorage.queue.core.windows.net/;FileEndpoint=https://jayappstorage.file.core.windows.net/");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference(tableName);

                await table.CreateIfNotExistsAsync();

                TableOperation insertOperation = TableOperation.InsertOrReplace(dl);

                var data = await table.ExecuteAsync(insertOperation);

                return (ActionResult)new OkObjectResult(data.Result);
            }catch(Exception ex){
                return (ActionResult)new OkObjectResult(ex);
            }
        }
    }
}

