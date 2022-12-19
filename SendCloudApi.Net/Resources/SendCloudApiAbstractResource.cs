﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SendCloudApi.Net.Resources
{
    public abstract class SendCloudApiAbstractResource
    {
        protected readonly SendCloudClient Client;
        protected string HostUrl = "https://panel.sendcloud.sc/api/v2/";
        protected string Authorization;
        protected bool CreateRequest = true;
        protected bool GetRequest = true;
        protected bool UpdateRequest = true;
        protected bool DeleteRequest = false;
        protected string SingleResource = string.Empty;
        protected string ListResource = string.Empty;
        protected string CreateResource = string.Empty;
        protected string UpdateResource = string.Empty;
        protected string Resource = string.Empty;
        protected string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        protected SendCloudApiAbstractResource(SendCloudClient client)
        {
            Client = client;
            Authorization = client.GetBasicAuth();
        }

        protected async Task<ApiResponse<T>> Create<T>(string data)
        {
            if (CreateRequest)
            {
                return await Client.Create<T>($"{HostUrl}{CreateResource}", Authorization, data, SingleResource, DateTimeFormat);
            }
            return new ApiResponse<T>(System.Net.HttpStatusCode.MethodNotAllowed, default(T));
        }

        protected async Task<ApiResponse<T>> Get<T>(int? objectId = null, Dictionary<string, string> parameters = null)
        {
            if (GetRequest)
            {
                if (objectId.HasValue)
                {
                    return await Client.Get<T>($"{HostUrl}{Resource}/{objectId.Value}", Authorization, parameters, SingleResource, DateTimeFormat);
                }

                return await Client.Get<T>($"{HostUrl}{Resource}", Authorization, parameters, ListResource, DateTimeFormat);
            }
            return new ApiResponse<T>(System.Net.HttpStatusCode.MethodNotAllowed, default(T));
        }

        protected async Task<ApiResponse<T>> Update<T>(string data, int? objectId = null)
        {
            if (UpdateRequest)
            {
                if (objectId.HasValue)
                {
                    return await Client.Update<T>($"{HostUrl}{UpdateResource}/{objectId.Value}", Authorization, data, SingleResource, DateTimeFormat);
                }
                return await Client.Update<T>($"{HostUrl}{UpdateResource}", Authorization, data, SingleResource, DateTimeFormat);
            }
            return new ApiResponse<T>(System.Net.HttpStatusCode.MethodNotAllowed, default(T));
        }

        protected async Task<ApiResponse<T>> Delete<T>(int objectId)
        {
            if (DeleteRequest)
            {
                return await Client.Delete<T>($"{HostUrl}{Resource}/{objectId}", Authorization);
            }
            return new ApiResponse<T>(System.Net.HttpStatusCode.MethodNotAllowed, default(T));
        }

        protected async Task<byte[]> Download<T>(string url)
        {
            return await Client.Download(url);
        }
    }
}
