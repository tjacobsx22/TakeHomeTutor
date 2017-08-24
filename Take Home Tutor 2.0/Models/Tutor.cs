using DevOne.Security.Cryptography.BCrypt;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Security;

namespace Take_Home_Tutor_2._0.Models
{
    [Serializable]
    public class Tutor
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Subject { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; }
        public string ProfileImage { get; set; }
        public bool Approved { get; set; }
        public string Email { get; set; }
        public int Zip { get; set; }
        public bool IsOnline { get; set; }
        public string BoardUser { get; set; }
        public string BoardPassword { get; set; }
        public string BoardDisplay { get; set; }
        public bool AcceptedTerms { get; set; }
        public string FullNameEmail
        {
            get { return string.Format("{0} {1} <{2}>", FirstName, LastName, Email); }
            set { /* just a name formatter */ }
        }

        public List<Tutor> GetAllTutors()
        {
            List<Tutor> tutors = new List<Tutor>();

            //Get results
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            con.Open();
            //Get tutors in these zipcodes
            string query = String.Format("SELECT * FROM Tutors t left join Terms trm on t.id = trm.TutorId where t.approved = 1 AND trm.AcceptedTerms = 1");
            using (SqlCommand cmd2 = new SqlCommand(query, con))
            {
                using (var rdr = cmd2.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Guid id = Guid.Empty;
                        Guid.TryParse(rdr["id"].ToString(), out id);
                        Tutor tut = new Tutor
                        {
                            ID = id,
                            FirstName = rdr["fname"].ToString(),
                            LastName = rdr["lname"].ToString(),
                            Bio = rdr["bio"].ToString(),
                            Subject = rdr["subject"].ToString(),
                            ProfileImage = rdr["ProfileImg"].ToString(),
                            Approved = rdr["approved"] == DBNull.Value ? false : Convert.ToBoolean(rdr["approved"]),
                            IsOnline = rdr["online"] == DBNull.Value ? false : Convert.ToBoolean(rdr["online"]),
                            AcceptedTerms = rdr["AcceptedTerms"] == DBNull.Value ? false : Convert.ToBoolean(rdr["AcceptedTerms"]),
                            BoardUser = rdr["whiteboardName"].ToString()
                        };
                        tutors.Add(tut);
                    }
                }
            }

            con.Close();

