﻿// ----------------------------------------------------------------------------
// (c) Seemant Rajvanshi, 2014
//
// This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
// copy of the license can be found in the License.txt file at the root of this distribution. 
// By using this source code in any fashion, you are agreeing to be bound 
// by the terms of the Apache License, Version 2.0.
//
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------
namespace FlexSearch.Core

[<AutoOpen>]
module AnalysisExtensions = 
    open FlexSearch.Api
    open FlexSearch.Core
    open FlexSearch.Utility
    open System.Collections.Generic
    open System
    open org.apache.lucene.analysis
    open FlexSearch.Common
    
    // ----------------------------------------------------------------------------
    // FlexSearch related validation helpers
    // ----------------------------------------------------------------------------
    let private MustGenerateFilterInstance (factoryCollection : IFactoryCollection) 
        (propName : string, value : FlexSearch.Api.TokenFilter) = 
        match factoryCollection.FilterFactory.GetModuleByName(value.FilterName) with
        | Choice1Of2(instance) -> instance.Initialize(value.Parameters)
        | _ -> 
            Choice2Of2(Errors.FILTER_NOT_FOUND
                       |> GenerateOperationMessage
                       |> Append("Filter Name", propName))
    
    let private MustGenerateTokenizerInstance (factoryCollection : IFactoryCollection) 
        (propName : string, value : FlexSearch.Api.Tokenizer) = 
        match factoryCollection.TokenizerFactory.GetModuleByName(value.TokenizerName) with
        | Choice1Of2(instance) -> instance.Initialize(value.Parameters)
        | _ -> 
            Choice2Of2(Errors.TOKENIZER_NOT_FOUND
                       |> GenerateOperationMessage
                       |> Append("Tokenizer Name", propName))
    
    type FlexSearch.Api.TokenFilter with
        
        /// <summary>
        /// Build a FilterFactory from TokenFilter
        /// </summary>
        /// <param name="factoryCollection"></param>
        member this.Build(factoryCollection : IFactoryCollection) = 
            maybe { 
                do! ("FilterName", this) |> MustGenerateFilterInstance factoryCollection
                let! filterFactory = factoryCollection.FilterFactory.GetModuleByName(this.FilterName)
                do! filterFactory.Initialize(this.Parameters)
                return filterFactory
            }
        
        static member Build(filters : List<FlexSearch.Api.TokenFilter>, factoryCollection : IFactoryCollection) = 
            maybe { 
                let result = new List<IFlexFilterFactory>()
                for filter in filters do
                    let! filterFactory = filter.Build(factoryCollection)
                    result.Add(filterFactory)
                return result
            }
    
    type FlexSearch.Api.Tokenizer with
        /// <summary>
        /// Build a TokenizerFactory from Tokenizer
        /// </summary>
        /// <param name="factoryCollection"></param>
        member this.Build(factoryCollection : IFactoryCollection) = 
            maybe { 
                do! ("TokenizerName", this) |> MustGenerateTokenizerInstance factoryCollection
                let! tokenizerFactory = factoryCollection.TokenizerFactory.GetModuleByName(this.TokenizerName)
                do! tokenizerFactory.Initialize(this.Parameters)
                return tokenizerFactory
            }
    
    type FlexSearch.Api.Analyzer with
        
        /// <summary>
        /// Return an analyzer from analyzer properties
        /// </summary>
        /// <param name="analyzerName"></param>
        /// <param name="factoryCollection"></param>
        member this.Build(analyzerName : string, factoryCollection : IFactoryCollection) = 
            maybe 
                { 
                let! tokenizerFactory = this.Tokenizer.Build(factoryCollection)
                let! filters = FlexSearch.Api.TokenFilter.Build(this.Filters, factoryCollection)
                return (new CustomAnalyzer(tokenizerFactory, filters.ToArray()) :> org.apache.lucene.analysis.Analyzer) }
        
        /// <summary>
        /// Build a dictionary of analyzers from analyzer properties
        /// </summary>
        /// <param name="analyzersDict"></param>
        /// <param name="factoryCollection"></param>
        static member Build(analyzersDict : Dictionary<string, FlexSearch.Api.Analyzer>, 
                            factoryCollection : IFactoryCollection) = 
            maybe { 
                let result = new Dictionary<string, Analyzer>(StringComparer.OrdinalIgnoreCase)
                for analyzer in analyzersDict do
                    let! analyzerObject = analyzer.Value.Build(analyzer.Key, factoryCollection)
                    result.Add(analyzer.Key, analyzerObject)
                return result
            }
