using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Take_Home_Tutor_2._0.Hubs
{
    //public class TutorHub : Hub
    //{
    //    public void Send(string student)
    //    {
    //        Clients.All.recieveNewStudent(student);
    //    }
    //}
    //crap, how to do this dynamically????
    public class TutorHub : Hub
    {
        public void Send(string tutor, string student)
        {
            Clients.Group(tutor).recieveNewStudent(student);
        }

        public void Connect(string tutor, Guid sessionId)
        {
            Clients.Group(tutor).connectNewStudent(sessionId);
        }

        public void Accept(string tutor, string tutorId)
        {
            Clients.All.acceptNewStudent(tutorId);
        }

        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;

            Groups.Add(Context.ConnectionId, name);

            return base.OnConnected();
        }
    }


}