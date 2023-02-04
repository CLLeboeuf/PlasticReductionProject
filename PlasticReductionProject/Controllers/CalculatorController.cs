﻿using Microsoft.Ajax.Utilities;
using PlasticReductionProject.DAL;
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

        private CalculatorResult cr
        { 
            get { return Session["CalculatorResults"] as CalculatorResult; }
            set { Session["CalculatorResults"] = value; }
        }
            
         
        // GET: Calculator
        public ActionResult Calculator()
        {
            
            cr = new CalculatorResult(5);
            List<int> usedRand = new List<int>();

            cr.Results.ForEach(x => {
                int counter = 0;
                int productCount = db.Products.Count();
                while(counter < 5)
                {
                var rand = Randomiser.RandomNumber(1, productCount);
                  if(!usedRand.Contains(rand))
                    {
                        usedRand.Add(rand);
                        var test = db.Products.Find(rand);
                        x.Product = test;
                        usedRand.Add(rand);
                        counter += 1;
                    }             
                }
            });
            ViewBag.QuestionCounter = "Question " + (this.cr.increment + 1).ToString() + " of " + this.cr.Results.Count().ToString();
            ViewBag.Page = "Calculator";
            return View(cr.Results.First());
        }

        //post results
        [HttpPost]
        public ActionResult Calculator(ProductResult result)
        {
            ProductResult toSave = this.cr.Results.ElementAt(this.cr.increment);
            PlasticType plasticType = db.PlasticTypes.Find(toSave.Product.Type);
            Product product = toSave.Product;
            toSave.Usage = result.Usage;
            toSave.PeriodUsed = result.PeriodUsed;
            toSave.PeriodRecycled = result.PeriodRecycled;
            toSave.AmountUsed = result.AmountUsed;
            toSave.AmountRecycled = result.AmountRecycled;

            PlasticScore IfFound = this.cr.PlasticScores.Where(x => x.Name.Equals(plasticType.Acronym)).FirstOrDefault();
            var score = calculateResultForProduct(result);
            var average = product.averageUtilisation;
            if (IfFound != null)
            {
                IfFound.Score += score;
                IfFound.Average += average;
            } else
            {
                this.cr.PlasticScores.Add(new PlasticScore(plasticType.Acronym, score, average));
            }
                   
            switch (product.Type)
            {
                case 1 :
                    cr.PPScore += score;
                    cr.PPAvg += average;
                    break;
                case 2:
                    cr.PPAScore += score;
                    cr.PPAAvg += average;
                    break;
                case 3:
                    cr.HDPEScore += score;
                    cr.HDPEAvg += average;
                    break;
                case 4:
                    cr.LDPEScore += score;
                    cr.LDPEAvg += average;
                    break;
                case 5:
                    cr.PVCScore += score;
                    cr.PVCAvg += average;
                    break;
                case 6:
                    cr.PETScore += score;
                    cr.PETAvg += average;
                    break;
                case 7:
                    cr.PSScore += score;
                    cr.PSAvg += average;
                    break;
                default:
                    cr.OtherScore += score;
                    cr.OtherAvg += average;
                    break;
            }
            this.cr.increment++;
            
            ViewBag.QuestionCounter = "Question " + (this.cr.increment + 1).ToString() + " of " + this.cr.Results.Count().ToString();

            if (this.cr.increment == 5)
            {
              //  db.SaveChanges();
               return RedirectToAction("Report");
            }

            return View(this.cr.Results.ElementAt(this.cr.increment));
        }

        private double calculateResultForProduct(ProductResult result)
        {
            //needs to be put in a function
            var usedMultiplier = 1;
            var recycledMultiplier = 1;
            switch (result.PeriodUsed.ToString())
            {
                case "Day":
                    usedMultiplier = 365;
                    break;
                case "Week":
                    usedMultiplier = 52;
                    break;
                case "Month":
                    usedMultiplier = 12;
                    break;
                default:
                    break;
            }
            switch (result.PeriodRecycled.ToString())
            {
                case "Day":
                    recycledMultiplier = 365;
                    break;
                case "Week":
                    recycledMultiplier = 52;
                    break;
                case "Month":
                    recycledMultiplier = 12;
                    break;
                default:
                    break;
            }

           // double recycleRate = db.PlasticTypes.Find(Id == 1).;

            double score = result.AmountUsed * usedMultiplier - result.AmountRecycled * recycledMultiplier;
            return score;
    
        }


        // GET: Report
        public ActionResult Report()
        {
            // addCookieToViewBag();
         
            ViewBag.Page = "Report";

            var totalScore = this.cr.HDPEScore + this.cr.LDPEScore + this.cr.OtherScore + this.cr.PETScore + this.cr.PPAScore + this.cr.PPScore
                + this.cr.PSScore + this.cr.PVCScore;
            var totalAvg = this.cr.HDPEAvg + this.cr.LDPEAvg + this.cr.OtherAvg + this.cr.PETAvg + this.cr.PPAAvg + this.cr.PPAvg
                + this.cr.PSAvg + this.cr.PVCAvg;

            var compScore = totalScore / totalAvg;

            List<Badge> badges = new List<Badge>();

            badges = db.Badges.ToList();

            if (compScore < 0.01)
            {
                ViewBag.Comment = badges.ElementAt(5).Comment.ToString();
                ViewBag.Image = badges.ElementAt(5).BadgeUrl.ToString();
            }
            else if (compScore < 0.05)
            {
                ViewBag.Comment = badges.ElementAt(4).Comment.ToString();
                ViewBag.Image = badges.ElementAt(4).BadgeUrl.ToString();
            }
            else if (compScore < 0.1)
            {
                ViewBag.Comment = badges.ElementAt(3).Comment.ToString();
                ViewBag.Image = badges.ElementAt(3).BadgeUrl.ToString();
            }
            else if (compScore < 0.3)
            {
                ViewBag.Comment = badges.ElementAt(2).Comment.ToString();
                ViewBag.Image = badges.ElementAt(2).BadgeUrl.ToString();
            }
            else if (compScore < 0.5)
            {
                ViewBag.Comment = badges.ElementAt(1).Comment.ToString();
                ViewBag.Image = badges.ElementAt(1).BadgeUrl.ToString();
            }



            return View(this.cr);
        }

        

        public ActionResult Products()
        {
            ViewBag.Page = "Products";
            

            List<Product> ProductList = (List<Product>)(from productID in db.Products
                                                        select productID)
                                                        .ToList().Distinct().ToList();

            return View(ProductList);
        }

        public ActionResult PlasticTypes()
        {
            ViewBag.Page = "PlasticTypes";


            List<PlasticType> PlasticTypeList = (List<PlasticType>)(from plastic_Id in db.PlasticTypes
                                                                      select plastic_Id)
                                                        .ToList().Distinct().ToList();

            return View(PlasticTypeList);
        }

        public void addCookieToViewBag()
        {

            if (HttpContext.Request.Cookies["UserCookie"] == null)
            {
                var SessionCookie = new HttpCookie("UserCookie");
                SessionCookie.Values.Add(Session.SessionID.ToString(), "SessionId");
                Response.Cookies.Add(SessionCookie);
                HttpCookie cookie = HttpContext.Request.Cookies["UserCookie"];
                ViewBag.SessionCookie = cookie.Values[0];
            }
            else
            {
                var SessionCookie = new HttpCookie(Session.SessionID.ToString());
                HttpCookie oldCookie = HttpContext.Request.Cookies["UserCookie"];
                string oldSessionId = oldCookie.Values["SessionId"].ToString();
                string currSessionId = Session.SessionID.ToString();
                string combinedSessionID = oldSessionId + "," + currSessionId;
                oldCookie.Values.Add("SessionId", combinedSessionID);
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
            }  
        }
    }
}