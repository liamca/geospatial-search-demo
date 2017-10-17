using OSMSearch.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSMSearch.Controllers
{
    public class HomeController : Controller
    {
        private Search _Search = new Search();
        public ActionResult Index()
        {
            ViewBag.BingApiKey = ConfigurationManager.AppSettings["BingApiKey"];
            return View();
        }

        [HttpGet]
        public ActionResult AutoComplete(string term, bool fuzzy = true)
        {
            var response = _Search.AutoComplete(term, fuzzy);

            List<string> suggestions = new List<string>();
            foreach (var result in response)
            {
                suggestions.Add(result.desc);
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = suggestions
            };
        }

        public ActionResult Search(double lat, double lon, string zoomLevel, string q = "")
        {
            // If blank search, assume they want to search everything
            if (string.IsNullOrWhiteSpace(q))
                q = "*";

            var response = _Search.DoSearch(q, lat, lon, zoomLevel);
            if (response == null)
                return null;

            return new JsonResult
            {
                // ***************************************************************************************************************************
                // If you get an error here, make sure to check that you updated the SearchServiceName and SearchServiceApiKey in Web.config
                // ***************************************************************************************************************************
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new SearchResults() { Results = response.Results, Facets = response.Facets, Count = Convert.ToInt32(response.Count) }
            };
        }


    }
}