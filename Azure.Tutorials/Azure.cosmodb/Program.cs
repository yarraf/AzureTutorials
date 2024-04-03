using Azure.cosmodb;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Microsoft.Azure.Documents;

const string connectionString = "";

Console.WriteLine($"[Connection string]:\t{connectionString}");


CosmosSerializationOptions serializerOptions = new()
{
    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
};

using CosmosClient client = new CosmosClientBuilder(connectionString)
    .WithSerializerOptions(serializerOptions)
    .Build();

Console.WriteLine("[Client ready]");


Console.WriteLine("[starting create database]");


Database cosmosdb = await client.CreateDatabaseIfNotExistsAsync("cosmosdbWork");

Console.WriteLine($"[Database created]: \t{cosmosdb.Id}");


///Create container
ContainerProperties properties = new ContainerProperties(
                                                            id: "Products",
                                                            partitionKeyPath: "/categoryId"
                                                        );


//autoscaling
var throughput = ThroughputProperties.CreateAutoscaleThroughput(autoscaleMaxThroughput: 1000);

Container container = await cosmosdb.CreateContainerIfNotExistsAsync(
                                                                containerProperties: properties,
                                                                throughputProperties: throughput
                                                                );
Console.WriteLine($"[Container created]: \t{container.Id}");

//create new category instance

Category goggles = new Category("ef7fa0f1-0e9d-4435-aaaf-a778179a94ad", "gear-snow-goggles");

Microsoft.Azure.Cosmos.PartitionKey goggleskey = new("gear-snow-goggles");

Category result = await container.UpsertItemAsync(goggles, goggleskey);

Console.WriteLine($"[New item created]: \t{result.Id}\t{result.Type}");

//new category

Category helmets = new Category("91f79374-8611-4505-9c28-3bbbf1aa7df7", "gear-climb-helmets");

Microsoft.Azure.Cosmos.PartitionKey helmetskey = new("gear-climb-helmets");

ItemResponse<Category> response = await container.UpsertItemAsync(helmets, helmetskey);

Console.WriteLine($"[New item created]: \t{response.Resource.Id}\t(Type: {response.Resource.Type})\t(RUs: {response.RequestCharge})");


//using transaction to insert in first new category tent related with products

Category tents = new(
    Id: "5df21ec5-813c-423e-9ee9-1a2aaead0be4",
    CategoryId: "gear-camp-tents"
);

Product cirroa = new(
    Id: "e8dddee4-9f43-4d15-9b08-0d7f36adcac8",
    CategoryId: "gear-camp-tents"
)
{
    Name = "Cirroa Tent",
    Price = 490.00m,
    Archived = false,
    Quantity = 15
};

Product kuloar = new(
    Id: "e6f87b8d-8cd7-4ade-a005-14d3e2fbd1aa",
    CategoryId: "gear-camp-tents"
)
{
    Name = "Kuloar Tent",
    Price = 530.00m,
    Archived = false,
    Quantity = 8
};

Product mammatin = new(
    Id: "f7653468-c4b8-47c9-97ff-451ee55f4fd5",
    CategoryId: "gear-camp-tents"
)
{
    Name = "Mammatin Tent",
    Price = 0.00m,
    Archived = true,
    Quantity = 0
};

Product nimbolo = new(
    Id: "6e3b7275-57d4-4418-914d-14d1baca0979",
    CategoryId: "gear-camp-tents"
)
{
    Name = "Nimbolo Tent",
    Price = 330.00m,
    Archived = false,
    Quantity = 35
};


Microsoft.Azure.Cosmos.PartitionKey tentsKey = new("gear-camp-tents");


TransactionalBatch batch = container.CreateTransactionalBatch(tentsKey)
    .UpsertItem<Category>(tents)
    .UpsertItem<Product>(cirroa)
    .UpsertItem<Product>(kuloar)
    .UpsertItem<Product>(mammatin)
    .UpsertItem<Product>(nimbolo);


Console.WriteLine("[Batch started]");

using TransactionalBatchResponse batchResponse = await batch.ExecuteAsync();

for (int i = 0; i < batchResponse.Count; i++)
{
    TransactionalBatchOperationResult<Item> batchResult = batchResponse.GetOperationResultAtIndex<Item>(i);
    Console.WriteLine($"[New item created]:\t{batchResult.Resource.Id}\t(Type: {batchResult.Resource.Type})");
}

Console.WriteLine($"[Batch completed]:\t(RUs: {batchResponse.RequestCharge})");