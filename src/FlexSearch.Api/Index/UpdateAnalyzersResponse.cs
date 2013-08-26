﻿namespace FlexSearch.Api.Index
{
    using System.Runtime.Serialization;

    using ServiceStack.ServiceInterface.ServiceModel;

    [DataContract]
    public class UpdateAnalyzersResponse
    {
        #region Public Properties

        [DataMember(Order = 1)]
        public ResponseStatus ResponseStatus { get; set; }

        #endregion
    }
}