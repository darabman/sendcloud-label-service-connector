using SendCloudApi.Net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelServiceConnector.WebApi
{
    internal class EmptyWebClient : IWebClient
    {
        public Parcel<Country> CreateParcel(CreateParcel createParcel)
        {
            return new Parcel<Country>();
        }

        public Parcel<Country> GetParcel(string id)
        {
            return new Parcel<Country>();
        }

        public ShippingMethod GetShippingMethod(string id)
        {
            return new ShippingMethod();
        }

        public ShippingMethod[] GetShippingMethods()
        {
            return new ShippingMethod[0];
        }
    }
}
