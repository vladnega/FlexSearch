namespace FlexSearch.Api.Types
{
    using System.ComponentModel;
    using System.Net;
    using System.Runtime.Serialization;

    using ServiceStack.ServiceHost;

    [Api("Search")]
    [ApiResponse(HttpStatusCode.BadRequest, ApiDescriptionHttpResponse.BadRequest)]
    [ApiResponse(HttpStatusCode.InternalServerError, ApiDescriptionHttpResponse.InternalServerError)]
    [ApiResponse(HttpStatusCode.OK, ApiDescriptionHttpResponse.Ok)]
    [Route("/search/", "POST,GET", Summary = "Search for documents in the index", Notes = "")]
    [DataContract]
    public class SearchQuery
    {
        #region Fields

        private int count = 10;

        #endregion

        #region Constructors and Destructors

        public SearchQuery(string[] columns, string indexName, int count, SearchFilter query)
        {
            this.Columns = columns;
            this.IndexName = indexName;
            this.Count = count;
            this.Query = query;
        }

        public SearchQuery()
        {
        }

        #endregion

        #region Public Properties

        [DataMember(Order = 1)]
        [Description("Columns to return")]
        public string[] Columns { get; set; }

        [DataMember(Order = 2)]
        [ApiMember(Description = "Number of records to return", ParameterType = "query", IsRequired = true)]
        public int Count
        {
            get
            {
                return this.count;
            }

            set
            {
                this.count = value;
            }
        }

        [DataMember(Order = 3)]
        [Description("Text highlighting related options")]
        public HighlightOption Highlight { get; set; }

        [DataMember(Order = 4)]
        [ApiMember(Description = ApiDescriptionGlobalTypes.IndexName, ParameterType = "query", IsRequired = true)]
        public string IndexName { get; set; }

        [DataMember(Order = 5)]
        [ApiMember(
            Description = "Column name by which results should be sorted. By default results are sorted by relevance.",
            ParameterType = "query", IsRequired = true)]
        public string OrderBy { get; set; }

        [DataMember(Order = 6)]
        [Description("Query to execute")]
        public SearchFilter Query { get; set; }

        [DataMember(Order = 7)]
        [ApiMember(Description = "Number of records to skip. Useful for paging.", ParameterType = "query",
            IsRequired = true)]
        public int Skip { get; set; }

        #endregion
    }
}