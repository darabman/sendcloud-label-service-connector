using SendCloudApi.Net.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LabelServiceConnector.WebApi
{
    internal class EmptyWebClient : IWebClient
    {
        public Task<Label> CreateLabel(int[] parcelId)
        {
            throw new NotImplementedException();
        }

        public Task<Parcel<Country>[]> CreateParcel(List<CreateParcel> createParcel)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> DownloadLabel(string url)
        {
            throw new NotImplementedException();
        }

        public Task<ShippingMethod[]> GetShippingMethods()
        {
            throw new NotImplementedException();
        }
    }
}
