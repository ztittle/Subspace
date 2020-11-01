using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Subspace.Rtp.Rtcp
{
    public static class RtcpPacketSerializer
    {
        public static void Serialize(RtcpPacket packet, Span<byte> buffer)
        {
            var idx = 0;
            if (packet is RtcpReceiverReportPacket receptReportPacket)
            {

                buffer[idx] = (byte)(receptReportPacket.Version << 6 | (receptReportPacket.Padding ? 1 : 0) << 5 | (ushort)receptReportPacket.ReceptionReports.Count);
                idx += 1;
                buffer[idx] = (byte)receptReportPacket.PacketType;
                idx += 1;
                BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(idx), receptReportPacket.LengthIn32BitWordsMinusOne);
                idx += 2;
                BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(idx), receptReportPacket.SynchronizationSource);
                idx += 4;
                foreach (var report in receptReportPacket.ReceptionReports)
                {
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(idx), report.SynchronizationSource);
                    idx += 4;
                    buffer[idx] = report.FractionLost;
                    idx += 1;
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(idx), report.CumulativeNumberOfPacketsLost);
                    idx += 3;
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(idx), report.ExtendedHighestSequenceNumberReceived);
                    idx += 4;
                    BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(idx), report.InterarrivalJitter);
                    idx += 4;
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(idx), report.LastSRTimestamp);
                    idx += 4;
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(idx), report.DelaySinceLastSR);
                    idx += 4;
                }
            }

            if (packet is RtcpSourceDescriptionPacket senderDescriptionPacket)
            {
                buffer[idx] = (byte)(senderDescriptionPacket.Version << 6 | (senderDescriptionPacket.Padding ? 1 : 0) << 5 | (ushort)senderDescriptionPacket.Chunks.Count);
                idx += 1;
                buffer[idx] = (byte)senderDescriptionPacket.PacketType;
                idx += 1;
                BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(idx), senderDescriptionPacket.LengthIn32BitWordsMinusOne);
                idx += 2;
                foreach (var chunk in senderDescriptionPacket.Chunks)
                {
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(idx), chunk.SynchronizationSource);
                    idx += 4;
                    foreach (var item in chunk.Items)
                    {
                        buffer[idx] = (byte)item.Type;
                        idx += 1;
                        buffer[idx] = (byte)item.Text.Length;
                        idx += 1;
                        var textBytes = Encoding.UTF8.GetBytes(item.Text);
                        textBytes.CopyTo(buffer.Slice(idx));
                        idx += textBytes.Length;
                    }
                }
            }
        }
    }
}
