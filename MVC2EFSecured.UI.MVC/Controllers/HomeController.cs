using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using MVC2EFSecured.UI.MVC.Models;
using System.Net.Mail;
using System.Net;
using System;

namespace MVC2EFSecured.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactViewModels cvm)
        {
            //When a class has validation attributes, that validation should be checked BEFORE attempting to process any data
            if (!ModelState.IsValid)
            {
                //Send them back to the form by passing the object to the view,
                //the form returns with the original populated information
                return View(cvm);
            }

            //Only executes if the form(object) passes model validation


            #region Send Email functionality
            //Build the message - what we see when we receive the email
            //Step 1) Build the email message body(content for the email)
            string message = $"You have received an email from {cvm.Name} with the subject - {cvm.Subject}. Please respond to {cvm.Name}" +
                $"with your response to the following message: <br><cite>{cvm.Message}</cite>";

            //The MailMessage Object takes several parameters and has 3 overloads
            /*This instance in particular takes 4 parameters:
             from: admin@domain.com
             to: example@domain2.com
             subject: here is the subject
             */
            //Step 2) Create the MailMessage object and customize it
            MailMessage msg = new MailMessage(
                //From - your domain email (admin@YOURdomain.com)
                "admin@yourdomain.com",
                //TO - Where the email actually lands(should be sent to your personal email)
                "personalemail@email.com",
                //SUBJECT
                cvm.Subject,
                //BODY
                message
                );

            //MailMessage properties
            msg.IsBodyHtml = true;//Allow HTML formatting in the email (Message has HTML in it)
            msg.Priority = MailPriority.High;//The default is Normal
            //CC or BCC other recipients
            //msg.CC.ADD("Another2Email.com")
            //Response to the sender's email instead of our own SMTP client
            msg.ReplyToList.Add(cvm.Email);

            //Step 3) Create the SmtpClient that will send the email
            //The Client will need info from the host to route the email
            SmtpClient client = new SmtpClient("mail.yourdomain.com");
            //Client credentials (smarterASP requires your email and password)
            client.Credentials = new NetworkCredential("admin@yourdomain.com", "Password");
            client.Port = 8889;

            //Step 4) Attempt to send the email
            //It is possible that the email server is down or we have configuration issues,
            //so we want to encapsulate our code in a try/catch
            try
            {
                //Attempt to send the email
                client.Send(msg);
            }
            catch (Exception ex)
            {
                ViewBag.CustomerMessage = $"Sorry, something went wrong, please try again later or review the stacktrace<br>.{ex.StackTrace}";
                return View(cvm);
            }

            //If all goes well and the email is able to send, we will send them to a confirmation View
            //return View("Email Confirmation", cvm);

            ViewBag.CustomerMessage = "Message Sent";
            return View();
            #endregion

        }
    }
}
