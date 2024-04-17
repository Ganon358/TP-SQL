namespace ReplicaSet.Sevenet;

using System.CommandLine;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

partial class Program {

	private static readonly string ConnectionString = "mongodb://localhost:27017";
	private static readonly string TestCollectionName = "test_users";

    static async Task<int> Main(string[] args) {
		EncodingProvider provider = CodePagesEncodingProvider.Instance;
		Encoding.RegisterProvider(provider);


        Option<string> fileName = new("--file-name", getDefaultValue: () => "data.json", description: "The file name of the generated data file");
        Option<int> dataCount = new("--data-count", getDefaultValue: () => 100, description: "The number of generated data");

        RootCommand rootCommand = new();

		Command generateDataCommand = new("generate") {
			fileName,
			dataCount
		};
		Command crudCommand = new("test");

		rootCommand.Add(generateDataCommand);
		rootCommand.Add(crudCommand);


        generateDataCommand.SetHandler(GenerateData, fileName, dataCount);
        crudCommand.SetHandler(TestCRUD);

        return await rootCommand.InvokeAsync(args);
    }

    private static void TestCRUD() {
		MongoClient client = new(ConnectionString);
		IMongoDatabase db = client.GetDatabase("replicaset");

		Console.WriteLine($"Suppression de tous les Utilisateurs");
		db.DropCollection(TestCollectionName);
		IMongoCollection<UserData> collection = db.GetCollection<UserData>(TestCollectionName);

		UserData user1 = GenerateUser();
		UserData user2 = GenerateUser();
		UserData user3 = GenerateUser();

		Console.WriteLine($"Insertion d'Utilisateurs : {user1}\n");
		collection.InsertOne(user1);

		Console.WriteLine($"Insertion d'Utilisateurs : {user2}\n");
		collection.InsertOne(user2);

		Console.WriteLine($"Insertion d'Utilisateurs : {user3}\n");
		collection.InsertOne(user3);


		Console.WriteLine($"Suppression de l'Utilisateur {user1.Name}");
        DeleteResult deleteResult = collection.DeleteOne(Builders<UserData>.Filter.Eq(user => user.Id, user1.Id));
		Console.WriteLine(deleteResult.IsAcknowledged + "\n");


		Console.WriteLine($"Modification de l'Utilisateur {user2.Name} (Ajout de 5 ans d'âge)");
		UpdateResult updateRes = collection.UpdateOne(
			Builders<UserData>.Filter.Eq(user => user.Id, user2.Id),
			Builders<UserData>.Update.Set(user => user.Age, user2.Age + 5)
		);
		Console.WriteLine(updateRes.IsAcknowledged + "\n");

		Console.WriteLine($"Récupération de l'Utilisateur {user2.Name}");
		Console.WriteLine(collection.Find(Builders<UserData>.Filter.Eq(user => user.Id, user2.Id)).First() + "\n");

		Console.WriteLine($"Récupération de tous les Utilisateurs");
		Console.WriteLine(string.Join("\n", collection.Find(Builders<UserData>.Filter.Empty).ToList()));
    }

    private static void GenerateData(string fileName, int dataCount) {
		List<UserData> data = [];
		for (int i = 0; i < dataCount; i++) {
			data.Add(GenerateUser());
		}

		string serialized = JsonSerializer.Serialize(data.ToArray());

		using FileStream? sw = File.Create(fileName);
		sw.Write( Encoding.UTF8.GetBytes(serialized) );
    }

	private static UserData GenerateUser() {
		DateTime birthDate = Faker.Date.Birthday();

		return new() {
			Name = Faker.Name.FullName(),
			Age = (byte)GetAge(birthDate),
			CreatedAt = Faker.Date.Between(birthDate, DateTime.Today),
			Email = Faker.Internet.Email()
		};
	}
	private static int GetAge(DateTime birthDate) {
		DateTime today = DateTime.Today;

		int age = today.Year - birthDate.Year;
		if (birthDate.Date > today.AddYears(-age)) {
			age--;
		}

		return age;
	}
}