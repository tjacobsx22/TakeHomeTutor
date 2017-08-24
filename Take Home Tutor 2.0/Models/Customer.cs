using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Take_Home_Tutor_2._0.Models
{
    public class Customer
    {
        public Guid CustomerId { get; set; }
        public Guid TutorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string StripeId { get; set; }
        public string Description { get; set; }
        public string PlanId { get; set; }
        public string CouponId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string CardId { get; set; }

        public void Save()
        {
            //Get results
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["TakeHomeTutor"].ConnectionString);
            SqlCommand cmd = new SqlCommand();

            cmd.Connection = con;
            con.Open();

            cmd.CommandText = "usp_SaveNewCustomer";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CustomerId", (Guid)this.CustomerId);
            cmd.Parameters.AddWithValue("@TutorId", (Guid)this.TutorId);
            cmd.Parameters.AddWithValue("@FirstName", this.FirstName.ToString());
            cmd.Parameters.AddWithValue("@LastName", this.LastName.ToString());
            cmd.Parameters.AddWithValue("@Email", this.Email.ToString());
            cmd.Parameters.AddWithValue("@StripeId", this.StripeId.ToString());
            cmd.Parameters.AddWithValue("@Description", this.Description.ToString());
            cmd.Parameters.AddWithValue("@PlanId", this.PlanId.ToString());
            cmd.Parameters.AddWithValue("@CouponId", this.CouponId.ToString());
            cmd.Parameters.AddWithValue("@Address1", this.Address1.ToString());
            cmd.Parameters.AddWithValue("@Address2", this.Address2.ToString());
            cmd.Parameters.AddWithValue("@City", this.City.ToString());
            cmd.Parameters.AddWithValue("@State", this.State.ToString());
            cmd.Parameters.AddWithValue("@Zipcode", this.Zipcode.ToString());
            cmd.Parameters.AddWithValue("@CardId", this.CardId.ToString());
            cmd.ExecuteNonQuery();

            con.Close();
        }
    }
}
