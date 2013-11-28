using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace PubnubSilverlight.UnitTest
{
    /// <summary>
    /// Custom class for testing the encryption and decryption 
    /// </summary>
    public class CustomClass
    {
        public string foo = "hi!";
        public int[] bar = { 1, 2, 3, 4, 5 };
    }

    //[Seri
    public class SecretCustomClass
    {
        public string foo = "hello!";

        public int[] bar = { 10, 20, 30, 40, 50 };
    }

    public class PubnubDemoObject
    {
        public double VersionID = 3.4;
        public string Timetoken = "13601488652764619";
        public string OperationName = "Publish";
        public string[] Channels = { "ch1" };
        public PubnubDemoMessage DemoMessage = new PubnubDemoMessage();
        public PubnubDemoMessage CustomMessage = new PubnubDemoMessage("Welcome to the world of Pubnub for Publish and Subscribe. Hah!");
        public Person[] SampleXml = new DemoRoot().Person.ToArray();
    }

    public class PubnubDemoMessage
    {
        public string DefaultMessage = "~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\ {}| asdfghjkl;' :\" zxcvbnm,./ <>? ";

        public PubnubDemoMessage()
        {
        }

        public PubnubDemoMessage(string message)
        {
            DefaultMessage = message;
        }

    }

    public class DemoRoot
    {
        public List<Person> Person
        {
            get
            {
                List<Person> ret = new List<Person>();
                Person p1 = new Person();
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

                Person p2 = new Person();
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

                return ret;

            }
        }
    }

    public class Person
    {
        public string ID { get; set; }

        public Name Name;

        public Address Address;
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

}
