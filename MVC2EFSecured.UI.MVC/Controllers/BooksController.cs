using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC2EFSecured.DATA.EF_2;

namespace MVC2EFSecured.UI.MVC.Controllers
{

    /*
     * Membership and Security
     * 2 pieces of Security:
     *      Authentication: checking UserName and Password against the DB. (Are you who you say you are?)
     *      Authorization: What you can do (access rules)
     * When arriving at a website - What kind of user am I? **Unauthenticated/Anonymous**
     *      Authorization (access rules); where are you authorized? --> Controller
     *      Hiding Links to activity where that user (role group) does NOT have access
     */

    //[Authorize] --allows ANY authenticated user access
    //[Authorize(Roles = "Admin")] --only Admins could have access to the following
    //[Authorize(Roles= "Admin, Customer")] --prevents ALL anonymous users (in this case) ONLY Admin and Customer are allowed

    //*********SEE THE CREATE/EDIT/DELETE GET and POST ACTIONS FOR SECURING FUNCTIONAL (DB CRUD) ACTIVITY***************
    public class BooksController : Controller
    {
        private BookStoreEntities db = new BookStoreEntities();

        //[AllowAnonymous] -- anonymous users would be allowed but unspecified roles would be declined access
        // GET: Books
        public ActionResult Index()
        {
            var books = db.Books.Include(b => b.BookRoyality).Include(b => b.BookStatus).Include(b => b.Genre).Include(b => b.Publisher);
            return View(books.ToList());
        }
        
        // GET: Books/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }
        [Authorize(Roles = "Admin")]
        // GET: Books/Create
        public ActionResult Create()
        {
            ViewBag.BookID = new SelectList(db.BookRoyalities, "BookID", "BookID");
            ViewBag.BookStatusID = new SelectList(db.BookStatuses, "BookStatusID", "BookStatusName");
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "GenreName");
            ViewBag.PublisherID = new SelectList(db.Publishers, "PublisherID", "PublisherName");
            return View();
        }

