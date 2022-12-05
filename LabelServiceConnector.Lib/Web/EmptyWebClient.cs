using SendCloudApi.Net.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LabelServiceConnector.Lib.Web
{
    public class EmptyWebClient : IWebClient
    {
        public Task<Label> CreateLabel(int[] parcelId)
        {
            throw new NotImplementedException();
        }

        public Task<Parcel<Country>[]> CreateParcels(CreateParcel parcel)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> DownloadLabel(string url)
        {
            throw new NotImplementedException();
        }

        public Task<Parcel<Country>[]> GetParcels()
        {
            throw new NotImplementedException();
        }

        public Task<Parcel<Country>[]> GetParcels(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        public Task<Parcel<Country>[]> GetParcels(string status)
        {
            throw new NotImplementedException();
        }

        public Task<Parcel<Country>[]> GetParcels(int status)
        {
            throw new NotImplementedException();
        }

        public Task<Status[]> GetParcelStatuses()
        {
            throw new NotImplementedException();
        }

        public Task<ShippingMethod[]> GetShippingMethods()
        {
            throw new NotImplementedException();
        }
    }
}
