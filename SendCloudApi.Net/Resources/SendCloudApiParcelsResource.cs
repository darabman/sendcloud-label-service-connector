﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SendCloudApi.Net.Helpers;
using SendCloudApi.Net.Models;
using SendCloudApi.Net.Wrappers;

namespace SendCloudApi.Net.Resources
{
    public class SendCloudApiParcelsResource : SendCloudApiAbstractResource
    {
        public SendCloudApiParcelsResource(SendCloudClient client) : base(client)
        {
            Resource = "parcels";
            CreateResource = "parcels";
            UpdateResource = "parcels";
            ListResource = "parcels";
            SingleResource = "parcel";
            DateTimeFormat = "dd-MM-yyyy HH:mm:ss";
        }

        public async Task<Parcel<Country>[]> BulkCreate(CreateParcel[] parcels)
        {
            var wrapper = new DataWrapper { Parcels = parcels };
            var apiResponse = await Client.Create<Parcel<Country>[]>($"{HostUrl}{Resource}", Authorization, JsonHelper.Serialize(wrapper, DateTimeFormat), ListResource, DateTimeFormat);
            return apiResponse.Data;
        }

        public async Task<Parcel<Country>> Create(CreateParcel parcel)
        {
            var wrapper = new DataWrapper { Parcel = parcel };
            var apiResponse = await Create<Parcel<Country>>(JsonHelper.Serialize(wrapper, DateTimeFormat));
            return apiResponse.Data;
        }

        public async Task<Parcel<Country>[]> Get(int? limit = null, int? offset = null, int? parcelStatus = null, string trackingNumber = null, string orderNumber = null, DateTime? updatedAfter = null, ICollection<int> ids = null)
        {
            var parameters = new Dictionary<string, string>();
            if (limit.HasValue)
                parameters.Add("limit", limit.Value.ToString());
            if (offset.HasValue)
                parameters.Add("offset", offset.Value.ToString());
            if (parcelStatus.HasValue)
                parameters.Add("parcel_status", parcelStatus.Value.ToString());
            if (!string.IsNullOrWhiteSpace(trackingNumber))
                parameters.Add("tracking_number", trackingNumber);
            if (!string.IsNullOrWhiteSpace(orderNumber))
                parameters.Add("order_number", orderNumber);
            if (updatedAfter.HasValue)
                parameters.Add("updated_after", updatedAfter.Value.ToString("yyyy-MM-ddTHH:mm:ss"));

            ids = ids ?? Array.Empty<int>();
            if (ids.Count > 0)
            {
                parameters.Add("ids", string.Join(',', ids));
            }

            PagedResponse<Parcel<Country>[]> response;
            var data = new List<Parcel<Country>>();
            
            do
            {
                response = (PagedResponse<Parcel<Country>[]>)await Get<Parcel<Country>[]>(parameters: parameters);
                data.AddRange(response.Data);

                if (response.NextPage != null)
                {
                    var newParams = response.NextPage.Split('?')[1];

                    parameters.Clear();

                    foreach (var bit in newParams.Split('&'))
                    {
                        var name = bit.Split('=')[0];
                        var value = Uri.UnescapeDataString(bit.Split('=')[1]);
                        parameters.Add(name, value);
                    }
                }
            }
            while (response.NextPage != null);

            return data.ToArray();
        }

        public async Task<Parcel<Country>> Get(int parcelId)
        {
            var apiResponse = await Get<Parcel<Country>>(parcelId);
            return apiResponse.Data;
        }

        public async Task<Parcel<Country>> Update(CreateParcel parcel)
        {
            var wrapper = new DataWrapper { Parcel = parcel };
            var apiResponse = await Update<Parcel<Country>>(JsonHelper.Serialize(wrapper, DateTimeFormat));
            return apiResponse.Data;
        }

        public async Task<ParcelCancel> Cancel(int parcelId)
        {
            var apiResponse = await Client.Create<ParcelCancel>($"{HostUrl}{Resource}/{parcelId}/cancel", Authorization, string.Empty, string.Empty, DateTimeFormat);
            return apiResponse.Data;
        }

        public async Task<string> GetReturnPortalUrl(int parcelId)
        {
            var apiResponse = await Client.Get<string>($"{HostUrl}{Resource}/{parcelId}/return_portal_url", Authorization, null, "url", DateTimeFormat);
            return apiResponse.Data;
        }
    }
}
