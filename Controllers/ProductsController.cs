using BillSlicer.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace BillSlicer.Controllers {
    public class ProductsController : Controller {

        private ApplicationDbContext dbContext;

        public ProductsController () {
            dbContext = new ApplicationDbContext ();
        }

        private ApplicationUser getCurrentUser () {
            string currentUserID = User.Identity.GetUserId ();
            return dbContext.Users.FirstOrDefault (x => x.Id == currentUserID);
        }

        [Authorize]
        public ActionResult Index () {
            return RedirectToAction ("List");
        }

        [Authorize]
        public ActionResult List () {

            ApplicationUser currentUser = getCurrentUser ();

            if (currentUser.Room.Products.Count == 0) {
                ViewBag.RoomName = currentUser.Room.Name;
                return View ("NoProducts");
            }

            return View ("List", currentUser.Room.Products);

        }

        [Authorize]
        public ActionResult Delete (int id) {

            Product product = dbContext.Products.First (x => x.ID == id);
            dbContext.Products.Remove (product);
            dbContext.SaveChanges ();
            return RedirectToAction ("List");

        }

        [Authorize]
        public ActionResult Edit (int id) {

            Product prod = dbContext.Products.First (x => x.ID == id);
            if (prod.Roommate.Id == User.Identity.GetUserId ()) {
                Product model = dbContext.Products.FirstOrDefault (x => x.ID == id);
                return View ("Edit", model);
            } else
                return View ("Error");

        }

        [HttpPost]
        public ActionResult Edit (Product modifiedProduct) {

            Product dbProduct = dbContext.Products.First (x => x.ID == modifiedProduct.ID);

            if (User.Identity.GetUserId () == dbProduct.Roommate.Id) {
                Product oldProduct = dbContext.Products.First (x => x.ID == modifiedProduct.ID);

                oldProduct.Name = modifiedProduct.Name;
                oldProduct.Price = modifiedProduct.Price;
                oldProduct.Description = modifiedProduct.Description;

                dbContext.SaveChanges ();

                return RedirectToAction ("List");
            }
            return View ("Error");

        }

        [Authorize]
        public ActionResult Create () {
            
            return View ();
            
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create (Product newProduct) {

            ApplicationUser currentUser = getCurrentUser ();

            newProduct.Roommate = currentUser;
            newProduct.Room = currentUser.Room;

            dbContext.Products.Add (newProduct);
            dbContext.SaveChanges ();

            return RedirectToAction ("List");
            
        }

       [Authorize]
       public ActionResult Search (string SearchTerm) {

            ApplicationUser currentUser = getCurrentUser ();

            SearchTerm = SearchTerm.ToLower ();
            var results = currentUser.Room.Products.Where (p => p.Name.ToLower ().Contains (SearchTerm))
                .Select (p => new {
                    id = p.ID,
                    name = p.Name
                });

            var jsonData = new {
                data = results
            };

            return Json (jsonData, JsonRequestBehavior.AllowGet);

        }

    }
}