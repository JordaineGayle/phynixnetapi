
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace phynixnetapi.Data
{
	public static class DriverRepository<T> where T : class
	{
		private static readonly string DatabaseId = Environment.GetEnvironmentVariable("PhynixDatabaseName");//ConfigurationManager.AppSettings["database"];
		private static readonly string CollectionId = Environment.GetEnvironmentVariable("Driver");

		private static DocumentClient client;

		public static async Task<T> GetItemAsync(string id)
		{
			try
			{
				Document document = await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), new RequestOptions { PartitionKey = new PartitionKey("/driver") });
				return (T)(dynamic)document;
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return null;
				}
				else
				{
					return null;
				}
			}
		}

		public static async Task<IEnumerable<T>> GetItemsAsync()
		{
			IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
				UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
				new FeedOptions { MaxItemCount = -1 })
				.AsDocumentQuery();

			List<T> results = new List<T>();
			while (query.HasMoreResults)
			{
				results.AddRange(await query.ExecuteNextAsync<T>());
			}

			return results;
		}

		public static IEnumerable<T> GetItems(string queryString)
		{
			IQueryable<T> query = client.CreateDocumentQuery<T>(
				UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
				queryString,
				new FeedOptions { MaxItemCount = -1, EnableScanInQuery = true, EnableCrossPartitionQuery = true });

			return query.ToList();
		}

		public static async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, int maxCount = -1)
		{
			try
			{
				IDocumentQuery<T> query = null;
				if (maxCount != -1)
				{
					query = client.CreateDocumentQuery<T>(
						UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId)//, new FeedOptions { MaxItemCount = maxCount }
						)
						.Where(predicate).Take(maxCount)
						.AsDocumentQuery();
				}
				else
				{
					query = client.CreateDocumentQuery<T>(
						UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId)//, new FeedOptions { MaxItemCount = maxCount }
						)
						.Where(predicate)
						.AsDocumentQuery();
				}


				List<T> results = new List<T>();
				while (query.HasMoreResults)
				{
					results.AddRange(await query.ExecuteNextAsync<T>());
				}

				return results;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static async Task<Document> CreateItemAsync(T item)
		{
			try
			{
				return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static async Task<Document> UpdateItemAsync(string id, T item)
		{
			try
			{
				return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static async Task<Document> UpsertItemAsync(T item)
		{
			try
			{
				return await client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item, new RequestOptions { PartitionKey = new PartitionKey("/driver") });
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static async Task<Document> DeleteItemAsync(string id)
		{
			try
			{
				return await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), new RequestOptions { PartitionKey = new PartitionKey("/driver") });
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static async Task Initialize()
		{
			client = new DocumentClient(new Uri(Environment.GetEnvironmentVariable("Endpoint")), Environment.GetEnvironmentVariable("AuthKey"));
			await CreateDatabaseIfNotExistsAsync();
			await CreateCollectionIfNotExistsAsync();
		}

		private static async Task CreateDatabaseIfNotExistsAsync()
		{
			try
			{
				await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					RequestOptions options = new RequestOptions
					{
						OfferThroughput = 6000
					};

					await client.CreateDatabaseAsync(new Database { Id = DatabaseId }, options);
				}
				else
				{
					throw;
				}
			}
		}

		private static async Task CreateCollectionIfNotExistsAsync()
		{
			try
			{
				await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), new RequestOptions { PartitionKey = new PartitionKey("/driver") });
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					DocumentCollection myCollection = new DocumentCollection();
					myCollection.Id = CollectionId;
					myCollection.PartitionKey.Paths.Add("/driver");

					await client.CreateDocumentCollectionAsync(
						UriFactory.CreateDatabaseUri(DatabaseId),
						myCollection,
						new RequestOptions { OfferThroughput = 400 });
				}
				else
				{
					throw;
				}
			}
		}
	}
}
