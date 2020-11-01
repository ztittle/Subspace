using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Subspace.Rtp.Rtcp
{
    public static class RtcpPacketParser
    {
        public static RtcpPacket ParseRtcpPacket(Span<byte> rawBytes)
        {
            var idx = 0;
            
            var startPosition = idx;

            var b1 = rawBytes[idx];
            idx += 1;

            var version = (byte)(b1 >> 6);

            if (version != 2)
            {
                throw new NotSupportedException($"Rtcp packet version '{version}' is not supported.");
            }

            var padding = (b1 >> 5 & 1) == 1;

            var sourceCount = (byte)(b1 & 0x1f);

            var packetType = (RtcpPacketType)rawBytes[idx];
            idx += 1;

            var packetLength = BinaryPrimitives.ReadUInt16BigEndian(rawBytes.Slice(idx));
            idx += 2;
            var packetLenBytes = (packetLength + 1) * 4;

            RtcpPacket rtcpPacket;

            switch (packetType)
            {
                case RtcpPacketType.SenderReport:
                    rtcpPacket = ParseRtcpSenderReportPacket(sourceCount, rawBytes, ref idx);
                    break;
                case RtcpPacketType.ReceiverReport:
                    rtcpPacket = ParseRtcpReceiverReportPacket(sourceCount, rawBytes, ref idx);
                    break;
                case RtcpPacketType.SourceDescription:
                    rtcpPacket = ParseRtcpSourceDescriptionPacket(sourceCount, rawBytes, ref idx);
                    break;
                case RtcpPacketType.Goodbye:
                    rtcpPacket = ParseRtcpGoodbyePacket(sourceCount, rawBytes, ref idx, startPosition, packetLenBytes);
                    break;
                case RtcpPacketType.ApplicationDefined:
                    rtcpPacket = ParseAppDefinedPacket(packetLenBytes, rawBytes, ref idx);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported packet type '{packetType}'");
            }

            rtcpPacket.Padding = padding;

            return rtcpPacket;
        }

        private static RtcpPacket ParseAppDefinedPacket(int packetLenBytes, Span<byte> bytes, ref int idx)
        {
            var rtcpPacket = new RtcpAppDefinedPacket();

            rtcpPacket.SynchronizationSource = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));
            idx += 4;

            rtcpPacket.Name = Encoding.ASCII.GetString(bytes.Slice(idx, 4));
            idx += 4;

            rtcpPacket.Data = bytes.Slice(idx, packetLenBytes - idx).ToArray();

            return rtcpPacket;
        }

        private static RtcpReceiverReportPacket ParseRtcpReceiverReportPacket(byte sourceCount, Span<byte> bytes, ref int idx)
        {
            var rtcpPacket = new RtcpReceiverReportPacket();

            rtcpPacket.SynchronizationSource = BinaryPrimitives.ReadUInt32BigEndian(bytes);
            idx += 4;

            rtcpPacket.ReceptionReports = new List<RtcpReceptionReport>(sourceCount);

            for (var i = 0; i < sourceCount; i++)
            {
                var rtcpReceptionReport = ParseRtcpReceptionReport(bytes, ref idx);

                rtcpPacket.ReceptionReports.Add(rtcpReceptionReport);
            }

            return rtcpPacket;
        }

        private static RtcpSenderReportPacket ParseRtcpSenderReportPacket(byte sourceCount, Span<byte> bytes, ref int idx)
        {
            var rtcpPacket = new RtcpSenderReportPacket();

            rtcpPacket.SynchronizationSource = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));

            idx += 4;

            rtcpPacket.NtpTimestamp = BinaryPrimitives.ReadUInt64BigEndian(bytes.Slice(idx));

            idx += 8;

            rtcpPacket.RtpTimestamp = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));

            idx += 4;

            rtcpPacket.SenderPacketCount = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));

            idx += 4;

            rtcpPacket.SenderOctetCount = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));

            idx += 4;

            rtcpPacket.ReceptionReports = new List<RtcpReceptionReport>(sourceCount);

            for (var i = 0; i < sourceCount; i++)
            {
                var rtcpReceptionReport = ParseRtcpReceptionReport(bytes, ref idx);

                rtcpPacket.ReceptionReports.Add(rtcpReceptionReport);
            }

            return rtcpPacket;
        }

        private static RtcpReceptionReport ParseRtcpReceptionReport(Span<byte> bytes, ref int idx)
        {
            var receptionReport = new RtcpReceptionReport();

            receptionReport.SynchronizationSource = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));
            idx += 4;
            receptionReport.FractionLost = bytes[idx];
            idx += 1;
            receptionReport.CumulativeNumberOfPacketsLost = (uint)(bytes[idx] << 16 | bytes[idx+1] << 8 | bytes[idx+2]);
            idx += 3;
            receptionReport.ExtendedHighestSequenceNumberReceived = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));
            idx += 4;
            receptionReport.InterarrivalJitter = BinaryPrimitives.ReadInt32BigEndian(bytes.Slice(idx));
            idx += 4;
            receptionReport.LastSRTimestamp = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));
            idx += 4;
            receptionReport.DelaySinceLastSR = BinaryPrimitives.ReadUInt32BigEndian(bytes.Slice(idx));
            idx += 4;

            return receptionReport;
        }

        private static RtcpSourceDescriptionPacket ParseRtcpSourceDescriptionPacket(byte sourceCount, Span<byte> bytes, ref int idx)
        {
            var rtcpPacket = new RtcpSourceDescriptionPacket();

            rtcpPacket.Chunks = new List<RtcpSourceDescriptionChunk>(sourceCount);

            for (var i = 0; i < sourceCount; i++)
            {
                var chunk = new RtcpSourceDescriptionChunk();
                chunk.SynchronizationSource = BinaryPrimitives.ReadUInt32BigEndian(bytes);
                idx += 4;
                chunk.Items = new List<RtcpSourceDescriptionItem>();

                var type = (SourceDescriptionType)bytes[idx];
                idx += 1;

                while (type != SourceDescriptionType.End)
                {
                    var item = new RtcpSourceDescriptionItem();
                    item.Type = type;
                    var textLen = bytes[idx];
                    idx += 1;
                    var textBytes = bytes.Slice(0, textLen).ToArray();
                    idx += textLen;
                    item.Text = Encoding.UTF8.GetString(textBytes);
                    chunk.Items.Add(item);

                    type = (SourceDescriptionType)bytes[idx];
                    idx += 1;
                }

                rtcpPacket.Chunks.Add(chunk);
            }

            return rtcpPacket;
        }

        private static RtcpGoodbyePacket ParseRtcpGoodbyePacket(byte sourceCount, Span<byte> bytes, ref int idx, long startPosition, int packetLenBytes)
        {
            var rtcpPacket = new RtcpGoodbyePacket();

            rtcpPacket.SynchronizationSources = new List<uint>(sourceCount);

            for (var i = 0; i < sourceCount; i++)
            {
                rtcpPacket.SynchronizationSources.Add(BinaryPrimitives.ReadUInt32BigEndian(bytes));
                idx += 4;
            }

            var remainingBytes = packetLenBytes - (idx - startPosition);
            if (remainingBytes > 0)
            {
                var reasonBytes = bytes.Slice(0, (int)remainingBytes);
                idx += (int)remainingBytes;

                rtcpPacket.Reason = Encoding.UTF8.GetString(reasonBytes);
            }

            return rtcpPacket;
        }
    }
}
