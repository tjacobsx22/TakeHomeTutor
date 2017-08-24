using StackExchange.Redis;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Take_Home_Tutor_2._0.Models;
using System.Net.Mail;
using SendGrid;
using System.Net;
using System.Configuration;

namespace Take_Home_Tutor_2._0.Controllers
{
    public class SharedController : Controller
    {
        // GET: Shared
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Terms()
        {
            return View();
        }

        public ActionResult TutorBooking(Guid id)
        {
            ViewBag.StripeKey = ConfigurationManager.AppSettings.GetValues("StripePublishableKey").FirstOrDefault();
            Tutor tut = new Tutor();
            return View(tut.GetTutorsById(id));
        }

        public ActionResult Review(string first, string last, string student, string email, string tok, Guid ID, string tutor)
        {
            if (first != "" && last != "" & student != "" & email != "" & tok != "" & ID != Guid.Empty & tutor != "")
            {
                var myCharge = new StripeChargeCreateOptions();

                // always set these properties
                myCharge.Amount = 2000;
                myCharge.Currency = "usd";

                // setting up the card
                myCharge.Source = new StripeSourceOptions()
                {
                    // set this property if using a token
                    TokenId = tok,
                };

                var chargeService = new StripeChargeService();
                StripeCharge stripeCharge = chargeService.Create(myCharge);
                if (stripeCharge.Status == "succeeded")
                {
                    Tutor tut = new Tutor();
                    tut.CreateScheduledSession(first, last, student, email, ID, tutor);
                    //alert tutor
                }
                return Json(stripeCharge.Status, JsonRequestBehavior.AllowGet);
            }
            else {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult KeepAlive()
        {
            IDatabase cache = RedisConnector.DatabaseConnect();
            Tutor tutor = (Tutor)Session["User"];
            RedisKey key = tutor.ID.ToString();
            if (cache.KeyExists(key))
            {
                //update key to keep it fresh
                var expiration = DateTimeOffset.Now.AddMinutes(5);
                cache.KeyExpire(key, expiration.Subtract(DateTimeOffset.Now));
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult KillSession()
        {
            IDatabase cache = RedisConnector.DatabaseConnect();
            Tutor tutor = (Tutor)Session["User"];
            tutor.IsOnline = false;
            tutor.Save();
            RedisKey key = tutor.ID.ToString();
            if (cache.KeyExists(key))
            {
                //update key to keep it fresh
                cache.KeyDelete(key);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult EndLesson(string comment, Guid lessonId)
        {
            Tutor tut = new Tutor();
            tut.TutorSessionEnd(lessonId, comment);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult StudentEndLesson(int rating, string comment, Guid lessonId)
        {
            Tutor tut = new Tutor();
            tut.StudentSessionEnd(lessonId, comment, rating);
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Charge(string tok, string email, Guid session, Guid tutor)
        {
            var myCharge = new StripeChargeCreateOptions();

            // always set these properties
            myCharge.Amount = 2000;
            myCharge.Currency = "usd";

            // setting up the card
            myCharge.Source = new StripeSourceOptions()
            {
                // set this property if using a token
                TokenId = tok,
            };

            var chargeService = new StripeChargeService();
            StripeCharge stripeCharge = chargeService.Create(myCharge);
            Tutor tut = new Tutor();
            tut.CreateSession(session, email, tutor);
            return Json(stripeCharge.Status, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendEmail(string email, string subject, string message)
        {
            // Create the email object first, then add the properties.
            var myMessage = new SendGridMessage();

            // Add the message properties.
            myMessage.From = new MailAddress("Take Home Tutor <support@takehometutor.com>");

            // Add multiple addresses to the To field.
            List<string> recipients = new List<string>();

#if DEBUG
            recipients.Add("terrygjacobs@gmail.com");
#endif
#if !DEBUG
            recipients.Add(email);
#endif
            

            myMessage.AddTo(recipients);

            myMessage.Subject = subject;

            //Add the HTML and Text bodies
            myMessage.Text = message;

            string conn = ConfigurationManager.AppSettings.GetValues("EmailConnection").FirstOrDefault();
            string pass = ConfigurationManager.AppSettings.GetValues("EmailPass").FirstOrDefault();

            // Create credentials, specifying your user name and password.
            var credentials = new NetworkCredential(conn, pass);

            // Create a REST transport for sending email.
            var transportREST = new Web(credentials);

            // Send the email.
            transportREST.DeliverAsync(myMessage);

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Apply()
        {
            List<SelectListItem> subjects = new List<SelectListItem>();

            SelectListItem math = new SelectListItem { Text = Constants.Subject.math, Value = Constants.Subject.math };
            subjects.Add(math);
            SelectListItem eng = new SelectListItem { Text = Constants.Subject.ela, Value = Constants.Subject.ela };
            subjects.Add(eng);
            SelectListItem four_six = new SelectListItem { Text = Constants.Subject.four_six, Value = Constants.Subject.four_six };
            subjects.Add(four_six);
            SelectListItem highschool = new SelectListItem { Text = Constants.Subject.highschool, Value = Constants.Subject.highschool };
            subjects.Add(highschool);
            SelectListItem k_four = new SelectListItem { Text = Constants.Subject.k_four, Value = Constants.Subject.k_four };
            subjects.Add(k_four);
            SelectListItem science = new SelectListItem { Text = Constants.Subject.science, Value = Constants.Subject.science };
            subjects.Add(science);
            SelectListItem ss = new SelectListItem { Text = Constants.Subject.social_studies, Value = Constants.Subject.social_studies };
            subjects.Add(ss);
            SelectListItem middle = new SelectListItem { Text = Constants.Subject.middleschool, Value = Constants.Subject.middleschool };
            subjects.Add(middle);

            IEnumerable<SelectListItem> subject = subjects;
            ViewBag.Subjects = subject;
            return View();
        }

        [HttpPost]
        public ActionResult Application(string firstname, string lastname, string email, string password, int zip, string[] subject, string bio)
        {
            Tutor tut = new Tutor { FirstName = firstname, LastName = lastname, Email = email, Password = password, Zip = zip, Subject = string.Join(", ", subject), Bio = bio };
            tut.Create();
            tut.LogIn(email, password);
            return RedirectToAction("Index", "Profile");
        }

        public ActionResult TutorLogIn(Tutor tutor)
        {
            return View(tutor);
        }
    }
}