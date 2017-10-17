using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using OSMSearch.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace OSMSearch
{
    public class Search
    {
        private static Uri _serviceUri;
        private static HttpClient _httpClient;
        private static SearchServiceClient _searchClient;
        private static ISearchIndexClient _indexClient;
        private static string indexName = "mapzen";

        public static string errorMessage;
        static Search()
        {
            try
            {
                // We will use REST since I am using autocomplete which is currently private preview and not yet in .NET SDK
                _serviceUri = new Uri("https://" + ConfigurationManager.AppSettings["SearchServiceName"] + ".search.windows.net");
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("api-key", ConfigurationManager.AppSettings["SearchServiceApiKey"]);

                string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
                string apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];

                // Create an HTTP reference to the catalog index
                _searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));
                _indexClient = _searchClient.Indexes.GetClient(indexName);

            }
            catch (Exception e)
            {
                errorMessage = e.Message.ToString();
            }
        }

        public DocumentSearchResult DoSearch(string searchText, double lat, double lon, string zoomLevel)
        {
            // Execute search based on query string
            try
            {

                double Radius = DistanceInMeters(zoomLevel);
                SearchParameters sp = new SearchParameters()
                {
                    SearchMode = SearchMode.Any,
                    Top = 1000,
                    // Limit results
                    Select = new List<String>() { "id", "tags", "location" },
                    // Add count
                    IncludeTotalResultCount = true,
                    // Add facets
                    Facets = new List<String>() { "tags, count:33" },
                    Filter = "geo.distance(location, geography'POINT(" + lon + " " + lat + ")') le " + Radius.ToString()
                };
                sp.ScoringProfile = "locationScoring";      // Use a scoring profile
                sp.ScoringParameters = new List<ScoringParameter>();
                sp.ScoringParameters.Add(new ScoringParameter("locationPt", GeographyPoint.Create(lat, lon)));




                return _searchClient.Indexes.GetClient(indexName).Documents.Search(searchText, sp);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        private static double DistanceInMeters(string zoomLevel)
        {
            // Uning a very rough representation get the max radius of this map
            // You can get very detailed using this https://msdn.microsoft.com/en-us/library/aa940990.aspx
            double radius = 0;
            if (zoomLevel == "1")
                radius = 78271.52;
            else if (zoomLevel == "2")
                radius = 39135.76;
            else if (zoomLevel == "3")
                radius = 19567.88;
            else if (zoomLevel == "4")
                radius = 9783.94;
            else if (zoomLevel == "5")
                radius = 4891.97;
            else if (zoomLevel == "6")
                radius = 2445.98;
            else if (zoomLevel == "7")
                radius = 1222.99;
            else if (zoomLevel == "8")
                radius = 611.50;
            else if (zoomLevel == "9")
                radius = 305.75;
            else if (zoomLevel == "10")
                radius = 152.87;
            else if (zoomLevel == "11")
                radius = 76.44;
            else if (zoomLevel == "12")
                radius = 38.22;
            else if (zoomLevel == "13")
                radius = 19.11;
            else if (zoomLevel == "14")
                radius = 9.55;
            else if (zoomLevel == "15")
                radius = 4.78;
            else if (zoomLevel == "16")
                radius = 2.39;
            else if (zoomLevel == "17")
                radius = 1.19;
            else if (zoomLevel == "18")
                radius = 0.60;
            else if (zoomLevel == "19")
                radius = 0.30;

            // multiply by # of pixels (which is ~ 550) and convert m to km
            return radius * 550 / 1000;

        }

        public dynamic AutoComplete(string searchText, bool fuzzy)
        {
            // Pass the specified suggestion text and return the fields

            Uri uri = new Uri(_serviceUri, "/indexes/" + indexName + "/docs/autocomplete?suggesterName=sg&$top=7&autoCompleteMode=oneterm&search=" + Uri.EscapeDataString(searchText));
            HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(_httpClient, HttpMethod.Get, uri);
            AzureSearchHelper.EnsureSuccessfulSearchResponse(response);
            List<AutoCompleteItem> aciList = new List<AutoCompleteItem>();

            foreach (var option in AzureSearchHelper.DeserializeJson<dynamic>(response.Content.ReadAsStringAsync().Result).value)
            {
                AutoCompleteItem aci = new AutoCompleteItem();
                aci.id = (string)option["id"];
                aci.desc = (string)option["queryPlusText"];
                aciList.Add(aci);
            }

            return aciList;
        }

    }
}