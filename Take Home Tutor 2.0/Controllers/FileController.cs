using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Take_Home_Tutor_2._0.Models;

namespace Take_Home_Tutor_2._0.Controllers
{
    public class FileController : Controller
    {
        // GET: File
        public FileResult Index(Guid id)
        {
            Tutor tutor = new Tutor();
            var tut = id == Guid.Empty ? System.Web.HttpContext.Current.Session["User"] : tutor.GetTutorsById(id);
            tutor = (Tutor)tut;
            var file = tutor.GetProfileImageById(tutor.ID);
            MemoryStream ms = new MemoryStream(file);
            Image returnImage = Image.FromStream(ms);
            Image resizedProfile = returnImage.GetThumbnailImage(200, 200, null, IntPtr.Zero);
            MemoryStream ms2 = new MemoryStream();
            resizedProfile.Save(ms2, ImageFormat.Bmp);
            return File(ms2.ToArray(), System.Net.Mime.MediaTypeNames.Application.Octet);
        }
    }
}