using System.Collections.Generic;
using SendCloudApi.Net.Models;

namespace LabelServiceConnector.WebApi
{
    public interface IWebClient
    {
        public ShippingMethod[] GetShippingMethods();

        public ShippingMethod GetShippingMethod(string id);

        public Parcel<Country> CreateParcel(CreateParcel createParcel);

        public Parcel<Country> GetParcel(string id);
    }
}
