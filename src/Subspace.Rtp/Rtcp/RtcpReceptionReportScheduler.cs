using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Subspace.Rtp.Rtcp
{
    public interface IRtcpReceptionReportScheduler
    {
        /// <summary>
        /// The RECOMMENDED value for a fixed minimum
        /// interval is 5 seconds.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.2
        /// </summary>
        TimeSpan ReportTransmissionInterval { get; }

        void SetSenderReport(RtcpSenderReportPacket senderReportRtcpPacket);
        void Track(RtpPacket rtpPacket);
    }

    public class RtcpReceptionReportScheduler : IRtcpReceptionReportScheduler
    {
        private readonly IRtcpClient _rtcpClient;
        private readonly Timer _timer;
        private readonly Random _random;

        private readonly ConcurrentDictionary<uint, RtcpReceiverReportPacket> _receiverReports = new ConcurrentDictionary<uint, RtcpReceiverReportPacket>();
        private readonly ConcurrentDictionary<uint, RtcpSenderReportPacket> _senderReports = new ConcurrentDictionary<uint, RtcpSenderReportPacket>();

        public RtcpReceptionReportScheduler(IRtcpClient rtcpClient)
        {
            ReportTransmissionInterval = TimeSpan.FromSeconds(5);
            _timer = new Timer(SendReport, null, ReportTransmissionInterval, ReportTransmissionInterval);
            _rtcpClient = rtcpClient;
            _random = new Random();
        }

        private async void SendReport(object state)
        {
            try
            {
                foreach (var (senderSsrc, senderReport) in _senderReports)
                {
                    if (!_receiverReports.TryGetValue(senderSsrc, out var receiverReport))
                    {
                        continue;
                    }

                    var sourceDescriptionPacket = new RtcpSourceDescriptionPacket
                    {
                        Chunks = new List<RtcpSourceDescriptionChunk>(1)
                        {
                            new RtcpSourceDescriptionChunk
                            {
                                SynchronizationSource = receiverReport.SynchronizationSource,
                                Items = new List<RtcpSourceDescriptionItem>(1)
                                {
                                    new RtcpSourceDescriptionItem
                                    {
                                        Type = SourceDescriptionType.CName,
                                        Text = Environment.MachineName
                                    }
                                }
                            }
                        }
                    };

                    await _rtcpClient.SendAsync(senderReport.RemoteEndPoint, receiverReport, sourceDescriptionPacket);
                }
                _senderReports.Clear();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception occurred {e}", nameof(RtcpReceptionReportScheduler));
            }
        }

        /// <summary>
        /// The RECOMMENDED value for a fixed minimum
        /// interval is 5 seconds.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.2
        /// </summary>
        public TimeSpan ReportTransmissionInterval { get; }

        public void SetSenderReport(RtcpSenderReportPacket senderReportRtcpPacket)
        {
            _senderReports.AddOrUpdate(senderReportRtcpPacket.SynchronizationSource, k => senderReportRtcpPacket,
                (k, v) => senderReportRtcpPacket);
        }

        public void Track(RtpPacket rtpPacket)
        {
            var receiverReport =
                _receiverReports.GetOrAdd(rtpPacket.SynchronizationSource, k => new RtcpReceiverReportPacket
                {
                    SynchronizationSource = (uint)_random.Next()
                });

            var receptionReport = new RtcpReceptionReport();

            receptionReport.SynchronizationSource = rtpPacket.SynchronizationSource;

            // todo: correct rtcp reception report

            if (_senderReports.TryGetValue(rtpPacket.SynchronizationSource, out var senderReport))
            {
                receptionReport.ExtendedHighestSequenceNumberReceived = 0;
                receptionReport.LastSRTimestamp = (uint)(senderReport.NtpTimestamp >> 16);
                receptionReport.DelaySinceLastSR = 0;
            }

            receptionReport.FractionLost = 0;
            receptionReport.CumulativeNumberOfPacketsLost = 0;
            receptionReport.ExtendedHighestSequenceNumberReceived = 0;
            receptionReport.InterarrivalJitter = 0;

            receiverReport.ReceptionReports = new List<RtcpReceptionReport>(1)
            {
                receptionReport
            };
        }
    }
}
