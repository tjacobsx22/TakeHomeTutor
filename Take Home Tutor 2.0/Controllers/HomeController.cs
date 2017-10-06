using Flurl.Http;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Take_Home_Tutor_2._0.Models;

namespace Take_Home_Tutor_2._0.Controllers
{
    public class HomeController : Controller
    {
        //private static ConnectionMultiplexer _connection;

        //public static IServer GetDataCacheServer()
        //{
        //    if (_connection == null || !_connection.IsConnected)
        //    {
        //        _connection = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings.GetValues("RedisConnection").FirstOrDefault());
        //    }


        //    var endpoints = _connection.GetEndPoints();
        //    var server = _connection.GetServer(endpoints.First());
        //    return server;

        //}

        //public static IEnumerable<RedisKey> GetAllCacheKeys()
        //{
        //    var server = GetDataCacheServer();

        //    var keys = server.Keys();
        //    return keys;
        //}

        //public static void RedisStuff(){
        //IDatabase cache = Connection.GetDatabase();
        ////var key1 = cache.KeyDump("*");
        ////var endpoints = Connection.GetEndPoints();
        ////var server = Connection.GetServer(endpoints.First());
        ////var keys = server.Keys();
        //List<Tutor> user = new List<Tutor>();
        //var keys = GetAllCacheKeys();
        //List<string> cacheKeys = (from redisKey in keys
        //                          select redisKey.ToString()).ToList();
        //foreach (var key in cacheKeys.Where(k => k.Contains("_Data")))
        //{
        //    var val = cache.StringGet(key);
        //}
        //}

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HowItWorks()
        {
            return View();
        }

        public ActionResult Tutors()
        {
            return View();
        }

        public ActionResult GetTutors()
        {
            Tutor tut = new Tutor();
            List<Tutor> tutors = tut.GetAllTutors();
            return Json(tutors, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Live()
        {
            return View();
        }

        public async Task<ActionResult> GoLive(Guid id)
        {
            Tutor tutor = new Tutor();
            tutor = tutor.GetTutorsById(id);
            var myMeetings = await GetMeetingIds(tutor);
            var myMeetingIds = myMeetings.Split(',');
            ViewBag.ID = myMeetingIds[0];
            ViewBag.TutorID = tutor.ID;
            ViewBag.Tutor = tutor.BoardUser;
            ViewBag.SessionId = Guid.NewGuid();
            ViewBag.StripeKey = ConfigurationManager.AppSettings.GetValues("StripePublishableKey").FirstOrDefault();
            ViewBag.Minutes = ConfigurationManager.AppSettings.GetValues("TutorSessionDuration").FirstOrDefault();
            return View();
        }

        public async Task<ActionResult> PrivateLesson(Guid tutid, Guid pid)
        {
            Tutor tutor = new Tutor();
            Lesson lesson = new Lesson();
            tutor = tutor.GetTutorsById(tutid);
            lesson = lesson.GetIncompleteLessonsByTutorAndId(tutid, pid);
            //TODO:  If its not time for this lesson, return a message!
            TimeSpan diff = DateTime.Now.Subtract(lesson.ScheduleDate);
            if (diff.TotalMinutes > -15 && diff.TotalMinutes < 15)
            {
                tutor.CreateSession(pid, lesson.Email, tutid);
                var myMeetings = await GetMeetingIds(tutor);
                var myMeetingIds = myMeetings.Split(',');
                ViewBag.ID = myMeetingIds[0];
                return View();
            }
            else
            {
                ViewBag.ScheduledLesson = lesson.ScheduleDate;
                return View("ScheduledLesson", tutor);
            }
        }

        private async Task<string> GetMeetingIds(Tutor tutor)
        {
            var myMeetings = await "http://www.twiddla.com/API/ListActive.aspx"
                .PostUrlEncodedAsync(new { username = tutor.BoardUser, password = tutor.BoardPassword })
                .ReceiveString();
            if (myMeetings.Length < 1)
            {
                myMeetings = await "http://www.twiddla.com/API/CreateMeeting.aspx"
                    .PostUrlEncodedAsync(new { username = tutor.BoardUser, password = tutor.BoardPassword })
                    .ReceiveString();
            }
            return myMeetings.ToString();
        }

        public ActionResult GetOnlineTutors()
        {
            Tutor tut = new Tutor();
            List<Tutor> tutors = tut.GetOnlineTutors();
            IDatabase cache = RedisConnector.DatabaseConnect();

            foreach (var tutor in tutors)
            {
                RedisKey key = tutor.ID.ToString();
                if (!cache.KeyExists(key))
                {
                    tutor.IsOnline = false;
                    tutor.Save();
                }

            }

            return Json(tutors.Where(t => t.IsOnline), JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult ForTutors()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult SubmitContactMessage(string firstname, string lastname, string email, string message)
        {
            if(string.IsNullOrWhiteSpace(firstname) || string.IsNullOrWhiteSpace(lastname) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "All fields are required" }, JsonRequestBehavior.AllowGet);
            }
            Tutor tut = new Tutor();
            var msg = string.Format("{0} {1} <{2}> - {3}", firstname, lastname, email, message);
            tut.SendEmail("support@takehometutor.com", "Contact Us Request", msg);
            return Json( new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Blog()
        {
            return View();
        }

        public ActionResult TutorLogIn(string errorMsg)
        {
            if (Session["User"] != null)
            {
                Tutor tutor = (Tutor)Session["User"];
                return RedirectToAction("Index", "Profile");
            }
            return View(errorMsg);
        }

        public ActionResult TutorLogOut()
        {
            if (Session["User"] != null)
            {
                IDatabase cache = RedisConnector.DatabaseConnect();
                Tutor tutor = (Tutor)Session["User"];
                RedisKey key = tutor.ID.ToString();
                if (cache.KeyExists(key))
                    cache.KeyDelete(key);
                Session.Remove("User");
                FormsAuthentication.SignOut();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}