using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Tokens;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using phynixnetapi.Data;
using phynixnetapi.Helpers;
using phynixnetapi.Model;
using phynixnetapi.Model.Display;
using phynixnetapi.Model.TableModels;

namespace phynixapi.Authentication
{
    public static class Authentication
    {
        [FunctionName("Login")]
		public static async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequest req)
        {
			try
			{
				await DriverRepository<Driver>.Initialize();
				await RiderRepository<Rider>.Initialize();
				await UserRepository<User>.Initialize();

				string token = req.Query["token"];

				if (string.IsNullOrEmpty(token))
				{
					return (ActionResult)new OkObjectResult("Please set the token feild as a query parameter '?token='");
				}

				string decode = string.Empty;

				try
				{
					decode = token.DecodeString();
				}
				catch (Exception)
				{
					return (ActionResult)new OkObjectResult("Invalid base64 Token");
				}


				var parts = decode.Split(':');

				//return req.CreateResponse(HttpStatusCode.OK, parts);

				if (parts == null || parts.Count() <= 0)
				{
					return (ActionResult)new OkObjectResult("Token Is invalid");
				}

				var email = parts[0];

				var password = parts[1];

				var type = parts[2];

				DisplayUser ds = null;

				if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(type))
				{
					return (ActionResult)new OkObjectResult("Token failed");
				}

				password = password.EncodeString();

				if (type.ToLower().Equals("user"))
				{
					var u = UserRepository<User>.GetItems($"Select * from UserData u where u.Email = '{email}' and u.Password = '{password}' and u.IsActivated = true");

					if (u == null || u.Count() <= 0)
					{
						return (ActionResult)new OkObjectResult("Failed To locate a user with the provide credentials or user is nnot yet activated");
					}

					var fs = u.FirstOrDefault();

					ds = new DisplayUser()
					{
						Firstname = fs.Firstname,
						Lastname = fs.Lastname,
						id = fs.id,
						DateCreated = fs.DateCreated,
						Email = fs.Email,
						//IsLoggedIn = fs.IsLoggedIn,
						IsActivated = fs.IsActivated,
						LastModified = fs.LastModified??null,
						Phone = fs.Phone
						//Status = fs.Status
					};

                    var updateUser = await UserRepository<User>.GetItemAsync(ds.id);

                    if(updateUser != null)
                    {
                        updateUser.IsLoggedIn = true;

                        await UserRepository<User>.UpdateItemAsync(updateUser.id, updateUser);


                    }
					//return req.CreateResponse(HttpStatusCode.OK, ds);
				}
				else if (type.ToLower().Equals("rider"))
				{
					var u = RiderRepository<Rider>.GetItems($"Select * from RiderData u where u.Email = '{email}' and u.Password = '{password}' and u.IsActivated = true ");

					if (u == null || u.Count() <= 0)
					{
						return (ActionResult)new OkObjectResult("Failed To locate a user with the provide credentials or user is nnot yet activated");
					}

					var fs = u.FirstOrDefault();

					ds = new DisplayUser()
					{
						Firstname = fs.Firstname,
						Lastname = fs.Lastname,
						id = fs.id,
						DateCreated = fs.DateCreated,
						Email = fs.Email,
						IsLoggedIn = fs.IsLoggedIn,
						IsActivated = fs.IsActivated,
						LastModified = fs.LastModified ?? null,
						Phone = fs.Phone,
						Status = fs.Status
					};
				}
				else if (type.ToLower().Equals("driver"))
				{
					var u = DriverRepository<Driver>.GetItems($"Select * from DriverData u where u.Email = '{email}' and u.Password = '{password}' and u.IsActivated = true");

					if (u == null || u.Count() <= 0)
					{
						return (ActionResult)new OkObjectResult("Failed To locate a user with the provide credentials or user is nnot yet activated");
					}

					var fs = u.FirstOrDefault();

					ds = new DisplayUser()
					{
						Firstname = fs.Firstname,
						Lastname = fs.Lastname,
						id = fs.id,
						DateCreated = fs.DateCreated,
						Email = fs.Email,
						IsLoggedIn = fs.IsLoggedIn,
						IsActivated = fs.IsActivated,
						LastModified = fs.LastModified ?? null,
						Phone = fs.Phone,
						Status = fs.Status
					};
				}
				else
				{
					return (ActionResult)new OkObjectResult("invalid login type");
				}

				if (ds == null)
				{
					return (ActionResult)new OkObjectResult("Login failed no user located, sorry.");
				}


				try
				{


					string key = Environment.GetEnvironmentVariable("Secret");

					var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

					var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

					var header = new JwtHeader(credentials);

					var claim = new[]
					{
					new Claim(ClaimTypes.NameIdentifier,ds.id),
					new Claim(ClaimTypes.Name,email)
				};

					var tokens = new JwtSecurityToken(
						issuer: "Phynix",
						audience: "Phynix Inc",
						claims: claim,
						notBefore: DateTime.Now.AddMinutes(1),
						expires: DateTime.Now.AddDays(2),
						signingCredentials: credentials);

					var handler = new JwtSecurityTokenHandler();

					var tokenString = handler.WriteToken(tokens);

                    

					return (ActionResult)new OkObjectResult(new { Result = new { data = ds, token = tokenString } });
				}
				catch (Exception ex)
				{
					return (ActionResult)new OkObjectResult(ex);
				}
			}
			catch(Exception ex)
			{
				return (ActionResult)new OkObjectResult(ex);
			}
		}

        [FunctionName("Authenticate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "main",ConnectionStringSetting = "AzureSignalRConnection")] SignalRConnectionInfo connectionInfo)
        {
            req.IsValidToken();

            return connectionInfo;
        }

        [FunctionName("RiderAuth")]
        public static SignalRConnectionInfo RiderAuth(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "rider", ConnectionStringSetting = "AzureSignalRConnection")] SignalRConnectionInfo connectionInfo)
        {
            req.IsValidToken();
            return connectionInfo;
        }

        [FunctionName("DriverAuth")]
        public static SignalRConnectionInfo DriverAuth(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "driver", ConnectionStringSetting = "AzureSignalRConnection")] SignalRConnectionInfo connectionInfo)
        {
            req.IsValidToken();
            return connectionInfo;
        }

        [FunctionName("BroadcastMyAddress")]
        public static async Task<dynamic> BroadcastMyAddress(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [SignalR(HubName = "driver", ConnectionStringSetting = "AzureSignalRConnection")]IAsyncCollector<SignalRMessage> message)
        {
            req.IsValidToken();
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                dynamic dr = JsonConvert.DeserializeObject(requestBody);

                string tableName = "DriverLocation";

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=jayappstorage;AccountKey=7W2/D8mqXslm8RXIfNSAv3lT0AtxvIbwFz2At+HBlFEyduECnb3Le3NVaHlR4LrP1luhvkbDtk59Lq8qrVXLbQ==;BlobEndpoint=https://jayappstorage.blob.core.windows.net/;TableEndpoint=https://jayappstorage.table.core.windows.net/;QueueEndpoint=https://jayappstorage.queue.core.windows.net/;FileEndpoint=https://jayappstorage.file.core.windows.net/");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable table = tableClient.GetTableReference(tableName);

                await table.CreateIfNotExistsAsync();

                //var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Location");
                //var query = new TableQuery<DriverLocation>().Where(condition);
                //var lst = await table.ExecuteQuerySegmentedAsync(query,null);




                await message.AddAsync(
                new SignalRMessage
                {
                    Target = "newrider",
                    Arguments = new[] { dr }
                });

                return dr;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

    }
}
