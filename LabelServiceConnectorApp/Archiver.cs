using Microsoft.Extensions.Logging;
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

        private const int ArchivePeriodHours = 24;

        public Archiver(ILogger logger, CancellationToken cancel)
        {
            _cancel = cancel;
            _logger = logger;
        }

        public void Run()
        {
            new Task(() =>
            {
                var archivePeriod = ArchivePeriodHours * 60 * 60 * 1000;
                
                while (!_cancel.IsCancellationRequested)
                {
                    try
                    {
                        ArchivePass();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"General fault while trying to archive shipped parcels");
                        _logger.LogDebug(ex.ToString() + $" {ex.Message}");

                        break;
                    }
                }

            }).Start();
        }

        private void ArchivePass()
        {
            using (var context = new ArchiveContext())
            {

            }
            /*
 * Archive process:
 *  On label create:
 *      add 'tracked' record
 *  run once per period (24h):
 *       check record logged > period (30 days)
 *       call tracking API for record, save as <tracking_number>.txt
 *       update record
 */
        }
    }
}
