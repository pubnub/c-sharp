using System;
using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
//using LitJson;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		System.Xml.XmlDocument message = (new TestDemoMessage()).TryXmlDemo();
		string myString = Newtonsoft.Json.JsonConvert.SerializeObject(message);
		Debug.Log("HH = " + myString);
		
		//string LitTest = LitJson.JsonMapper.ToJson(message);
		//Debug.Log("LitTest = " + LitTest);
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}


	public class TestDemoMessage
    {
        public System.Xml.XmlDocument TryXmlDemo()
        {
            System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.LoadXml("<DemoRoot><Person ID='ABCD123'><Name><First>John</First><Middle>P.</Middle><Last>Doe</Last></Name><Address><Street>123 Duck Street</Street><City>New City</City><State>New York</State><Country>United States</Country></Address></Person><Person ID='ABCD456'><Name><First>Peter</First><Middle>Z.</Middle><Last>Smith</Last></Name><Address><Street>12 Hollow Street</Street><City>Philadelphia</City><State>Pennsylvania</State><Country>United States</Country></Address></Person></DemoRoot>");
            return xmlDocument;
        }
    }