        [Authorize(Roles ="Admin")]
        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookID,ISBN,BookTitle,Description,GenreID,Price,UnitsSold,PublishDate,PublisherID,BookImage,IsSiteFeature,IsGenreFeature,BookStatusID")] Book book, HttpPostedFileBase imageUpload)
        {//The HttpPostedFileBase indentifier (name) must match the input name in the view (spelling, not casing)
            if (ModelState.IsValid)
            {
                //Only process the file IF all other validation has passed (Model.State.IsValid)

                #region FileUpload

                //Provide a default value in case the user does NOT provide an image
                string imageName = "noImage.png";

                //Check the input and process if its value is NOT null
                if (imageUpload != null)
                {
                    //Get the file name and save it to a variable (resuse the string declared for default)
                    imageName = imageUpload.FileName;
                    //Use the fileName and extract the Extension and save it to a variable (important for images)

                    //Create a List of valid extensions
                    string ext = imageName.Substring(imageName.LastIndexOf(".")); // return .extension
                    //Compare our extension to the List
                    string[] goodExts = new string[] { ".jpeg", ".jpg", ".png", ".gif"};
                    //If we have a VALID extension:
                    if (goodExts.Contains(ext.ToLower()))
                    {
                        //rename the file (conventially we do a guid) - if you need something more descriptive you can dynamically create a unique value
                        //You could take part of the book title and concatenate with DateTime.Now OR you could use part of the userID to define
                        //  --(if you go to these routes, make sure your datatype in SQL is big enough to accept it)
                        //EX: imageName = book.BookTitle.Substring(0,20) +DateTime.Now + ext; //20 chars of the Title plus the DateTime stamp

                        //Here use the GUID (Global Unique Identifier)
                        imageName = Guid.NewGuid() + ext;

                        //Save the image file to the webserver
                        imageUpload.SaveAs(Server.MapPath("~/Content/images/Books/" + imageName));
                    }

                    //Invalid extension, default back to the default NoImage.png
                    else
                    {
                        imageName = "noImage.png";
                    }
                }
                //No matter whether the file upload has a file in it or NOT, update the DB
                //  --with the correct imageName for the record
                book.BookImage = imageName;

                #endregion

                //Sending the information to EF
                db.Books.Add(book);
                //Saving the changes to DataBase
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookID = new SelectList(db.BookRoyalities, "BookID", "BookID", book.BookID);
            ViewBag.BookStatusID = new SelectList(db.BookStatuses, "BookStatusID", "BookStatusName", book.BookStatusID);
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "GenreName", book.GenreID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "PublisherID", "PublisherName", book.PublisherID);
            return View(book);
        }
        [Authorize(Roles = "Admin")]
        // GET: Books/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookID = new SelectList(db.BookRoyalities, "BookID", "BookID", book.BookID);
            ViewBag.BookStatusID = new SelectList(db.BookStatuses, "BookStatusID", "BookStatusName", book.BookStatusID);
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "GenreName", book.GenreID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "PublisherID", "PublisherName", book.PublisherID);
            return View(book);
        }

        [Authorize(Roles ="Admin")]
        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookID,ISBN,BookTitle,Description,GenreID,Price,UnitsSold,PublishDate,PublisherID,BookImage,IsSiteFeature,IsGenreFeature,BookStatusID")] Book book, HttpPostedFileBase imageUpload)
        {
            if (ModelState.IsValid)
            {
                //Only process the file IF all other validation has passed (Model.State.IsValid)

                #region FileUpload

                //Check the input and process if its value is NOT null
                if (imageUpload != null)
                {
                    //Get the file name and save it to a variable (resuse the string declared for default)
                    string imageName = imageUpload.FileName;
                    //Use the fileName and extract the Extension and save it to a variable (important for images)

                    //Create a List of valid extensions
                    string ext = imageName.Substring(imageName.LastIndexOf(".")); // return .extension
                    //Compare our extension to the List
                    string[] goodExts = new string[] { ".jpeg", ".jpg", ".png", ".gif" };
                    //If we have a VALID extension:
                    if (goodExts.Contains(ext.ToLower()))
                    {
                        //rename the file (conventially we do a guid) - if you need something more descriptive you can dynamically create a unique value
                        //You could take part of the book title and concatenate with DateTime.Now OR you could use part of the userID to define
                        //  --(if you go to these routes, make sure your datatype in SQL is big enough to accept it)
                        //EX: imageName = book.BookTitle.Substring(0,20) +DateTime.Now + ext; //20 chars of the Title plus the DateTime stamp

                        //Here use the GUID (Global Unique Identifier)
                        imageName = Guid.NewGuid() + ext;

                        //Save the image file to the webserver
                        imageUpload.SaveAs(Server.MapPath("~/Content/images/Books/" + imageName));

                        //Only if the file type is correct  will we save the file and update the record with the correct imageName ofr the record
                        book.BookImage = imageName;
                    }
                }//If there is no NEW image information, the existing image is kept (HiddenFor() in the view)

                #endregion

                //Pass the modified (updated version of the record to EF) - set the state to Modified
                db.Entry(book).State = EntityState.Modified;
                //UPDATE/SAVE changes to the DB
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookID = new SelectList(db.BookRoyalities, "BookID", "BookID", book.BookID);
            ViewBag.BookStatusID = new SelectList(db.BookStatuses, "BookStatusID", "BookStatusName", book.BookStatusID);
            ViewBag.GenreID = new SelectList(db.Genres, "GenreID", "GenreName", book.GenreID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "PublisherID", "PublisherName", book.PublisherID);
            return View(book);
        }
        [Authorize(Roles = "Admin")]
        // GET: Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            if(book.BooksAuthors.Count==0 && book.BookRoyality == null)
            {
                return View(book);
            }
            else
            {
                //send back to the index if the book is NOT deletable
                //(they must first remove the references in the BookRoyalities and/or BookAuthors Table(s)
                //Express to the user the WHY the book is not being deleted
                //This session variable will hang between web requests for 20min of inactivity OR browser closure
                //We would need to handle the nulling of this value after they return to the index page then move to ANY page
                Session["DeleteError"] = "The book selected for deletion is in use, please remove any reference to this book from other datatables...a$$hole";
                return RedirectToAction("Index");//appear as a page refresh
            }            
        }

        [Authorize(Roles ="Admin")]
        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
