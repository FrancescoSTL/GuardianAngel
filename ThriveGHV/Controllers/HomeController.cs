using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.Mvc;
using Parse;
using System.Threading.Tasks;
using System.Threading;

namespace ThriveGHV.Controllers
{
    public class HomeController : Controller
    {
        public IEnumerable<ParseObject> User;
        public IEnumerable<ParseObject> Citations;

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> About()
        {
                //get our message meta data
            string From = Request["From"];
            string To = Request["To"];
            string Body = Request["Body"];
            string name = Request["callerID"];

            bool hasAcct = false;

                //initialize our conversation depth and type
            int depth = 0;
            char convType = 'n';

                //initialize our reply
            string message = "Welcome to Guardian Angel. Type MENU to see what we can help with.";
            
                //initialize connection to parse & async function call
            User = await hasAccount(To);

                //check through all users in the list to see if their phone number is in there
            foreach (ParseObject user in User)
            {
                string phoneNumber = user.Get<String>("phoneNumber");
                if (phoneNumber == "6363686750") 
                    hasAcct = true;
            }

            //T548660236


                //if they aren't in the app
            if (!hasAcct)
            {
                saveUser(From.Substring(2, 10));
            }

                //check to see if the user wants to reset or go to the menu
            if (Body.ToUpper().Contains("RESET"))
            {
                Session["depth"] = 0;
                Session["convType"] = 'n';
                message = "Welcome to Guardian Angel. Type MENU to see what we can help with.";
            }
            else if (Body.ToUpper().Contains("MENU"))
            {
                Session["depth"] = 0;
                Session["convType"] = 'n';
                message = "If you have been pulled over, text a 1. To pay a ticket, text 2. To get court date information, text 3.";
            } else if (Session["depth"] != null && Session["convType"] != null)
            {

                //grab the current depth and convType from where previously saved
                depth = (int)Session["depth"];
                convType = (char)Session["convType"];

                //if we are at the base of our prompt
                if (depth == 0)
                {
                    //then set the conversation type based upon the user's input
                    if (Body.ToUpper().Contains("POLICE") || Body.ToUpper().Contains("PULLED OVER"))
                    {
                        convType = 'p';
                        depth++;
                        message = "Where are you located?";
                    }
                    else if (Body.ToUpper().Contains("PAY") && Body.ToUpper().Contains("TICKET") || Body.ToUpper().Contains("TICKET"))
                    {
                        convType = 't';
                        depth++;
                        message = "What is your drivers license number?";
                    }
                    else if (Body.ToUpper().Contains("COURT") || Body.ToUpper().Contains("CASE") || Body.ToUpper().Contains("CHARGE"))
                    {
                        convType = 'c';
                        depth++;
                        message = "Is your case completed?";
                    }
                    else if(Body.Length == 10)
                    {
                        Citations = await findCitations(Body);

                        foreach (ParseObject citation in Citations)
                        {
                            string dlNumber = citation.Get<String>("drivers_license_number");
                            if (dlNumber == Body)
                            {
                                message = "Court Location: " + citation.Get<String>("court_location") + " Court Date: " + citation.Get<String>("court_date");
                                break;
                            }
                        }
                    }
                    else
                        message = "Welcome to Guardian Angel. Type MENU to see what we can help with.";
                }
                else if(depth > 0)
                {
                    if (convType == 'p'){
                        if (depth <= 2)
                        {
                            message = policeConv(depth, Body);
                            depth++;
                        }
                        else
                        {
                            Session["depth"] =  0;
                            Session["convType"] = 'n';
                            depth = 0;
                            convType = 'n';
                        }
                        
                    }
                    else if (convType == 't'){
                        if(depth <= 1)
                        {
                            message = ticketConv(depth,Body);
                            depth++;
                        }
                        else
                        {
                            Session["depth"] =  0;
                            Session["convType"] = 'n';
                            depth = 0;
                            convType = 'n';
                        }
                    }
                    else if (convType == 'c') {
                        if(depth <= 5){
                            message = courtConv(depth,Body);
                            depth++;
                        }
                        else
                        {
                            Session["depth"] = 0;
                            Session["convType"] = 'n';
                            depth = 0;
                            convType = 'n';
                        }
                    }
                }
            }
  
                // save our depth and convtype
            Session["depth"] = depth;
            Session["convType"] = convType;

                //store it for our view
            ViewBag.Message = message;

                //send it to our view
            return View();
        }

        public async Task<IEnumerable<ParseObject>> hasAccount(string to)
        {
            ParseClient.Initialize("uKBwFfPQYPnu3xfAplcCzNSu3z3NFv1aT6YL8CGZ", "KzsuRCdQS8CQzaYkfYsrWZyKBj2RSLYiwmV6Igbf");
            ParseQuery<ParseObject> query = ParseObject.GetQuery("User");
            return await query.FindAsync();
        }

        public async Task saveUser(string phone)
        {
            ParseClient.Initialize("uKBwFfPQYPnu3xfAplcCzNSu3z3NFv1aT6YL8CGZ", "KzsuRCdQS8CQzaYkfYsrWZyKBj2RSLYiwmV6Igbf");
            ParseObject newUser = new ParseObject("User");
            newUser["phoneNumber"] = phone;
            await newUser.SaveAsync();
        }

        public async Task<IEnumerable<ParseObject>> findCitations(string dl)
        {
            ParseClient.Initialize("uKBwFfPQYPnu3xfAplcCzNSu3z3NFv1aT6YL8CGZ", "KzsuRCdQS8CQzaYkfYsrWZyKBj2RSLYiwmV6Igbf");
            ParseQuery<ParseObject> query = ParseObject.GetQuery("Citation");
            return await query.FindAsync();
        }

        public string policeConv(int depth, string response)
        {
            string message = "error in police conversation";

            if(depth == 1)
            {
                message = "Are you safe?";
            }
            if(depth == 2)
            {
                if (response.ToUpper().Contains("NO"))
                    message = "If you are in an emergency, call 911. We have sent a text message to your emergency contact with your location.";
                else
                    message = "We have sent a text message to your emergency contact with your location.";
            }
            if(depth == 3)
            {
                message = "What else can we help you with?";
            }

            return message;
        }

        public string ticketConv(int depth, string response)
        {
            string message = "error in ticket conversation";

            if (depth == 1)
            {
                message = "We're here to help. How would you like to pay your ticket?";
                depth++;
            }

            return message;
        }

        public string courtConv(int depth, string response)
        {
            string message = "error in court conversation";

            if (depth == 1)
            {
                message = "Do you have an upcoming court date?";
                depth++;
            }
            else if (depth == 2)
            {
                message = "Do you need to report anything?";
                depth++;
            }
            else if (depth == 3)
            {
                message = "What would you like to report?";
                depth++;
            }
            else if (depth == 4)
            {
                message = "How would you rate your experience 1-10?";
                depth++;
            }
            else if (depth == 5)
                message = "Noted, thank you for the feedback.";

            return message;
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}