using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebPoll.Controllers
{
    public class HelloWorldController : Controller
    {
        //
        // GET: /HelloWorld/

        public ActionResult Index()
        {
            return View();
        }

        //public string Welcome()
        //{
        //    return "Hello! This is C# demo client";
        //}

        public ActionResult Welcome(string name, int numberOfTimes=1)
        {
            ViewBag.HelloName = name;
            ViewBag.NumberOfTimes = numberOfTimes;

            return View();
        }

    }
}
