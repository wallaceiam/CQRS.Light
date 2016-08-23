using CQRS.Light.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Net;

namespace CQRS.Light.Repository.DocumentDB
{
    public class DocumentDBRepository<TAggregate> : IRepository<TAggregate>
        where TAggregate : IEntity
    {
        private readonly IDocumentClient client;
        private readonly string databaseName;
        private readonly string collectionName;

        public DocumentDBRepository(string endpointUri, string primaryKey, string databaseName, string collectionName)
        {
            this.client = new DocumentClient(new Uri(endpointUri), primaryKey);
            this.databaseName = databaseName;
            this.collectionName = collectionName;
            this.CreateDatabaseIfNotExists(this.databaseName).ConfigureAwait(true);
            this.CreateDocumentCollectionIfNotExists(this.databaseName, this.collectionName).ConfigureAwait(true);

        }

        public Task<long> CountAsync()
        {
            return Task.FromResult<long>(
                this.client.CreateDocumentQuery<TAggregate>(UriFactory.CreateDocumentCollectionUri(this.databaseName, this.collectionName))
                .Count<TAggregate>()
                );
        }

        public async Task DeleteAllAsync()
        {
            await this.client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(this.databaseName, this.collectionName));
            await this.CreateDocumentCollectionIfNotExists(this.databaseName, this.collectionName);
        }

        public async Task DeleteAsync(TAggregate item)
        {
            await DeleteAsync(item.Id);
        }

        public async Task DeleteAsync(Guid id)
        {
            await this.client.DeleteDocumentAsync(
                UriFactory.CreateDocumentUri(this.databaseName, this.collectionName, id.ToString()));
        }

        public Task<IEnumerable<TAggregate>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<TAggregate>>(
                this.client.CreateDocumentQuery<TAggregate>(UriFactory.CreateDocumentCollectionUri(this.databaseName, this.collectionName))
            );
        }

        public Task<IQueryable<TAggregate>> GetAsync()
        {
            return Task.FromResult<IQueryable<TAggregate>>(
                this.client.CreateDocumentQuery<TAggregate>(UriFactory.CreateDocumentCollectionUri(this.databaseName, this.collectionName))
                .AsQueryable<TAggregate>()
            );
        }

        public Task<TAggregate> GetByIdAsync(Guid id)
        {
            return Task.FromResult<TAggregate>(
                this.client.CreateDocumentQuery<TAggregate>(UriFactory.CreateDocumentCollectionUri(this.databaseName, this.collectionName))
                .Where(x => x.Id == id).FirstOrDefault()
                );
        }

        public async Task SaveAllAsync(IEnumerable<TAggregate> items)
        {
            foreach (var item in items)
                await SaveAsync(item);
        }

        public async Task SaveAsync(TAggregate item)
        {
            await this.client.UpsertDocumentAsync(UriFactory.CreateDocumentUri(this.databaseName, this.collectionName, item.Id.ToString()),
                item);
        }

        // ADD THIS PART TO YOUR CODE
        private async Task CreateDatabaseIfNotExists(string databaseName)
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                await this.client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.client.CreateDatabaseAsync(new Database { Id = databaseName });
                }
                else
                {
                    throw;
                }
            }
        }

        // ADD THIS PART TO YOUR CODE
        private async Task CreateDocumentCollectionIfNotExists(string databaseName, string collectionName)
        {
            try
            {
                await this.client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    // Configure collections for maximum query flexibility including string range queries.
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // Here we create a collection with 400 RU/s.
                    await this.client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        collectionInfo,
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
