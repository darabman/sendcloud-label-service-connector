using SendCloudApi.Net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SendCloudApi.Net;

namespace LabelServiceConnector.WebApi
{
    public class SendCloudWebClient : IWebClient
    {
        private SendCloudClient _client;

        public SendCloudWebClient(string endPoint, string apiKey, string secret)
        {
            _client = new SendCloudClient(apiKey, secret);
        }

        public Task<Parcel<Country>> CreateParcel(CreateParcel createParcel)
        {
            return _client.Parcels.Create(createParcel);
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
    }
}
