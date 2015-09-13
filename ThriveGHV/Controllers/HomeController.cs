using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.Mvc;

namespace ThriveGHV.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            int counter=0;

                // get the session varible if it exists    
            if (Session["counter"]!=null) { 
                counter=(int)Session["counter"]; 
            }
     
                // increment it
            counter++;
  
                // save it
            Session["counter"] = counter;

                //get our message meta data
            string From = Request["From"];
            string To = Request["To"];
            string Body = Request["Body"];

                //store it for our view
            ViewBag.Name = Body;
            ViewBag.Count = counter;

                //send it to our view
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}