﻿using PlasticReductionProject.DAL;
using PlasticReductionProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlasticReductionProject.Views.Calculator
{
    public class CalculatorController : Controller
    {
        private LinkDbContext db = new LinkDbContext();

        // GET: Calculator
        public ActionResult Calculator()
        {
            ViewBag.Page = "Calculator";
            return View();
        }

        // GET: Report
        public ActionResult Report()
        {

            if (HttpContext.Request.Cookies["UserCookie"] == null) {   
                var SessionCookie = new HttpCookie("UserCookie");
                SessionCookie.Values.Add("SessionId", Session.SessionID.ToString());
                Response.Cookies.Add(SessionCookie);
                HttpCookie cookie = HttpContext.Request.Cookies["UserCookie"];
                ViewBag.SessionCookie = cookie.Values["SessionId"];
            }
            else 
            {
                var SessionCookie = new HttpCookie(Session.SessionID.ToString());
                HttpCookie oldCookie = HttpContext.Request.Cookies["UserCookie"];
                string oldSessionId = oldCookie.Values["SessionId"].ToString();
                string currSessionId = Session.SessionID.ToString();
                string combinedSessionID = oldSessionId + "," + currSessionId; 
                oldCookie.Values.Add("SessionId",combinedSessionID);
                //SessionCookie.Values.Add("SessionIDs", "SessionId");
                HttpCookie cookie = HttpContext.Request.Cookies["UserCookie"];
                ViewBag.SessionCookie = oldCookie.Values["SessionId"];
                var counter = 0;
                ViewBag.CookieKey = "";
                foreach (var value in cookie.Values)
                {
                    ViewBag.CookieKey += value.ToString();
                   
                    counter += 1;
                } 
                //ViewBag.CookieKey = cookie.Value;
            }

            ViewBag.Page = "Report";
            return View();
        }

        public ActionResult Products()
        {
            ViewBag.Page = "Products";

            List<Product> ProductList = (List<Product>)(from productID in db.Products
                                                        select productID)
                                                        .ToList().Distinct().ToList();

            return View(ProductList);
        }
    }
}