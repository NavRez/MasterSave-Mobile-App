﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Authentication;
using Newtonsoft.Json.Linq;

namespace App1
{
    public class CosmosAccess
    {

        public CosmosAccess()
        {
            TextsList = new List<List<string>>();
            CashList = new List<List<string>>();

            string connectionString = @"mongodb://sara:rHFncA47vQfBGVYmZgIn6Q0Epc5gbdLvNYPvx6jwSP20cy65D2qQpzMWFgzRIOp1FHTfotmgfkqvgzeETkclZQ==@sara.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&maxIdleTimeMS=120000&appName=@sara@";
            //connectionString is the mongodb address from azure you should put
            MongoClientSettings settings = MongoClientSettings.FromUrl(
              new MongoUrl(connectionString)
            );
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            //same for the settings

            CosmosClient = new MongoClient(settings);
            MongoDatabase = CosmosClient.GetDatabase("admin");
            MongoCollection = MongoDatabase.GetCollection<BsonDocument>("sara");
            //name of my database
        }

        public void AddJson(JObject jObject)
        {
            BsonDocument bsonElements = BsonDocument.Parse(jObject.ToString());
            MongoCollection.InsertOne(bsonElements);
        }
        public Dictionary<string, BsonValue> RetrieveDict()
        {
            //name of my collection
            var documents = MongoCollection.Find(new BsonDocument()).ToList();
            var dictionary = new Dictionary<string, BsonValue>();

            List<Dictionary<string, BsonValue>> dictList = new List<Dictionary<string, BsonValue>>();

            foreach (BsonDocument bsons in documents)
            {
                var internalDict = new Dictionary<string, BsonValue>();
                Recurse(bsons, internalDict);
                dictList.Add(internalDict);

            }

            //var valTest = dictList[3]["readResults"][0]["lines"][0]["text"].RawValue;
            int i = 0;
            bool total = false;
            foreach (Dictionary<string, BsonValue> keyValuePairs in dictList)
            {
                List<string> simplifiedDict = new List<string>();
                List<string> simplifiedCashDict = new List<string>();
                while (true)
                {
                    try
                    {
                        if (i == 0)
                        {
                            var dateTime = keyValuePairs["createdDateTime"].RawValue;
                            simplifiedDict.Add((string)dateTime);
                            simplifiedCashDict.Add((string)dateTime);
                            total = false;
                            
                        }
                        var valTest = keyValuePairs["readResults"][0]["lines"][i++]["text"].RawValue;
                        simplifiedDict.Add((string)valTest);
                        string simpVal = valTest.ToString();
                        if (simpVal.Contains("$"))
                        {
                            simplifiedCashDict.Add(RemoveSpecialCharacters((string)valTest));
                            total = false;
                        }
                        else if (simpVal.Contains("TOTAL") || simpVal.Contains("Total"))
                        {
                            total = true;
                        }
                        else if (total)
                        {
                            simplifiedCashDict.Add(RemoveSpecialCharacters((string)valTest));
                            total = false;
                        }
                    }
                    catch (Exception e)
                    {
                        TextsList.Add(simplifiedDict);
                        if (simplifiedCashDict.Count > 1)
                        {
                            CashList.Add(simplifiedCashDict);
                        }
                        i = 0;
                        break;
                    }
                }
            }




            //Console.WriteLine("The list of databases are:");

            //foreach (BsonDocument doc in documents)
            //{
            //    Console.WriteLine(doc.ToString());
            //}
            return (dictionary = dictList.ElementAt<Dictionary<string, BsonValue>>(0));

        }


        //https://stackoverflow.com/questions/39024541/how-to-convert-mongo-document-into-key-value-pair-in-net

        private static void Recurse(BsonDocument doc, Dictionary<string, BsonValue> dictionary)
        {
            foreach (var elm in doc.Elements)
            {
                if (!elm.Value.IsBsonDocument)
                {
                    if (!dictionary.ContainsKey(elm.Name))
                    {
                        dictionary.Add(elm.Name, elm.Value);
                    }
                }
                else
                {
                    Recurse((elm.Value as BsonDocument), dictionary);
                }
            }
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || c == '.' || c == ',')
                {
                    if (c != ',')
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        sb.Append('.');
                    }
                }
            }
            return sb.ToString();
        }

        MongoClient CosmosClient { get; set; }
        IMongoDatabase MongoDatabase { get; set; }
        IMongoCollection<BsonDocument> MongoCollection { get; set; }

        public List<List<string>> TextsList { get; set; }
        public List<List<string>> CashList { get; set; }


    }
}