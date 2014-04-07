/**
 * Autogenerated by Thrift Compiler (0.9.1)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace FlexSearch.Api.Connector
{
  public static class ConnectorConstants
  {
    public static FlexSearch.Api.Message.OperationMessage CONNECTION_NAME_NOT_FOUND = new FlexSearch.Api.Message.OperationMessage();
    public static FlexSearch.Api.Message.OperationMessage QUERY_NAME_NOT_FOUND = new FlexSearch.Api.Message.OperationMessage();
    static ConnectorConstants()
    {
      CONNECTION_NAME_NOT_FOUND.DeveloperMessage = "The requested connection name does not exist.";
      CONNECTION_NAME_NOT_FOUND.UserMessage = "The requested connection name does not exist.";
      CONNECTION_NAME_NOT_FOUND.ErrorCode = 20000;
      QUERY_NAME_NOT_FOUND.DeveloperMessage = "The requested query name does not exist.";
      QUERY_NAME_NOT_FOUND.UserMessage = "The requested query name does not exist.";
      QUERY_NAME_NOT_FOUND.ErrorCode = 20001;
    }
  }
}
