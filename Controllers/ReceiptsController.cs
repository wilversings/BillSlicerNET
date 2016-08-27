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
    public class ReceiptsController : Controller {

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
            var split = dbContext.Checkages.Where (c => c.Receipt.ID == id && c.Product.ID == subItemId).FirstOrDefault ();
            dbContext.Checkages.Remove (split);
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
            dbContext.Checkages.Add (new Models.Split {
                Product = product,
                Receipt = receipt,
                Quantity = 1,
                SplitString = "".PadLeft (receipt.Room.Users.Count, '0')
            });
            dbContext.SaveChanges ();

            return new System.Web.Mvc.HttpStatusCodeResult (System.Net.HttpStatusCode.Created);

        }

        private decimal[] setCheckOuts (IEnumerable<Split> splits) {

            int payerNumber = getCurrentUser ().Room.Users.Count;
            var checkout = new decimal[payerNumber] ;

            for (int jndex = 0; jndex < payerNumber; ++jndex) {
                checkout[jndex] = 0;
            }

            int index = 0;
            foreach (Split split in splits) {

                decimal average = 0;

                int checkedNumber = 0;
                foreach(char c in split.SplitString) {
                    if (c == '1') {
                        ++checkedNumber;
                    }
                }
                if (checkedNumber == 0)
                    continue;
                average = split.Product.Price * split.Quantity;
                average /= checkedNumber;
                for (int jndex = 0; jndex < split.SplitString.Count(); ++jndex) {
                    if (split.SplitString[jndex] == '1') {
                        checkout[jndex] += average;
                    }
                }

                ++index;
            }
            
            return checkout;

        }

        [Authorize]
        public ActionResult Split (int id) {

            var receipt = dbContext.Receipts.FirstOrDefault (x => x.ID == id);
            var checkages = dbContext.Checkages.Where (x => x.Receipt.ID == id);

            var currentUser = getCurrentUser ();

            var roommates = currentUser.Room.Users;

            ViewBag.ReceiptId = id;
            List<Split> splitCheckages = new List<Split> ();

            ViewBag.Payers = new List<string> ();
            foreach (ApplicationUser roommate in roommates) {
                string firstName = roommate.FullName.Split (new char[] { ' ' }).First ();
                ViewBag.Payers.Add (roommate.FullName);
            }

            foreach (var checkage in checkages) {
                checkage.SplitDecoded = DecodeSplit (checkage.SplitString, roommates.Count);
            }

            ViewBag.Checkout = setCheckOuts (checkages);

            return View ("Split", checkages.ToArray());

        }

        [Authorize]
        public ActionResult Check (int id, int subItemId, bool chk, int index) {

            var split = dbContext.Checkages
                .Where (c => c.Receipt.ID == id && c.Product.ID == subItemId).FirstOrDefault ();

            var strSplit = new StringBuilder (split.SplitString);

            strSplit[index] = chk ? '1' : '0';
            split.SplitString = strSplit.ToString ();
            dbContext.SaveChanges ();

            return Json (new {
                data = setCheckOuts (dbContext.Checkages.Where (c => c.Receipt.ID == id))
            }, JsonRequestBehavior.AllowGet);

        }

        [Authorize]
        public ActionResult ChangeQuantity (int id, int subItemId, decimal quantity) {

            var split = dbContext.Checkages
                .Where (c => c.Receipt.ID == id && c.Product.ID == subItemId).FirstOrDefault ();

            split.Quantity = quantity;
            dbContext.SaveChanges ();

            //setCheckOuts (dbContext.Checkages.Where(c => c.Receipt.ID == id));

            return Json (new {
                data = setCheckOuts (dbContext.Checkages.Where (c => c.Receipt.ID == id))
            }, JsonRequestBehavior.AllowGet);

        }

        
    }
}