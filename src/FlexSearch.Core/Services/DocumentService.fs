﻿// ----------------------------------------------------------------------------
// (c) Seemant Rajvanshi, 2013
//
// This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
// copy of the license can be found in the License.txt file at the root of this distribution. 
// By using this source code in any fashion, you are agreeing to be bound 
// by the terms of the Apache License, Version 2.0.
//
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------
namespace FlexSearch.Core

open FlexSearch.Api
open FlexSearch.Api.Message
open FlexSearch.Core
open FlexSearch.Utility
open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Data
open System.IO
open System.Linq
open System.Threading
open System.Threading.Tasks
open System.Threading.Tasks.Dataflow
open java.io
open java.util
open org.apache.lucene.analysis
open org.apache.lucene.analysis.core
open org.apache.lucene.analysis.miscellaneous
open org.apache.lucene.analysis.util
open org.apache.lucene.codecs
open org.apache.lucene.codecs.lucene42
open org.apache.lucene.document
open org.apache.lucene.index
open org.apache.lucene.search
open org.apache.lucene.store

[<AutoOpen>]
[<RequireQualifiedAccess>]
/// Exposes high level operations that can performed across the system.
/// Most of the services basically act as a wrapper around the functions 
/// here. Care should be taken to not introduce any mutable state in the
/// module but to only pass mutable state as an instance of NodeState
module DocumentService = 
    /// <summary>
    /// Service wrapper around all document related services
    /// </summary>
    /// <param name="state"></param>
    type Service(nodeState : INodeState, searchService : ISearchService) = 
        
        /// <summary>
        /// Get a document by Id
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        let GetDocument indexName documentId = 
            maybe { 
                let! flexIndex = nodeState.IndicesState.GetRegisteration(indexName)
                let q = new SearchQuery(indexName, (sprintf "%s = '%s'" Constants.IdField documentId))
                q.ReturnScore <- false
                q.ReturnFlatResult <- true
                q.Columns.Add("*")
                match searchService.Search(flexIndex, q) with
                | Choice1Of2(v') -> 
                    let result = v'.Documents.First().Fields
                    return! Choice1Of2(result)
                | Choice2Of2(e) -> return! Choice2Of2(e)
            }
        
        /// <summary>
        /// Get top 10 document from the index
        /// </summary>
        let GetDocuments indexName = 
            maybe { 
                let! flexIndex = nodeState.IndicesState.GetRegisteration(indexName)
                let q = new SearchQuery(indexName, (sprintf "%s matchall 'x'" Constants.IdField))
                q.ReturnScore <- false
                q.ReturnFlatResult <- true
                q.Columns.Add("*")
                q.MissingValueConfiguration.Add(Constants.IdField, MissingValueOption.Ignore)
                match searchService.Search(flexIndex, q) with
                | Choice1Of2(v') -> 
                    let result = v'.Documents |> Seq.map (fun x -> x.Fields)
                    return! Choice1Of2(result.ToList())
                | Choice2Of2(e) -> return! Choice2Of2(e)
            }
        
        /// <summary>
        /// Add or update an existing document
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <param name="fields"></param>
        let AddorUpdateDocument indexName documentId fields = 
            maybe { 
                let! (flexIndex, documentTemplate) = Index.indexExists (nodeState.IndicesState, indexName)
                let (targetIndex, documentTemplate) = 
                    Index.updateDocument (flexIndex, documentTemplate, documentId, 1, fields)
                flexIndex.Shards.[targetIndex]
                    .TrackingIndexWriter.updateDocument(new Term(Constants.IdField, documentId), 
                                                        documentTemplate.Document) |> ignore
            }
        
        /// <summary>
        /// Add a new document to the index
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        /// <param name="fields"></param>
        let AddDocument indexName documentId fields = 
            maybe { 
                let! (flexIndex, documentTemplate) = Index.indexExists (nodeState.IndicesState, indexName)
                let (targetIndex, documentTemplate) = 
                    Index.updateDocument (flexIndex, documentTemplate, documentId, 1, fields)
                flexIndex.Shards.[targetIndex].TrackingIndexWriter.addDocument(documentTemplate.Document) |> ignore
            }
        
        /// <summary>
        /// Delete a document by Id
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="documentId"></param>
        let DeleteDocument indexName documentId = 
            maybe { 
                let! (flexIndex, documentTemplate) = Index.indexExists (nodeState.IndicesState, indexName)
                let targetIndex = Document.mapToShard documentId flexIndex.Shards.Length
                flexIndex.Shards.[targetIndex]
                    .TrackingIndexWriter.deleteDocuments(new Term(Constants.IdField, documentId)) |> ignore
            }
        
        /// <summary>
        /// Delete all documents of an index
        /// </summary>
        /// <param name="indexName"></param>
        let DeleteAllDocuments indexName = 
            maybe { let! (flexIndex, documentTemplate) = indexExists (nodeState.IndicesState, indexName)
                    flexIndex.Shards |> Array.iter (fun shard -> shard.TrackingIndexWriter.deleteAll() |> ignore) }
        
        interface IDocumentService with
            member this.GetDocument(indexName, documentId) = GetDocument indexName documentId
            member this.GetDocuments indexName = GetDocuments indexName
            member this.AddOrUpdateDocument(indexName, documentId, fields) = 
                AddorUpdateDocument indexName documentId fields
            member this.AddDocument(indexName, documentId, fields) = AddDocument indexName documentId fields
            member this.DeleteDocument(indexName, documentId) = DeleteDocument indexName documentId
            member this.DeleteAllDocuments indexName = DeleteAllDocuments indexName