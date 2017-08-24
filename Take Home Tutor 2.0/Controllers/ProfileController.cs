using System.Collections.Generic;
using System.Web.Mvc;
using Take_Home_Tutor_2._0.Models;
using Flurl.Http;
using StackExchange.Redis;
using Newtonsoft.Json;
using System;
using System.Web;
using System.IO;
using System.Configuration;
using System.Linq;

namespace Take_Home_Tutor_2._0.Controllers
{
    public class ProfileController : Controller
    {
        public ActionResult Index(string email, string password)
        {
            Tutor tutor = new Tutor();
            if (System.Web.HttpContext.Current.Session["User"] != null)
            {
                var tut = System.Web.HttpContext.Current.Session["User"];
                tutor = (Tutor)tut;
            }
            else
            {
                tutor = tutor.LogIn(email, password);
                if (tutor.Email == null)
                {
                    return RedirectToAction("TutorLogIn", "Shared", tutor);
                }
            }
            return View(tutor);
        }

        public ActionResult GetLessons()
        {
            Lesson l = new Lesson();
            Tutor tutor = new Tutor();
            var tut = System.Web.HttpContext.Current.Session["User"];
            tutor = (Tutor)tut;
            var lessons = l.GetIncompleteLessonsById(tutor.ID);
            return Json(lessons, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveSchedule(string date, string url, Guid id, Guid tutid, string email)
        {
            Lesson les = new Lesson();
            DateTime dt = new DateTime();
            if (DateTime.TryParse(date, out dt))
            {
                les.UpdateScheduleByPid(dt, url, id, tutid);
                Tutor tut = new Tutor();
                tut = tut.GetTutorsById(tutid);
                string message = string.Format("You have an appointment with {0} on {1}.  If you need to re-schedule please contact your tutor as soon as possible. " +
                    "You will use the following link to connect to your tutoring session during the scheduled date and time: {2}", tut.FullNameEmail, date, url);
                tut.SendEmail(email, "New Appointment - Take Home Tutor", message, tut.FullNameEmail);
            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> SaveProfImg(HttpPostedFileBase profPic, Guid tutId)
        {
            ImageService _imageService = new ImageService();
            Tutor tutor = new Tutor();
            var tut = tutor.GetTutorsById(tutId);
            var uploadedImage = await _imageService.CreateUploadedImage(profPic, string.Format("{0}{1}", tut.FirstName, tut.LastName));
            await _imageService.AddImageToBlobStorageAsync(uploadedImage);
            //Azure blob storage

            tut.ProfileImage = uploadedImage.Url;
            tut.Update();

            return Json(uploadedImage.Url, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GoOnline(bool isOnline)
        {
            Tutor tutor = new Tutor();
            if (System.Web.HttpContext.Current.Session["User"] != null)
            {
                var tut = System.Web.HttpContext.Current.Session["User"];
                tutor = (Tutor)tut;
                var serializedObject = JsonConvert.SerializeObject(tutor);
                IDatabase cache = RedisConnector.DatabaseConnect();
                if (isOnline)
                {
                    var expiration = DateTimeOffset.Now.AddMinutes(5);
                    cache.StringSet(tutor.ID.ToString(), serializedObject, expiration.Subtract(DateTimeOffset.Now));
                }
                else
                {
                    RedisKey key = tutor.ID.ToString();
                    if(cache.KeyExists(key))
                        cache.KeyDelete(key);
                }
                tutor.IsOnline = isOnline;
                tutor.Save();
            }
            return Json(isOnline, JsonRequestBehavior.AllowGet);
        }

        public async System.Threading.Tasks.Task<ActionResult> LiveBoard()
        {
            Tutor tutor = new Tutor();
            if (System.Web.HttpContext.Current.Session["User"] != null)
            {
                var meetingId = "";
                tutor = (Tutor)System.Web.HttpContext.Current.Session["User"];
                var myMeetings = await "http://www.twiddla.com/API/ListActive.aspx"
                    .PostUrlEncodedAsync(new { username = tutor.BoardUser, password = tutor.BoardPassword })
                    .ReceiveString();
                if (string.IsNullOrEmpty(myMeetings))
                {
                    meetingId = await "http://www.twiddla.com/API/CreateMeeting.aspx"
                        .PostUrlEncodedAsync(new { username = tutor.BoardUser, password = tutor.BoardPassword })
                        .ReceiveString();
                }
                else
                {
                    var myMeetingIds = myMeetings.Split(',');
                    meetingId = myMeetingIds[0];
                }
                ViewBag.ID = meetingId;
                ViewBag.Name = tutor.BoardUser;
                ViewBag.Pass = tutor.BoardPassword;
                ViewBag.TutorID = tutor.ID;
                ViewBag.Email = tutor.Email;
                ViewBag.Minutes = ConfigurationManager.AppSettings.GetValues("TutorSessionDuration").FirstOrDefault();
                return View();
            }
            return View();
        }

        public async System.Threading.Tasks.Task<ActionResult> NewTwiddlaUser(string username, string password)
        {
            Tutor tutor = new Tutor();
            if (System.Web.HttpContext.Current.Session["User"] != null)
            {
                tutor = (Tutor)System.Web.HttpContext.Current.Session["User"];
                var boardDisplayName = string.Format("{0} {1}", tutor.FirstName, tutor.LastName);
                var newUser = await "http://www.twiddla.com/API/CreateUser.aspx"
                    .PostUrlEncodedAsync(new { username = ConfigurationManager.AppSettings.GetValues("TwiddlaApiUser").FirstOrDefault(), password = ConfigurationManager.AppSettings.GetValues("TwiddlaApiPass").FirstOrDefault(),
                        newusername = username, newpassword = password, displayname = boardDisplayName })
                    .ReceiveString();
                if(newUser.Contains("-1"))
                {
                    var errorMsg = newUser.Replace("-1", "");
                    return Json(new { success = false, msg = errorMsg }, JsonRequestBehavior.AllowGet);
                }
                tutor.BoardUser = username;
                tutor.BoardPassword = password;
                tutor.BoardDisplay = boardDisplayName;
                tutor.Update();
            }
            return Json(new { success = true, msg = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ContractTerms()
        {
            Tutor tutor = new Tutor();
            if (System.Web.HttpContext.Current.Session["User"] != null)
            {
                tutor = (Tutor)System.Web.HttpContext.Current.Session["User"];
                return View(tutor);
            }
            return RedirectToAction("TutorLogIn", "Shared", tutor);
        }

        public ActionResult AcceptedContract()
        {
            Tutor tutor = new Tutor();
            if (System.Web.HttpContext.Current.Session["User"] != null)
            {
                tutor = (Tutor)System.Web.HttpContext.Current.Session["User"];
                tutor.AcceptTerms(tutor);
                tutor.AcceptedTerms = true;
                return RedirectToAction("Index");
            }
            return RedirectToAction("TutorLogIn", "Shared", tutor);
        }
    }
}