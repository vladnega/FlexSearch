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
namespace FlexSearch.Core.HttpHandlers

open FlexSearch.Api
open FlexSearch.Core
open FlexSearch.Core.HttpHelpers

/// <summary>
///  Get a filter list by Id
/// </summary>
/// <method>GET</method>
/// <uri>/filterlists/:id</uri>
/// <resource>filterlist</resource>
/// <id>get-filterlist-by-id</id>
[<Name("GET-/filterlists/:id")>]
[<Sealed>]
type GetFilterListByIdHandler(resourceService : IResourceService) = 
    inherit HttpHandlerBase<unit, FilterList>()
    override this.Process(id, subId, body, context) = 
        (resourceService.GetResource<FilterList>(subId.Value), Ok, NotFound)

/// <summary>
///  Create or update a filter list by Id
/// </summary>
/// <method>PUT</method>
/// <uri>/filterlists/:id</uri>
/// <resource>filterlist</resource>
/// <id>update-filterlist-by-id</id>
[<Name("PUT-/filterlists/:id")>]
[<Sealed>]
type PutFilterListByIdIdHandler(resourceService : IResourceService) = 
    inherit HttpHandlerBase<FilterList, unit>()
    override this.Process(id, subId, body, context) = 
        (resourceService.UpdateResource(subId.Value, body.Value), Ok, BadRequest)

/// <summary>
///  Delete a filter list by Id
/// </summary>
/// <method>DELETE</method>
/// <uri>/filterlists/:id</uri>
/// <resource>filterlist</resource>
/// <id>delete-filterlist-by-id</id>
[<Name("DELETE-/filterlists/:id")>]
[<Sealed>]
type DeleteFilterListByIdHandler(resourceService : IResourceService) = 
    inherit HttpHandlerBase<unit, unit>()
    override this.Process(id, subId, body, context) = 
        (resourceService.DeleteResource<FilterList>(subId.Value), Ok, BadRequest)

/// <summary>
///  Get a map list by Id
/// </summary>
/// <method>GET</method>
/// <uri>/maplists/:id</uri>
/// <resource>maplist</resource>
/// <id>get-maplist-by-id</id>
[<Name("GET-/maplists/:id")>]
[<Sealed>]
type GetMapListByIdHandler(resourceService : IResourceService) = 
    inherit HttpHandlerBase<unit, MapList>()
    override this.Process(id, subId, body, context) = (resourceService.GetResource<MapList>(subId.Value), Ok, NotFound)

/// <summary>
///  Create or update a map list by Id
/// </summary>
/// <method>GET</method>
/// <uri>/maplists/:id</uri>
/// <resource>maplist</resource>
/// <id>create-maplist-by-id</id>
[<Name("PUT-/maplists/:id")>]
[<Sealed>]
type PutMapListByIdIdHandler(resourceService : IResourceService) = 
    inherit HttpHandlerBase<MapList, unit>()
    override this.Process(id, subId, body, context) = 
        (resourceService.UpdateResource(subId.Value, body.Value), Ok, BadRequest)

/// <summary>
///  Delete a map list by Id
/// </summary>
/// <method>DELETE</method>
/// <uri>/maplists/:id</uri>
/// <resource>maplist</resource>
/// <id>delete-maplist-by-id</id>
[<Name("DELETE-/maplists/:id")>]
[<Sealed>]
type DeleteMapListByIdHandler(resourceService : IResourceService) = 
    inherit HttpHandlerBase<unit, unit>()
    override this.Process(id, subId, body, context) = 
        (resourceService.DeleteResource<MapList>(subId.Value), Ok, BadRequest)
