using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Take_Home_Tutor_2._0.Models
{
    class Lesson
    {
        public Guid PurchaseId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string StudentName { get; set; }
        public Guid TutorId { get; set; }
        public string TutorName { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ScheduleDate { get; set; }
        public bool Completed { get; set; }


        public List<Lesson> GetIncompleteLessonsById(Guid tutid)
        {
            var lessons = new List<Lesson>();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);

            con.Open();
            //Get tutors in these zipcodes
            string query = String.Format("SELECT * FROM ScheduledLessons WHERE Completed = 0 AND TutorId = '{0}'", tutid);
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var pid = new Guid();
                        var tid = new Guid();
                        var dt = new DateTime();
                        var st = new DateTime();
                        var complete = false;
                        bool.TryParse(rdr["LastName"].ToString(), out complete);
                        Guid.TryParse(rdr["PurchaseId"].ToString(), out pid);
                        Guid.TryParse(rdr["TutorId"].ToString(), out tid);
                        DateTime.TryParse(rdr["CreateDate"].ToString(), out dt);
                        DateTime.TryParse(rdr["ScheduleDate"].ToString(), out st);
                        Lesson lesson = new Lesson
                        {
                            PurchaseId = pid,
                            FirstName = rdr["FirstName"].ToString(),
                            LastName = rdr["LastName"].ToString(),
                            TutorId = tid,
                            CreateDate = dt,
                            ScheduleDate = st,
                            Email = rdr["Email"].ToString(),
                            StudentName = rdr["StudentName"].ToString(),
                            TutorName = rdr["TutorName"].ToString(),
                            Completed = complete
                        };

                        lessons.Add(lesson);
                    }
                }
            }

            con.Close();

            return lessons;
        }

        public Lesson GetIncompleteLessonsByTutorAndId(Guid tutid, Guid purchaseid)
        {
            var lesson = new Lesson();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);

            con.Open();
            //Get tutors in these zipcodes
            string query = string.Format("SELECT TOP 1 * FROM ScheduledLessons WHERE purchaseid = '{0}' AND TutorId = '{1}'", purchaseid, tutid);
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var pid = new Guid();
                        var tid = new Guid();
                        var dt = new DateTime();
                        var st = new DateTime();
                        var complete = false;
                        bool.TryParse(rdr["LastName"].ToString(), out complete);
                        Guid.TryParse(rdr["PurchaseId"].ToString(), out pid);
                        Guid.TryParse(rdr["TutorId"].ToString(), out tid);
                        DateTime.TryParse(rdr["CreateDate"].ToString(), out dt);
                        DateTime.TryParse(rdr["ScheduleDate"].ToString(), out st);
                        lesson = new Lesson
                        {
                            PurchaseId = pid,
                            FirstName = rdr["FirstName"].ToString(),
                            LastName = rdr["LastName"].ToString(),
                            TutorId = tid,
                            CreateDate = dt,
                            ScheduleDate = st,
                            Email = rdr["Email"].ToString(),
                            StudentName = rdr["StudentName"].ToString(),
                            TutorName = rdr["TutorName"].ToString(),
                            Completed = complete
                        };
                    }
                }
            }

            con.Close();

            return lesson;
        }

        public void UpdateScheduleByPid(DateTime date, string url, Guid pid, Guid tutid)
        {
            try
            {
                DateTime dt = new DateTime(date.Ticks, DateTimeKind.Utc);
                string stringDate = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);

                con.Open();
                //Get tutors in these zipcodes
                SqlCommand myCmd = new SqlCommand("UPDATE scheduledlessons set ScheduleDate = @date, LessonUrl = @url WHERE PurchaseId = @pid AND TutorId = @tutid", con);
                myCmd.Parameters.AddWithValue("@date", date);
                myCmd.Parameters.AddWithValue("@url", url);
                myCmd.Parameters.AddWithValue("@pid", pid);
                myCmd.Parameters.AddWithValue("@tutid", tutid);
                myCmd.ExecuteNonQuery();

                con.Close();
            }catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
