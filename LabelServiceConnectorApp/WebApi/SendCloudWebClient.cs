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

        public Parcel<Country> CreateParcel(CreateParcel createParcel)
        {
            throw new NotImplementedException();
        }

        public Parcel<Country> GetParcel(string id)
        {
            throw new NotImplementedException();
        }

        public ShippingMethod GetShippingMethod(string id)
        {
            throw new NotImplementedException();
        }

        public ShippingMethod[] GetShippingMethods()
        {
            throw new NotImplementedException();
        }

    }
}
