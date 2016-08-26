using BillSlicer.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BillSlicer.Controllers
{
    public class ReceiptsController : Controller
    {
        public ApplicationDbContext dbContext;
        public ReceiptsController () {
            dbContext = new ApplicationDbContext ();
        }

        private List<bool> DecodeSplit (string split, int roomMateNumber) {

            var ans = new List<bool> ();

            if (split == null)
                split = "";

            split = split.PadRight (roomMateNumber, '0');

            foreach (char c in split) {
                if (c == '1')
                    ans.Add (true);
                else
                    ans.Add (false);
            }

            return ans;

        }

        private string SplitEncode (IList<bool> splitDecoded) {

            var ans = new StringBuilder ();
            foreach (bool check in splitDecoded) {
                if (check) {
                    ans.Append ('1');
                } else {
                    ans.Append ('0');
                }
            }

            return ans.ToString ();

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
        public ActionResult List()
        {
            ApplicationUser currentUser = getCurrentUser ();

            if (currentUser.Room.Receipts.Count == 0) {
                ViewBag.RoomName = currentUser.Room.Name;
                return View ("NoReceipts");
            }

            return View ("List", currentUser.Room.Receipts);
        }

        [Authorize]
        public ActionResult Create () {
            return View ();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create (Models.Receipt newReceipt) {

            ApplicationUser currentUser = getCurrentUser ();

            newReceipt.DateAdded = DateTime.Now;
            newReceipt.Room = currentUser.Room;
            newReceipt.Roommate = currentUser;

            dbContext.Receipts.Add (newReceipt);
            dbContext.SaveChanges ();
            return RedirectToAction ("List");

        }

        [Authorize]
        public ActionResult Delete (int id) {

            Receipt receipt = dbContext.Receipts.FirstOrDefault (x => x.ID == id);
            dbContext.Receipts.Remove (receipt);
            dbContext.SaveChanges ();
            return RedirectToAction ("List");

        }

        [Authorize]
        public ActionResult AddTo (int id) {

            var receipt = dbContext.Receipts.Where (r => r.ID == id).FirstOrDefault ();
            ViewBag.ReceiptId = id;
            return View ("AddTo", receipt.Products);

        }

        [System.Web.Http.HttpDelete]
        [Authorize]
        public ActionResult DeleteProduct (int id, int subItemId) {

            var receipt = dbContext.Receipts.Where (r => r.ID == id).FirstOrDefault ();
            receipt.Products.Remove (receipt.Products.Where (p => p.ID == subItemId).FirstOrDefault ());
            dbContext.SaveChanges ();

            return new System.Web.Mvc.HttpStatusCodeResult (System.Net.HttpStatusCode.Accepted);

        }

        [HttpPost]
        [Authorize]
        public ActionResult AddProduct (int id) { 

            int prodId = Int32.Parse(Request.Form["prodId"]);
            var receipt = dbContext.Receipts.Where (r => r.ID == id).FirstOrDefault ();
            var product = dbContext.Products.Where (p => p.ID == prodId).FirstOrDefault();
            receipt.Products.Add (product);

            dbContext.SaveChanges ();

            return new System.Web.Mvc.HttpStatusCodeResult (System.Net.HttpStatusCode.Created);

        }

        private void setCheckOuts (IEnumerable<Split> splits) {

            int payerNumber = splits.FirstOrDefault ().SplitDecoded.Count;
            var checkout = new decimal[payerNumber] ;

            for (int jndex = 0; jndex < payerNumber; ++jndex) {
                checkout[jndex] = 0;
            }

            int index = 0;
            foreach (Split split in splits) {

                decimal average = 0;

                int checkedNumber = 0;
                for (int jndex = 0; jndex < split.SplitDecoded.Count; ++jndex) {
                    if (split.SplitDecoded[jndex]) {
                        ++checkedNumber;
                    }
                }
                if (checkedNumber == 0)
                    continue;
                average = split.Product.Price * split.Quantity;
                average /= checkedNumber;
                for (int jndex = 0; jndex < split.SplitDecoded.Count; ++jndex) {
                    if (split.SplitDecoded[jndex]) {
                        checkout[jndex] += average;
                    }
                }

                ++index;
            }
            ViewBag.Checkout = checkout;

        }

        [Authorize]
        public ActionResult Split (int id) {

            var receipt = dbContext.Receipts.FirstOrDefault (x => x.ID == id);
            var checkages = dbContext.Checkages.Where (x => x.Receipt_Id == id);

            var currentUser = getCurrentUser ();

            var roommates = currentUser.Room.Users;

            List<Split> splitCheckages = new List<Split> ();

            ViewBag.Payers = new List<string> ();
            foreach (ApplicationUser roommate in roommates) {
                string firstName = roommate.FullName.Split (new char[] { ' ' }).First ();
                ViewBag.Payers.Add (roommate.FullName);
            }

            foreach (var checkage in checkages) {
                checkage.SplitDecoded = DecodeSplit (checkage.SplitString, roommates.Count);
            }

            setCheckOuts (checkages);

            return View ("Split", checkages.ToArray());


        }

        [Authorize]
        [HttpPost]
        public ActionResult Split (int id, IEnumerable <Split> newCheckage) {

            dbContext.Checkages.RemoveRange (dbContext.Checkages.Where (x => x.Receipt_Id == id));

            foreach (var checkage in newCheckage) {

                dbContext.Checkages.Add (new Split {
                    Receipt_Id = id,
                    Product_Id = checkage.Product_Id,
                    Quantity = checkage.Quantity,
                    SplitString = SplitEncode (checkage.SplitDecoded)
                });
            
            }

            dbContext.SaveChanges ();

            return RedirectToAction ("Split", new { id = id });

        }

        
    }
}