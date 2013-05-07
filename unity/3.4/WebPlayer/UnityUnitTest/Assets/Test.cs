using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		JsonSample.PersonToJson();
		
		object message = ((new TestDemoMessage()).TryXmlDemo());
		//string myString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(message);
		string myString = Newtonsoft.Json.JsonConvert.SerializeObject(message);
		//string myString = UnitySerializer.JSONSerialize(message);
		//Newtonsoft.Json.Linq.JObject test = Newtonsoft.Json.Linq.JObject.Parse(myString);
		//string jsonString = JsonConvert.SerializeObject(test);
		Debug.Log("HH = " + myString);
		//Debug.Log("jsonString = " + jsonString);
		
//		var s = new SerializationWrapper();
//		string serializedObject;
//		using(var ms = new MemoryStream())
//        {
//            s.Serialize(ms, message);
//			TextReader reader = new StreamReader(ms);
//			serializedObject = reader.ReadToEnd();
//			Debug.Log("serializedObject = " + serializedObject);
//        }
		
		//LitJson.JsonWriter writer1 = new LitJson.JsonWriter(Console.Out);

		//string LitTest = LitJson.JsonMapper.ToJson(message);
		//Debug.Log("LitTest = " + LitTest);
		
		//LitJson.JsonMapper.t
		
		//JsonFx.Json.JsonWriter writer = new JsonFx.Json.JsonWriter();
		//string fxTest = writer.
		//Debug.Log("fxTest = " + fxTest);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}


	public class TestDemoMessage
    {
        public List<Person> TryXmlDemo()
        {
			List<Person> ret = new List<Person>();
			Person p1= new Person();
			p1.ID = "ABCD123";
			//PersonID id1 = new PersonID(); id1.ID = "ABCD123" ;
			//p1.ID = id1;
			Name n1 = new Name();
			n1.First = "John";
			n1.Middle = "P.";
			n1.Last = "Doe";
			p1.Name = n1;
		
			Address a1 = new Address();
			a1.Street = "123 Duck Street";
			a1.City = "New City";
			a1.State = "New York";
			a1.Country = "United States";
			p1.Address = a1;
			
			ret.Add(p1);
		
			Person p2= new Person();
			p2.ID = "ABCD456";
			//PersonID id2 = new PersonID(); id2.ID = "ABCD123" ;
			//p2.ID = id2;
			Name n2 = new Name();
			n2.First = "Peter";
			n2.Middle = "Z.";
			n2.Last = "Smith";
			p2.Name = n2;
		
			Address a2 = new Address();
			a2.Street = "12 Hollow Street";
			a2.City = "Philadelphia";
			a2.State = "Pennsylvania";
			a2.Country = "United States";
			p2.Address = a2;
		
			ret.Add(p2);
		
            //System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
			//xmlDocument.XmlResolver = null;
            //xmlDocument.LoadXml("<DemoRoot>
			//<Person ID='ABCD123'><Name><First>John</First><Middle>P.</Middle><Last>Doe</Last></Name>
			//<Address><Street>123 Duck Street</Street><City>New City</City><State>New York</State><Country>United States</Country></Address></Person>
			//<Person ID='ABCD456'><Name><First>Peter</First><Middle>Z.</Middle><Last>Smith</Last></Name><Address>
			//<Street>12 Hollow Street</Street><City>Philadelphia</City><State>Pennsylvania</State><Country>United States</Country></Address></Person></DemoRoot>");
            return ret;
        }
//        public string TryXmlDemo()
//        {
//            System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
//            xmlDocument.LoadXml("<DemoRoot><Person ID='ABCD123'><Name><First>John</First><Middle>P.</Middle><Last>Doe</Last></Name><Address><Street>123 Duck Street</Street><City>New City</City><State>New York</State><Country>United States</Country></Address></Person><Person ID='ABCD456'><Name><First>Peter</First><Middle>Z.</Middle><Last>Smith</Last></Name><Address><Street>12 Hollow Street</Street><City>Philadelphia</City><State>Pennsylvania</State><Country>United States</Country></Address></Person></DemoRoot>");
//            return xmlDocument;
//        }
    }

	public class Person
	{
		public string ID { get; set; }
	    // C# 3.0 auto-implemented properties
	    public Name   Name;
		public Address  Address;
	}

	public class Name
	{
		public string First { get; set; }
		public string Middle { get; set; }
		public string Last { get; set; }
	}

	public class Address
	{
			public string Street { get; set; }
			public string City { get; set; }
			public string State { get; set; }
			public string Country { get; set; }
	}

	public class PersonID
	{
		[XmlAttribute]
		public string ID { get; set;}
	}

public class JsonSample: MonoBehaviour
{
    public static void PersonToJson()
    {
        Person bill = new Person();
		
//		Name name = new Name();
//        name.First = "William";
//		name.Middle = "P.";
//		name.Last = " Shakespeare";
//		bill.Name = name;
//        bill.Age  = 51;
//        bill.Birthday = new DateTime(1564, 4, 26);
//		bill.SampleXml = new TestDemoMessage().TryXmlDemo();
		
		//string myString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(bill);
		//Debug.Log(myString);
		
        //string json_bill = UnitySerializer.JSONSerializeForDeserializeInto(bill);
        //Debug.Log(json_bill);
//		
//		Newtonsoft.Json.Linq.JObject test = Newtonsoft.Json.Linq.JObject.Parse(json_bill);
		//string jsonString = JsonConvert.SerializeObject(bill);
		//Debug.Log("PersonToJson -> jsonString = " + jsonString);
//		
//		var s = new SerializationWrapper();
//		string serializedObject;
//		using(var ms = new MemoryStream())
//        {
//            s.Serialize(ms, bill);
//			TextReader reader = new StreamReader(ms);
//			serializedObject = reader.ReadToEnd();
//			Debug.Log("serializedObject = " + serializedObject);
//        }
		
		
    }

    public static void JsonToPerson()
    {
        string json = @"
            {
                ""Name""     : ""Thomas More"",
                ""Age""      : 57,
                ""Birthday"" : ""02/07/1478 00:00:00""
            }";

//        Person thomas = JsonMapper.ToObject<Person>(json);
//
//        Debug.Log("Thomas' age: " + thomas.Age);
    }
}

public sealed class SerializationWrapper 
{
    readonly JsonSerializer serializer = new JsonSerializer();

    public void Serialize(Stream ms, object obj)
    {
        var jsonTextWriter = new JsonTextWriter(new StreamWriter(ms));
        serializer.Serialize(jsonTextWriter,obj);
        jsonTextWriter.Flush();
        ms.Position = 0;
    }

    public TType Deserialize<TType>(Stream ms)
    {
        var jsonTextReader = new JsonTextReader(new StreamReader(ms));
        return serializer.Deserialize<TType>(jsonTextReader);
    }
}

