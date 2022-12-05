using LabelServiceConnector.Lib.Data;
using LabelServiceConnector.Lib.Models;
using LabelServiceConnector.Lib.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendCloudApi.Net.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LabelServiceConnector.Agents
{
    public class Archiver
    {
        private CancellationToken _cancel;

        private ILogger _logger;

        private Timer _timer;

        private readonly object _archiveLock = new object();

        

        public int DeliveredStatusKey { get; private set; }

        public Archiver(ILogger logger, CancellationToken cancel)
        {
            _cancel = cancel;
            _logger = logger;

            _cancel.Register(OnCancel);

            _timer = new Timer(OnTimerInterval, null, Timeout.Infinite, ArchiveRecordContext.ArchivePeriodHours * 60 * 60 * 1000);
        }

        public void Start()
        {
            _timer.Change(0, ArchiveRecordContext.ArchivePeriodHours * 60 * 60 * 1000);
        }

        public async void DownloadAll()
        {
            _logger.LogInformation("Manually downloading all 'Delivered' shipments..");

            int updated = 0;
            int added = 0;

            var ep = Configuration.Api["EndPoint"] ?? string.Empty;
            var key = Configuration.Api["ApiKey"] ?? string.Empty;
            var secret = Configuration.Api["ApiSecret"] ?? string.Empty;

            IWebClient webClient = ep == "None"
                ? new EmptyWebClient()
                : new SendCloudWebClient(ep, key, secret);

            var parcels = await webClient.GetParcels(DeliveredStatusKey);

            lock (_archiveLock)
            {
                using (var archive = new ArchiveRecordContext())
                {
                    foreach (var p in parcels)
                    {
                        var record = archive.ParcelRecords.FirstOrDefault(pr => pr.Id == p.Id);

                        if (record == default)
                        {
                            record = new ParcelRecordEntity()
                            {
                                Id = p.Id,
                                ArchivedOn = DateTime.Now,
                                ShipmentDate = p.DateCreated,
                                Status = ParcelRecordEntity.DeliveryStatus.Delivered,
                                ArchivedFilePath = ArchiveParcel(p),
                                TrackingNumber = p.TrackingNumber
                            };

                            archive.ParcelRecords.Add(record);

                            _logger.LogDebug($"Downloaded shipment '{record.TrackingNumber}' to {record.ArchivedFilePath}");

                            added++;
                        }
                        else
                        {
                            _logger.LogDebug($"Updating shipment '{record.TrackingNumber}'");

                            if (p.Status.Id != DeliveredStatusKey.ToString())
                            {
                                _logger.LogWarning($"Shipment with tracking number '{p.TrackingNumber}' " +
                                    $"was unexpectedly retrieved with '{p.Status.Message}' status, skipping..");

                                continue;
                            }

                            record.ArchivedOn = DateTime.Now;
                            record.ArchivedFilePath = ArchiveParcel(p);
                            record.Status = ParcelRecordEntity.DeliveryStatus.Delivered;

                            archive.Update(record);

                            

                            updated++;
                        }
                    }

                    archive.SaveChanges();
                }

                _logger.LogInformation($"Download finished with {added} record(s) added and {updated} records updated");
            }
        }

        public async Task UpdateDeliveredStatusKey()
        {
            var ep = Configuration.Api["EndPoint"] ?? string.Empty;
            var key = Configuration.Api["ApiKey"] ?? string.Empty;
            var secret = Configuration.Api["ApiSecret"] ?? string.Empty;

            IWebClient webClient = ep == "None"
                ? new EmptyWebClient()
                : new SendCloudWebClient(ep, key, secret);

            _logger.LogInformation("Retrieving status information from label provider");

            try
            {
                var statuses = await webClient.GetParcelStatuses();
                var deliveredStatus = statuses.Single(s => s.Message == "Delivered");

                DeliveredStatusKey = int.Parse(deliveredStatus.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve the 'Delivered' status key from label provider, using default ({DeliveredStatusKey})");
                _logger.LogError($"{ex} - {ex.Message}");
            }
        }

        private async void OnTimerInterval(object? state)
        {
            try
            {
                    var interval = DateTime.Now.Subtract(TimeSpan.FromDays(ArchiveRecordContext.ArchiveAfterDays));

                    _logger.LogInformation($"Archiving any delivered shipments created before {interval:f}");

                    int recordsUpdated = await ArchivePass();

                    _logger.LogInformation($"{recordsUpdated} shipment(s) archived");
            }
            catch (Exception ex)
            {
                _logger.LogError($"General fault while trying to archive shipped parcels");
                _logger.LogDebug(ex.ToString() + $" {ex.Message}");
            }
        }

        private void OnCancel()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private async Task<int> ArchivePass()
        {
            IQueryable<ParcelRecordEntity> records;

            using (var archive = new ArchiveRecordContext())
            {
                var daysAgo = DateTime.Now.Subtract(TimeSpan.FromDays(ArchiveRecordContext.ArchiveAfterDays));

                records = archive.ParcelRecords
                    .Where(r => r.Status == ParcelRecordEntity.DeliveryStatus.Pending)
                    .Where(r => r.ShipmentDate < daysAgo);

                var ep = Configuration.Api["EndPoint"] ?? string.Empty;
                var key = Configuration.Api["ApiKey"] ?? string.Empty;
                var secret = Configuration.Api["ApiSecret"] ?? string.Empty;

                IWebClient webClient = ep == "None"
                    ? new EmptyWebClient()
                    : new SendCloudWebClient(ep, key, secret);

                var parcels = await webClient.GetParcels(records.Select(r => r.Id).ToList());

                if (parcels.Length != records.Count())
                {
                    throw new SendCloudApi.Net.Exceptions.SendCloudException(
                        $"Error when retrieving shipped parcels: Expected {records.Count()}, received {parcels.Length}");
                }

                lock (_archiveLock)
                {
                    foreach (var p in parcels)
                    {
                        var record = records.Single(r => r.Id == p.Id);

                        try
                        {
                            if (p.Status.Id == DeliveredStatusKey.ToString())
                            {
                                var savedPath = ArchiveParcel(p);

                                record.Status = ParcelRecordEntity.DeliveryStatus.Delivered;
                                record.ArchivedFilePath = savedPath;
                                record.ArchivedOn = DateTime.Now;

                                _logger.LogInformation($"Parcel with Tracking Number {p.TrackingNumber} has been archived to {savedPath}");
                            }
                            else
                            {
                                record.Status = ParcelRecordEntity.DeliveryStatus.NotDelivered;

                                _logger.LogInformation($"Parcel with Tracking Number {p.TrackingNumber} " +
                                                       $"has not been delivered after {ArchiveRecordContext.ArchiveAfterDays} days " +
                                                       $"(Status: '{p.Status.Message}', Shipment Created '{p.DateCreated:f}') " +
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
                }

                return archive.SaveChanges();
            }
        }
        
        private string ArchiveParcel(Parcel<Country> p)
        {
            var outDir = (Configuration.Config["ArchiveOutputDir"] ?? "archive\\") +
                $"{p.DateCreated.Year.ToString("D4")}\\" +
                $"{p.DateCreated.Month.ToString("D2")}\\";
            var fullOutPath = Path.GetFullPath(outDir) + $"{p.TrackingNumber}.txt";
            var recordText = JsonConvert.SerializeObject(p, Formatting.Indented);

            //Warn here on overwrite?
            Directory.CreateDirectory(outDir);
            if (!File.Exists(fullOutPath))
            {
                File.WriteAllText(fullOutPath, recordText);
            }
            else
            {
                _logger.LogInformation($"File '{fullOutPath}' already exists, skipping..");
            }
            

            return fullOutPath;
        }
    }
}