            //Return list
            return tutors.OrderByDescending(t => t.IsOnline == true).ToList();
        }

        public byte[] GetProfileImageById(Guid id)
        {
            //Get results
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            byte[] file = new byte[0];
            con.Open();
            string query = String.Format("SELECT * FROM Tutors WHERE approved = 1");
            using (SqlCommand cmd2 = new SqlCommand(query, con))
            {
                using (var rdr = cmd2.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        file = (byte[])rdr["ProfileImg"];
                    }
                }
            }
            con.Close();
            return file;
        }

        public List<Tutor> GetTutorsByZip(int zipcode)
        {
            List<int> zipcodes = new List<int>();
            List<Tutor> tutors = new List<Tutor>();

            //Get results
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "usp_getZipCodesByRadius";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("ZipCode", zipcode);
            cmd.Parameters.AddWithValue("Distance", 10);

            cmd.Connection = con;

            con.Open();

            reader = cmd.ExecuteReader();
            // Data is accessible through the DataReader object here.
            while (reader.Read())
            {
                int zip = (int)reader["Zipcode"];
                zipcodes.Add(zip);
            }
            con.Close();

            con.Open();
            //Get tutors in these zipcodes
            string query = String.Format("SELECT * FROM  Tutors t left join Terms trm on t.id = trm.TutorId where t.zip IN({0})", string.Join(",", zipcodes));
            using (SqlCommand cmd2 = new SqlCommand(query, con))
            {
                using (var rdr = cmd2.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Guid id = Guid.Empty;
                        Guid.TryParse(rdr["id"].ToString(), out id);
                        Tutor tut = new Tutor {
                            ID = id,
                            FirstName = rdr["fname"].ToString(),
                            LastName = rdr["lname"].ToString(),
                            Bio = rdr["bio"].ToString(),
                            Subject = rdr["subject"].ToString(),
                            ProfileImage = rdr["ProfileImg"].ToString(),
                            Approved = rdr["approved"] == DBNull.Value ? false : Convert.ToBoolean(rdr["approved"]),
                            IsOnline = rdr["online"] == DBNull.Value ? false : Convert.ToBoolean(rdr["online"]),
                            AcceptedTerms = rdr["AcceptedTerms"] == DBNull.Value ? false : Convert.ToBoolean(rdr["AcceptedTerms"])
                        };

                        tutors.Add(tut);
                    }
                }
            }

            con.Close();

            //Return list
            return tutors;
        }

        public Tutor GetTutorsById(Guid tutId)
        {
            Tutor tutor = new Tutor();
            //Get results
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "usp_getTutorById";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("id", tutId);

            cmd.Connection = con;

            con.Open();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    Guid id = Guid.Empty;
                    Guid.TryParse(rdr["id"].ToString(), out id);
                    tutor = new Tutor
                    {
                        ID = id,
                        FirstName = rdr["fname"].ToString(),
                        LastName = rdr["lname"].ToString(),
                        Bio = rdr["bio"].ToString(),
                        Subject = rdr["subject"].ToString(),
                        ProfileImage = rdr["ProfileImg"].ToString(),
                        Email = rdr["Email"].ToString(),
                        Approved = rdr["approved"] == DBNull.Value ? false : Convert.ToBoolean(rdr["approved"]),
                        IsOnline = rdr["online"] == DBNull.Value ? false : Convert.ToBoolean(rdr["online"]),
                        AcceptedTerms = rdr["AcceptedTerms"] == DBNull.Value ? false : Convert.ToBoolean(rdr["AcceptedTerms"]),
                        BoardUser = rdr["whiteboardName"].ToString(),
                        BoardPassword = rdr["whiteboardPassword"].ToString()
                    };
                }
            }
            con.Close();

            //Return list
            return tutor;
        }

        public List<Tutor> GetOnlineTutors()
        {
            List<Tutor> tutors = new List<Tutor>();

            //Get results
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            SqlCommand cmd = new SqlCommand();

            con.Open();
            //Get tutors in these zipcodes
            string query = String.Format("SELECT * From Tutors t left join Terms trm on t.id = trm.TutorId where t.online = 1");
            using (SqlCommand cmd2 = new SqlCommand(query, con))
            {
                using (var rdr = cmd2.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Guid id = Guid.Empty;
                        Guid.TryParse(rdr["id"].ToString(), out id);
                        Tutor tut = new Tutor
                        {
                            ID = id,
                            FirstName = rdr["fname"].ToString(),
                            LastName = rdr["lname"].ToString(),
                            Email = rdr["email"].ToString(),
                            Bio = rdr["bio"].ToString(),
                            Subject = rdr["subject"].ToString(),
                            ProfileImage = rdr["ProfileImg"].ToString(),
                            Approved = rdr["approved"] == DBNull.Value ? false : Convert.ToBoolean(rdr["approved"]),
                            IsOnline = rdr["online"] == DBNull.Value ? false : Convert.ToBoolean(rdr["online"]),
                            AcceptedTerms = rdr["AcceptedTerms"] == DBNull.Value ? false : Convert.ToBoolean(rdr["AcceptedTerms"]),
                            BoardUser = rdr["whiteboardName"].ToString()

                        };

                        tutors.Add(tut);
                    }
                }
            }

            con.Close();

            //Return list
            return tutors;
        }

        public bool IsUsernameTaken(string UserName)
        {
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            myConnection.Open();
            SqlCommand myCommand = new SqlCommand("SELECT COUNT(*) AS Rows FROM users WHERE userid=@username", myConnection);
            myCommand.Parameters.AddWithValue("@username", UserName);
            SqlDataReader reader = myCommand.ExecuteReader();
            int rows = 0;
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    rows = (int)reader["Rows"];
                }
            }
            reader.Close();
            if (rows > 1)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public void CreateSession(Guid sessionId, string studentEmail, Guid tutorId)
        {
            try
            {
                SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
                myConnection.Open();
                SqlCommand myCmd = new SqlCommand("INSERT INTO Lessons" +
                 "(TutorId, SessionId, StudentEmail, StartDate) " +
                 "VALUES (@tut, @session, @user, @date)", myConnection);
                myCmd.Parameters.AddWithValue("@session", sessionId);
                myCmd.Parameters.AddWithValue("@user", studentEmail);
                myCmd.Parameters.AddWithValue("@tut", tutorId);
                myCmd.Parameters.AddWithValue("@date", DateTime.Now);
                myCmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateScheduledSession(string first, string last, string student, string email, Guid ID, string tutor)
        {
            try
            {
                Guid lessonId = Guid.NewGuid();
                SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
                myConnection.Open();
                SqlCommand myCommand = new SqlCommand("INSERT INTO ScheduledLessons (PurchaseId, FirstName, LastName, Email, StudentName, TutorId, TutorName, CreateDate, Completed) " +
                             "Values (@id, @first, @last, @email, @name, @tutId, @tutName, @date, @complete)", myConnection);
                myCommand.Parameters.AddWithValue("@id", lessonId);
                myCommand.Parameters.AddWithValue("@first", first);
                myCommand.Parameters.AddWithValue("@last", last);
                myCommand.Parameters.AddWithValue("@email", email);
                myCommand.Parameters.AddWithValue("@name", student);
                myCommand.Parameters.AddWithValue("@tutId", ID);
                myCommand.Parameters.AddWithValue("@tutName", tutor);
                myCommand.Parameters.AddWithValue("@date", DateTime.Now);
                myCommand.Parameters.AddWithValue("@complete", false);
                myCommand.ExecuteNonQuery();
                myConnection.Close();

                Tutor tut = GetTutorsById(ID);
                SendEmail(tut.Email, "New Appointment", string.Format("A new appointment with {0} has been requested by {1} {2}.  Please send them an email asap to schedule the lesson.  The customers email is: {3} . \r\n Your reference number for this lesson is: {4}", student, first, last, email, lessonId));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void TutorSessionEnd(Guid sessionId, string comment)
        {
            try
            {
                SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
                myConnection.Open();
                SqlCommand myCmd = new SqlCommand("UPDATE Lessons " +
                 "SET TutorComments = @comment " +
                 "WHERE SessionId = @session", myConnection);
                myCmd.Parameters.AddWithValue("@session", sessionId);
                myCmd.Parameters.AddWithValue("@comment", comment);
                myCmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void StudentSessionEnd(Guid sessionId, string comment, int rating)
        {
            try
            {
                SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
                myConnection.Open();
                SqlCommand myCmd = new SqlCommand("UPDATE Lessons " +
                 "SET StudentComments = @comment, Rating = @rating " +
                 "WHERE SessionId = @session", myConnection);
                myCmd.Parameters.AddWithValue("@session", sessionId);
                myCmd.Parameters.AddWithValue("@comment", comment);
                myCmd.Parameters.AddWithValue("@rating", rating);
                myCmd.ExecuteNonQuery();
                myConnection.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void Save()
        {
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            myConnection.Open();
            SqlCommand myCmd = new SqlCommand("UPDATE Tutors " +
             "SET online = @online " +
             "WHERE email = @user", myConnection);
            myCmd.Parameters.AddWithValue("@online", this.IsOnline);
            myCmd.Parameters.AddWithValue("@user", this.Email);
            myCmd.ExecuteNonQuery();
            myConnection.Close();
        }

        internal void Update()
        {
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            myConnection.Open();
            SqlCommand myCmd = new SqlCommand("UPDATE Tutors " +
             "SET ProfileImg = @ProfileImg, " +
             "whiteboardName = @whitebaordName, " +
             "whiteboardPassword = @whiteboardPassword " +
             "WHERE email = @user", myConnection);
            myCmd.Parameters.AddWithValue("@ProfileImg", this.ProfileImage);
            myCmd.Parameters.AddWithValue("@whitebaordName", this.BoardUser);
            myCmd.Parameters.AddWithValue("@whiteboardPassword", this.BoardPassword);
            myCmd.Parameters.AddWithValue("@user", this.Email);
            myCmd.ExecuteNonQuery();
            if (HttpContext.Current.Session["User"] != null)
            {
                HttpContext.Current.Session["User"] = this;
            }
            myConnection.Close();
        }

        internal void Create()
        {
            this.ID = Guid.NewGuid();
            string mySalt = BCryptHelper.GenerateSalt();
            string myHash = BCryptHelper.HashPassword(Password, mySalt);
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            myConnection.Open();
            SqlCommand myCmd = new SqlCommand("INSERT INTO Tutors " +
             "VALUES(@id, @fname, @lname, @zip, @subject, @email, @password, @ProfileImg, @bio, @approved, @online, @whiteboardName, @whiteboardPassword)", myConnection);
            myCmd.Parameters.AddWithValue("@id", this.ID);
            myCmd.Parameters.AddWithValue("@fname", this.FirstName);
            myCmd.Parameters.AddWithValue("@lname", this.LastName);
            myCmd.Parameters.AddWithValue("@zip", this.Zip);
            myCmd.Parameters.AddWithValue("@subject", this.Subject);
            myCmd.Parameters.AddWithValue("@email", this.Email);
            myCmd.Parameters.AddWithValue("@password", myHash);
            myCmd.Parameters.AddWithValue("@ProfileImg", "");
            myCmd.Parameters.AddWithValue("@bio", this.Bio);
            myCmd.Parameters.AddWithValue("@approved", false);
            myCmd.Parameters.AddWithValue("@online", false);
            myCmd.Parameters.AddWithValue("@whiteboardName", "");
            myCmd.Parameters.AddWithValue("@whiteboardPassword", "");

            myCmd.ExecuteNonQuery();
            myConnection.Close();

            SendEmail("terrygjacobs@gmail.com", "New Applicant", string.Format("A new applicant has applied:\r\n Email:{0} \r\n First Name: {1} \r\n Last Name: {2} \r\n ID: {3}", Email, FirstName, LastName, ID));
        }

        public void SendEmail(string email, string subject, string message, string from = "Take Home Tutor <support@takehometutor.com>")
        {
            // Create the email object first, then add the properties.
            var myMessage = new SendGridMessage();

            // Add the message properties.
            myMessage.From = new MailAddress(from);

            // Add multiple addresses to the To field.
            List<string> recipients = new List<string>();

#if DEBUG
            recipients.Add("terrygjacobs@gmail.com");
            myMessage.AddCc("terrygjacobs@gmail.com");
#endif
#if !DEBUG
            recipients.Add(email);
            myMessage.AddCc(from);
#endif

            myMessage.AddTo(recipients);
            myMessage.AddBcc(new MailAddress("Take Home Tutor Appointment <terrygjacobs@gmail.com>"));
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
        }
        //public Tutor GetUserIdByUsername(string UserName)
        //{
        //    Tutor usr = new Tutor();
        //    SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
        //    myConnection.Open();
        //    SqlCommand myCommand = new SqlCommand("SELECT * FROM tutors WHERE email=@username", myConnection);
        //    myCommand.Parameters.AddWithValue("@username", UserName);
        //    SqlDataReader reader = myCommand.ExecuteReader();
        //    if (reader.HasRows)
        //    {
        //        while (reader.Read())
        //        {
        //            usr.ID = (Guid)reader["id"];
        //            usr.Email = reader["userid"].ToString();
        //        }
        //    }
        //    reader.Close();
        //    myConnection.Close();
        //    return usr;
        //}

        //public Tutor GetUsernameByUserId(Guid UserId)
        //{
        //    Tutor usr = new Tutor();
        //    SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
        //    myConnection.Open();
        //    SqlCommand myCommand = new SqlCommand("SELECT * FROM users WHERE id=@userid", myConnection);
        //    myCommand.Parameters.AddWithValue("@userid", UserId);
        //    SqlDataReader reader = myCommand.ExecuteReader();
        //    if (reader.HasRows)
        //    {
        //        while (reader.Read())
        //        {
        //            usr.Id = (Guid)reader["id"];
        //            usr.UserId = reader["userid"].ToString();
        //            usr.Email = reader["email"].ToString();
        //        }
        //    }
        //    reader.Close();
        //    myConnection.Close();
        //    return usr;
        //}

        public Tutor RegisterUser(string User, string Email, string Password)
        {
            Guid newId = Guid.NewGuid();
            Tutor usr = new Tutor();

            string mySalt = BCryptHelper.GenerateSalt();
            string myHash = BCryptHelper.HashPassword(Password, mySalt);

            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            myConnection.Open();
            SqlCommand myCommand = new SqlCommand("INSERT INTO users (id, email, userid, password, LastLogIn) " +
                         "Values (@id,@email,@userid,@password,@date)", myConnection);
            myCommand.Parameters.AddWithValue("@id", newId);
            myCommand.Parameters.AddWithValue("@email", Email);
            myCommand.Parameters.AddWithValue("@userid", User);
            myCommand.Parameters.AddWithValue("@password", myHash);
            myCommand.Parameters.AddWithValue("@date", DateTime.Today);
            myCommand.ExecuteNonQuery();
            myConnection.Close();

            usr = LogIn(User, Password);

            return usr;
        }


        public void AcceptTerms(Tutor tut)
        {
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            myConnection.Open();
            SqlCommand myCommand = new SqlCommand("INSERT INTO Terms (TutorId, AcceptedTerms, Date) " +
                         "Values (@id,@accept,@date)", myConnection);
            myCommand.Parameters.AddWithValue("@id", tut.ID);
            myCommand.Parameters.AddWithValue("@accept", true);
            myCommand.Parameters.AddWithValue("@date", DateTime.Now);
            myCommand.ExecuteNonQuery();
            myConnection.Close();
        }

        public Tutor LogIn(string Username, string Password)
        {
            Tutor usr = new Tutor();
            if (HttpContext.Current.Session["User"] == null)
            {
                string mySalt = BCryptHelper.GenerateSalt();
                string myHash = BCryptHelper.HashPassword(Password, mySalt);

                bool match = false;
                string password = "";
                bool exists = true;
                SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
                myConnection.Open();

                SqlCommand myCommand = new SqlCommand("SELECT * FROM Tutors WHERE email=@username", myConnection);
                myCommand.Parameters.AddWithValue("@username", Username);
                SqlDataReader reader = myCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        usr.Email = (string)reader["email"];
                        usr.ID = new Guid(reader["id"].ToString());
                        password = reader["password"].ToString();
                    }
                }
                else
                {
                    exists = false;
                }

                reader.Close();

                if (password != "")
                {
                    match = BCryptHelper.CheckPassword(Password, password);
                }

                if (match)
                {
                    usr = GetTutorsById(usr.ID);
                    System.Web.HttpContext.Current.Session.Add("User", usr);
                    FormsAuthentication.SetAuthCookie(usr.BoardUser, false);
                }
                else if (password == "" && exists)
                {
                    usr = GetTutorsById(usr.ID);
                    SqlCommand myCmd = new SqlCommand("UPDATE Tutors " +
                                 "SET password = @password " +
                                 "WHERE email = @user", myConnection);
                    myCmd.Parameters.AddWithValue("@password", myHash);
                    myCmd.Parameters.AddWithValue("@user", Username);
                    myCmd.ExecuteNonQuery();
                    System.Web.HttpContext.Current.Session.Add("User", usr);
                    FormsAuthentication.SetAuthCookie(usr.BoardUser, false);
                }
                else
                {
                    usr = new Tutor();
                }
                myConnection.Close();
            }
            else
            {
                var tut = System.Web.HttpContext.Current.Session["User"];
                usr = (Tutor)tut;
            }

            return usr;
        }
    }
}