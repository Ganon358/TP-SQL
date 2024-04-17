namespace ReplicaSet.Sevenet;

using System.Text.Json.Serialization;
using System;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.Serialization;

[Serializable]
[DataContract]
public record class UserData {
    [BsonId]
    [DataMember]
	[BsonElement("_id")]
    public MongoDB.Bson.ObjectId Id { get; set; }

    [DataMember]
	[BsonElement("name")]
	public string Name {get; set;} = "";

    [DataMember]
	[BsonElement("age")]
	public byte Age {get; set;} = 0;

    [DataMember]
	[BsonElement("email")]
	public string Email {get; set;} = "";

    [DataMember]
	[BsonElement("createdAt")]
	public DateTime CreatedAt {get; set;} = DateTime.UnixEpoch;

}