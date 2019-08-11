using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using phynixnetapi.Data;
using phynixnetapi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace phynixnetapi
{
    public class TripFunc
    {
        [FunctionName("CreateTrip")]
        public static async Task<IActionResult> CreateTrip([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req)
        {

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            Trip trip = JsonConvert.DeserializeObject<Trip>(requestBody);

            await TripRepository<Trip>.Initialize();
            if (trip == null)
            {
                return (ActionResult)new OkObjectResult("Trip cannot be null or empty");
            }
            

            try
            {
                dynamic u = await TripRepository<Trip>.CreateItemAsync(trip);

                return (ActionResult)new OkObjectResult("Trip Created Successfully");
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult("An error has occured");
            }

        }

        [FunctionName("AcceptRider")]
        public static async Task<IActionResult> AcceptRider([HttpTrigger("post", Route = null)]HttpRequest req,
            [SignalR(HubName = "driver", ConnectionStringSetting = "AzureSignalRConnection")]IAsyncCollector<SignalRMessage> message)
        {
            try
            {
                await message.AddAsync(
                new SignalRMessage
                {
                    Target = "cancel",
                    Arguments = new[] { "cancel all other drivers" }
                });

                await TripRepository<Trip>.Initialize();

                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                //return (ActionResult)new OkObjectResult(data);

                Trip newTrip = new Trip()
                {
                    AddtionalInfo = "",
                    DateOfTrip = DateTime.Now,
                    IsPayed = false,
                    Origin = new Location() {Latitude = (double)data.Origin.Latitude,Longitude = (double)data.Origin.Longitude },
                    Destination = new Location() { Latitude = (double)data.Destination.Latitude, Longitude = (double)data.Destination.Longitude },
                    UserId = data.UserId,
                    DriverId = data.DriverId,
                    TotalTripPrice = data.Total
                };


                dynamic u = await TripRepository<Trip>.CreateItemAsync(newTrip);

                return (ActionResult)new OkObjectResult("Trip Created Successfully");
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult(ex);
            }

        }

        [FunctionName("GetTripsByDriver")]
        public static async Task<IActionResult> GetTripsByDriverAsync([HttpTrigger("get", Route =null)]HttpRequest req)
        {
            await TripRepository<Trip>.Initialize();

            string driverId = req.Query["driverid"];

            var trips = TripRepository<Trip>.GetItems($"Select * from TripData t where t.DriveId = '{driverId}' ");

            if(trips == null || trips.Count() <= 0)
            {
                return (ActionResult)new OkObjectResult("No Trips For Requested Driver");
            }

            return (ActionResult)new OkObjectResult(trips);
        }

        [FunctionName("GetTrips")]
        public static async Task<IActionResult> GetTrips([HttpTrigger("get", Route = null)]HttpRequest req)
        {
            try
            {
                //User user = await req.Content.ReadAsAsync<User>();

                await TripRepository<Trip>.Initialize();

                var trips = await TripRepository<Trip>.GetItemsAsync();

                if (trips == null || trips.Count() <= 0)
                {
                    return (ActionResult)new OkObjectResult("No Avilable Trips In The System.");
                }

                return (ActionResult)new OkObjectResult(trips);
            }
            catch (Exception ex)
            {
                return (ActionResult)new OkObjectResult(ex);
            }
        }

        [FunctionName("GetTripsBasedOnRating")]
        public static async Task<IActionResult> GetTripsBasedOnRating([HttpTrigger("get", Route = null)]HttpRequest req)
        {
            await TripRepository<Trip>.Initialize();

            string rating = req.Query["rating"];

            var trips = TripRepository<Trip>.GetItems($"Select * from TripData t where t.Rating = '{rating}' ");

            if (trips == null || trips.Count() <= 0)
            {
                return (ActionResult)new OkObjectResult("No Trips For Requested Driver");
            }

            return (ActionResult)new OkObjectResult(trips);
        }

        [FunctionName("GetTripByDate")]
        public static async Task<IActionResult> GetTripByDate([HttpTrigger("get", Route = null)]HttpRequest req)
        {
            await TripRepository<Trip>.Initialize();

            string date = req.Query["date"];

            var trips = TripRepository<Trip>.GetItems($"Select * from TripData t where t.DateCreated = '{date}' ");

            if (trips == null || trips.Count() <= 0)
            {
                return (ActionResult)new OkObjectResult("No Trips For Request");
            }

            return (ActionResult)new OkObjectResult(trips);
        }

        [FunctionName("GetTripUserId")]
        public static async Task<IActionResult> GetTripUserId([HttpTrigger("get", Route = null)]HttpRequest req)
        {
            string userid = req.Query["userid"];

            var trips = TripRepository<Trip>.GetItems($"Select * from TripData t where t.UserId = '{userid}' ");

            if (trips == null || trips.Count() <= 0)
            {
                return (ActionResult)new OkObjectResult("No Trips For Request");
            }

            return (ActionResult)new OkObjectResult(trips);
        }

        [FunctionName("GetTripBasedOnRate")]
        public static async Task<IActionResult> GetTripBasedOnRate([HttpTrigger("get", Route = null)]HttpRequest req)
        {
            await TripRepository<Trip>.Initialize();

            string rate = req.Query["rate"];

            var trips = TripRepository<Trip>.GetItems($"Select * from TripData t where t.UserId = '{rate}' ");

            if (trips == null || trips.Count() <= 0)
            {
                return (ActionResult)new OkObjectResult("No Trips For Request");
            }

            return (ActionResult)new OkObjectResult(trips);
        }

        [FunctionName("AddTripRating")]
        public static async Task<IActionResult> AddTripRating([HttpTrigger("post", Route = null)]HttpRequest req)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            if (string.IsNullOrEmpty((string)data.id))
            {
                return (ActionResult)new OkObjectResult("An Id is needed for this request");
            }

            await TripRepository<Trip>.Initialize();

            Trip trip = await TripRepository<Trip>.GetItemAsync(data.id);

            if(trip != null)
            {
                return (ActionResult)new OkObjectResult("No trip available by that id");
            }

            trip.Comment = data.Comment;

            trip.Rating = data.Rating;

            var updateTrip = await TripRepository<Trip>.UpdateItemAsync(trip.id,trip);

            if(updateTrip == null)
            {
                return (ActionResult)new OkObjectResult("Failed to update trip");
            }

            return (ActionResult)new OkObjectResult("Thank you for your service");

        }
    }
}
