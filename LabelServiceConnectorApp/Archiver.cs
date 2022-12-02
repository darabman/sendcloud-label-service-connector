using LabelServiceConnector.Lib.Data;
using LabelServiceConnector.Lib.Models;
using LabelServiceConnector.Lib.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LabelServiceConnector
{
    public class Archiver
    {
        private CancellationToken _cancel;

        private ILogger _logger;

        private Timer _timer;

        private const int ArchivePeriodHours = 24;

        private const int ArchiveAfterDays = 30;

        public Archiver(ILogger logger, CancellationToken cancel)
        {
            _cancel = cancel;
            _logger = logger;

            _cancel.Register(OnCancel);

            _timer = new Timer(OnTimerInterval, null, Timeout.Infinite, ArchivePeriodHours * 60 * 60 * 1000);
        }

        private void OnTimerInterval(object? state)
        {
            try
            {
                int recordsUpdated = ArchivePass();

                _logger.LogInformation($"Archive pass on {DateTime.Now:D} processed {recordsUpdated} shipment(s)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"General fault while trying to archive shipped parcels");
                _logger.LogDebug(ex.ToString() + $" {ex.Message}");
            }
        }

        public void Start()
        {
            _timer.Change(0, ArchivePeriodHours * 60 * 60 * 1000);
        }

        private void OnCancel()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private int ArchivePass()
        {
            IQueryable<ParcelRecordEntity> records;

            using (var archive = new ArchiveRecordContext())
            {
                var daysAgo = DateTime.Now.Subtract(TimeSpan.FromDays(ArchiveAfterDays));

                records = archive.ParcelRecords
                    .Where(r => r.Status == ParcelRecordEntity.DeliveryStatus.Pending)
                    .Where(r => r.ShipmentDate < daysAgo);

                var ep = Configuration.Api["EndPoint"] ?? string.Empty;
                var key = Configuration.Api["ApiKey"] ?? string.Empty;
                var secret = Configuration.Api["ApiSecret"] ?? string.Empty;

                IWebClient webClient = (ep == "None")
                    ? new EmptyWebClient()
                    : new SendCloudWebClient(ep, key, secret);

                var parcels = webClient.GetParcels(records.Select(r => r.Id).ToList()).Result;

                if (parcels.Length != records.Count())
                {
                    throw new SendCloudApi.Net.Exceptions.SendCloudException(
                        $"Error when retrieving shipped parcels: Expected {records.Count()}, received {parcels.Length}");
                }

                foreach (var p in parcels)
                {
                    var record = records.Single(r => r.Id == p.Id);

                    try
                    {
                        if (p.Status.Message == "Delivered")
                        {
                            var outDir = Configuration.Config["ArchiveOutputDir"] ?? "archive/";
                            var fullOutPath = Path.GetFullPath(outDir) + $"\\{p.TrackingNumber}.txt";
                            var recordText = JsonConvert.SerializeObject(p);

                            Directory.CreateDirectory(outDir);
                            File.WriteAllText(fullOutPath, recordText);

                            record.Status = ParcelRecordEntity.DeliveryStatus.Delivered;
                            record.ArchivedFilePath = fullOutPath;
                            record.ArchivedOn = DateTime.Now;

                            _logger.LogInformation($"Parcel with Tracking Number {p.TrackingNumber} has been archived to {fullOutPath}");
                        }
                        else
                        {
                            record.Status = ParcelRecordEntity.DeliveryStatus.NotDelivered;

                            _logger.LogInformation($"Parcel with Tracking Number {p.TrackingNumber} " +
                                                   $"has not been delivered after {ArchiveAfterDays} days " +
                                                   $"(Status: '{p.Status.Message}') " +
                                                   $"and so has not been archived.");
                            _logger.LogInformation("Please resolve the shipment with your label provider and then click 'Archive All Delivered Shipments' " +
                                                   "in the application");                            
                        }

                        archive.Update(record);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Fault while archiving parcel with Tracking Number {p.TrackingNumber} - {ex.Message}");
                    }
                }

                return archive.SaveChanges();
            }
        }
    }
}
