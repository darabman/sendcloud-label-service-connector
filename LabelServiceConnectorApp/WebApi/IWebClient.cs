using System.Collections.Generic;
using System.Threading.Tasks;
using SendCloudApi.Net.Models;

namespace LabelServiceConnector.WebApi
{
    public interface IWebClient
    {
        public Task<Parcel<Country>[]> CreateParcel(List<CreateParcel> createParcel);

        public Task<Label> CreateLabel(int[] parcelId);

        public Task<byte[]> DownloadLabel(string url);

        public Task<ShippingMethod[]> GetShippingMethods();
    }
}
