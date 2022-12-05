using System.Collections.Generic;
using System.Threading.Tasks;
using SendCloudApi.Net.Models;

namespace LabelServiceConnector.Lib.Web
{
    public interface IWebClient
    {
        public Task<Parcel<Country>[]> CreateParcels(CreateParcel createParcel);

        public Task<Label> CreateLabel(int[] parcelId);

        public Task<byte[]> DownloadLabel(string url);

        public Task<ShippingMethod[]> GetShippingMethods();

        public Task<Status[]> GetParcelStatuses();

        public Task<Parcel<Country>[]> GetParcels();

        public Task<Parcel<Country>[]> GetParcels(int status);

        public Task<Parcel<Country>[]> GetParcels(ICollection<int> ids);
    }
}
