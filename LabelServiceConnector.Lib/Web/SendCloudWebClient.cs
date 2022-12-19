using SendCloudApi.Net.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SendCloudApi.Net;

namespace LabelServiceConnector.Lib.Web
{
    public class SendCloudWebClient : IWebClient
    {
        private SendCloudClient _client;

        public SendCloudWebClient(string endPoint, string apiKey, string secret)
        {
            _client = new SendCloudClient(apiKey, secret);
        }

        public Task<Parcel<Country>[]> CreateParcels(CreateParcel parcel)
        {
            return _client.Parcels.BulkCreate(new CreateParcel[] { parcel });
        }

        public Task<Label> CreateLabel(int[] parcelId)
        {   
            _client.Label.BulkCreate(parcelId).Wait();
            return _client.Label.Get(parcelId[0]);
        }

        public Task<byte[]> DownloadLabel(string url)
        {
            return _client.Download(url);
        }

        public Task<ShippingMethod[]> GetShippingMethods()
        {
            return _client.ShippingMethods.Get();
        }

        public Task<Status[]> GetParcelStatuses()
        {
            return _client.ParcelStatuses.Get();
        }

        public Task<Parcel<Country>[]> GetParcels()
        {
            return _client.Parcels.Get();
        }

        public Task<Parcel<Country>[]> GetParcels(int status)
        {
            return _client.Parcels.Get(parcelStatus: status);
        }

        public async Task<Parcel<Country>[]> GetParcels(ICollection<int> ids)
        {
            if (ids.Count == 0)
            {
                return Array.Empty<Parcel<Country>>();
            }

            return await _client.Parcels.Get(ids: ids);
        }
    }
}
