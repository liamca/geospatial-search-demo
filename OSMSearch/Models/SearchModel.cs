using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSMSearch.Models
{
    public class SearchResults
    {
        public FacetResults Facets { get; set; }
        public IList<SearchResult> Results { get; set; }
        public int? Count { get; set; }
    }
    public class AutoCompleteItem
    {
        public string id { get; set; }
        public string desc { get; set; }
    }


}