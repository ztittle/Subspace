using System;
using System.Buffers.Binary;
using System.IO;

namespace Subspace.Rtp
{
    public static class RtpPacketParser
    {
        public static RtpPacket ParseRtpPacket(byte[] rawBytes)
        {
            var idx = 0;
            var b1 = rawBytes[idx];
            idx += 1;

            var rtpPacket = new RtpPacket();

            rtpPacket.RawBytes = rawBytes;
            rtpPacket.Version = (byte)(b1 >> 6);
            rtpPacket.Padding = (b1 >> 5 & 1) == 1;
            rtpPacket.Extension = (b1 >> 4 & 1) == 1;
            rtpPacket.ContributingSourceCount = (byte)(b1 & 0xF);

            var b2 = rawBytes[idx];
            idx += 1;
            rtpPacket.Marker = (b2 >> 7 & 1) == 1;
            rtpPacket.PayloadType = (byte)(b2 & 0x7F);

            rtpPacket.SequenceNumber = BinaryPrimitives.ReadUInt16BigEndian(rawBytes.AsSpan(idx));
            idx += 2;
            rtpPacket.Timestamp = BinaryPrimitives.ReadUInt32BigEndian(rawBytes.AsSpan(idx));
            idx += 4;
            rtpPacket.SynchronizationSource = BinaryPrimitives.ReadUInt32BigEndian(rawBytes.AsSpan(idx));
            idx += 4;

            var cSources = new int[rtpPacket.ContributingSourceCount];
            for (var i = 0; i < cSources.Length; i++)
            {
                cSources[i] = BinaryPrimitives.ReadInt32BigEndian(rawBytes.AsSpan(idx));
                idx += 4;
            }

            rtpPacket.ContributingSources = cSources;

            var payloadStart = idx;
            rtpPacket.Payload = new ArraySegment<byte>(rawBytes, payloadStart, rawBytes.Length - payloadStart);

            return rtpPacket;
        }
    }
}