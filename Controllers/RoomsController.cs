using BillSlicer.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BillSlicer.Controllers
{
    public class RoomsController : Controller
    {
        ApplicationDbContext dbContext;

        public RoomsController () {
            dbContext = new ApplicationDbContext ();
        }

        public ActionResult Index () {
            return RedirectToAction ("List");
        }

        [Authorize]
        public ActionResult List()
        {
            return View(dbContext.Rooms);
        }

        [Authorize]
        public ActionResult Create () {
            return View ();
        }

        [HttpPost]
        public ActionResult Create (Room newRoom) {

            dbContext.Rooms.Add (newRoom);
            dbContext.SaveChanges ();
            return RedirectToAction ("List");

        }

        [Authorize]
        public ActionResult Delete (int id) {

            dbContext.Rooms.Remove (dbContext.Rooms.First (x => x.ID == id));
            dbContext.SaveChanges ();
            return RedirectToAction("List");

        }

        [Authorize]
        public ActionResult Enroll (int id) {
            
            string currentUserID = User.Identity.GetUserId ();
            ApplicationUser currentUser = dbContext.Users.First (x => x.Id == currentUserID);
            currentUser.Room = dbContext.Rooms.First (x => x.ID == id);
            dbContext.SaveChanges ();
            return RedirectToAction ("List");

        }

    }
}