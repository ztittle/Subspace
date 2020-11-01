using System;
using System.IO;
using System.Linq;

namespace Subspace.Sdp
{
    public class SdpParser
    {
        public SdpSessionDescription Parse(string sdp)
        {
            var sessionDescription = new SdpSessionDescription();

            using var sr = new StringReader(sdp);

            ParseSessionDescription(sessionDescription, sr);

            return sessionDescription;
        }

        private void ParseSessionDescription(SdpSessionDescription sessionDescription, StringReader stringReader)
        {
            var line = stringReader.ReadLine();

            if (line is null) return;

            ParseSdpLine(line, out var key , out var value);

            switch (key)
            {
                case "v":
                    sessionDescription.Version = byte.Parse(value);
                    break;
                case "o":
                    sessionDescription.Origin = ParseOriginLine(value);
                    break;
                case "s":
                    sessionDescription.SessionName = value;
                    break;
                case "t":
                    var timeParts = value.Split(' ');
                    sessionDescription.StartTime = long.Parse(timeParts[0]);
                    sessionDescription.EndTime = long.Parse(timeParts[1]);
                    break;
                case "m":
                    ParseMediaDescription(sessionDescription, value, stringReader);
                    break;
                case "a":
                    ParseSessionAttribute(sessionDescription, value);
                    break;
            }

            ParseSessionDescription(sessionDescription, stringReader);
        }

        private static void ParseSdpLine(string line, out string key, out string value)
        {
            var separatorIdx = line.IndexOf('=');
            key = line.Substring(0, separatorIdx);
            value = line.Substring(separatorIdx + 1);
        }

        private void ParseSessionAttribute(SdpSessionDescription sessionDescription, string attribute)
        {
            ParseAttributeKeyValue(attribute, out var key, out var value);

            switch (key)
            {
                case "bundle" when value != null:
                    sessionDescription.Bundles = value.Split(' ').ToList();
                    break;
                case "msid-semantic" when value != null:
                    sessionDescription.WebRtcMediaStreamId = value.Substring(value.IndexOf(" WMS ", StringComparison.Ordinal) + 1);
                    break;
            }
        }

        private static void ParseAttributeKeyValue(string attribute, out string key, out string value)
        {
            var attributeKeyIdx = attribute.IndexOf(':');

            key = attributeKeyIdx == -1 ? attribute : attribute.Substring(0, attributeKeyIdx);
            value = attributeKeyIdx == -1 ? null : attribute.Substring(attributeKeyIdx + 1);
        }

        private void ParseMediaDescription(SdpSessionDescription sessionDescription, string medialine, TextReader stringReader)
        {
            var mediaDescription = new SdpMediaDescription();

            var parts = medialine.Split(' ');

            mediaDescription.Media = TryGet(parts, 0);
            mediaDescription.Port = int.Parse(TryGet(parts, 1));
            mediaDescription.Protocol = TryGet(parts, 2);
            mediaDescription.MediaFormatDescriptions = parts.Skip(3)
                .Select(int.Parse)
                .ToDictionary(pt => pt, pt => new SdpMediaFormatDescription
                {
                    PayloadType = pt
                });

            var line = stringReader.ReadLine();
            while (line != null)
            {
                ParseSdpLine(line, out var key, out var value);
                switch (key)
                {
                    case "c":
                        mediaDescription.Connection = ParseConnectionLine(value);
                        break;
                    case "a":
                        ParseMediaDescriptionAttribute(mediaDescription, value);
                        break;
                    case "m":
                        ParseMediaDescription(sessionDescription, value, stringReader);
                        break;
                }

                line = stringReader.ReadLine();
            }

            sessionDescription.MediaDescriptions.Add(mediaDescription);
        }

        private void ParseMediaDescriptionAttribute(SdpMediaDescription mediaDescription, string attribute)
        {
            ParseAttributeKeyValue(attribute, out var key, out var value);

            switch (key)
            {
                case "control":
                    mediaDescription.RtspControlUrl = new Uri(value, UriKind.RelativeOrAbsolute);
                    break;
                case "framerate":
                    mediaDescription.Framerate = decimal.Parse(value);
                    break;
                case "sendrecv":
                    mediaDescription.SendReceiveMode = SendReceiveMode.SendRecv;
                    break;
                case "sendonly":
                    mediaDescription.SendReceiveMode = SendReceiveMode.SendOnly;
                    break;
                case "recvonly":
                    mediaDescription.SendReceiveMode = SendReceiveMode.RecvOnly;
                    break;
                case "inactive":
                    mediaDescription.SendReceiveMode = SendReceiveMode.Inactive;
                    break;
                case "mid":
                    mediaDescription.MediaId = value;
                    break;
                case "ice-ufrag":
                    mediaDescription.IceAttributes.UsernameFragment = value;
                    break;
                case "ice-pwd":
                    mediaDescription.IceAttributes.Password = value;
                    break;
                case "ice-options":
                    mediaDescription.IceAttributes.Options = value;
                    break;
                case "fingerprint":
                    var fingerprintParts = value.Split(' ');
                    mediaDescription.DtlsAttributes.HashFunc = TryGet(fingerprintParts, 0);
                    mediaDescription.DtlsAttributes.Fingerprint = TryGet(fingerprintParts, 1);
                    break;
                case "setup":
                    mediaDescription.DtlsAttributes.Setup = value;
                    break;
                case "rtpmap":
                    ParseRtpMapAttribute(mediaDescription, value);
                    break;
                case "fmtp":
                    ParseFormatParametersAttribute(mediaDescription, value);
                    break;
                case "rtcp-fb":
                    ParseRtcpFeedbackCapabilityAttribute(mediaDescription, value);
                    break;
                case "ssrc":
                    ParseSsrcAttribute(mediaDescription, value);
                    break;
                case "candidate":
                    var parts = value.Split(' ');
                    var sdpIceCandidate = new SdpIceCandidate();

                    sdpIceCandidate.Foundation = long.Parse(TryGet(parts, 0));
                    sdpIceCandidate.ComponentId = byte.Parse(TryGet(parts, 1));
                    sdpIceCandidate.Transport = TryGet(parts, 2);
                    sdpIceCandidate.Priority = uint.Parse(TryGet(parts, 3));
                    sdpIceCandidate.ConnectionAddress = TryGet(parts, 4);
                    sdpIceCandidate.Port = ushort.Parse(TryGet(parts, 5));
                    sdpIceCandidate.CandidateType = TryGet(parts, 7);

                    mediaDescription.IceCandidates.Add(sdpIceCandidate);
                    break;
                case "end-of-candidates":
                    break;
            }
        }

        private void ParseSsrcAttribute(SdpMediaDescription mediaDescription, string value)
        {
            var parts = value.Split(' ');
            var mediaAttributes = mediaDescription.MediaSourceAttributes;
            mediaAttributes.Ssrc = uint.Parse(TryGet(parts, 0));
            var ssrcParam = TryGet(parts, 1);

            switch (ssrcParam)
            {
                case "cname":
                    mediaAttributes.CName = ssrcParam;
                    break;
                case "msid":
                    mediaAttributes.MsId = ssrcParam;
                    mediaAttributes.MsIdAppData = TryGet(parts, 2);
                    break;
            }
        }

        private static void ParseRtcpFeedbackCapabilityAttribute(SdpMediaDescription mediaDescription, string value)
        {
            var separatorIdx = value.IndexOf(' ');
            var payloadTypeStr = value.Substring(0, separatorIdx);
            var cap = value.Substring(separatorIdx + 1);
            if (payloadTypeStr == "*")
            {
                foreach (var mfd in mediaDescription.MediaFormatDescriptions.Values)
                {
                    mfd.RtcpFeedbackCapability.Add(cap);
                }
            }
            else
            {
                var payloadType = int.Parse(payloadTypeStr);
                var mfd = mediaDescription.MediaFormatDescriptions[payloadType];

                mfd.RtcpFeedbackCapability.Add(cap);
            }
        }

        private void ParseFormatParametersAttribute(SdpMediaDescription mediaDescription, string value)
        {
            var separatorIdx = value.IndexOf(' ');
            var payloadType = int.Parse(value.Substring(0, separatorIdx));
            var formatParameters = value.Substring(separatorIdx + 1);
            var mfd = mediaDescription.MediaFormatDescriptions[payloadType];

            mfd.FormatParameters = formatParameters;
        }

        private void ParseRtpMapAttribute(SdpMediaDescription mediaDescription, string value)
        {
            var separatorIdx = value.IndexOf(' ');
            var payloadType = int.Parse(value.Substring(0, separatorIdx));
            var encoding = value.Substring(separatorIdx + 1);
            var encodingParts = encoding.Split('/');
            var mfd = mediaDescription.MediaFormatDescriptions[payloadType];

            mfd.EncodingName = encodingParts[0];
            if (int.TryParse(TryGet(encodingParts, 1), out var clockRate))
                mfd.ClockRate = clockRate;
            
            mfd.EncodingParameters = TryGet(encodingParts, 2);
        }

        private SdpConnection ParseConnectionLine(string value)
        {
            var parts = value.Split(' ');

            var connection = new SdpConnection();

            connection.NetType = TryGet(parts, 0);
            connection.AddrType = TryGet(parts, 1);
            connection.Address = TryGet(parts, 2);

            return connection;
        }

        private SdpOrigin ParseOriginLine(string value)
        {
            var parts = value.Split(' ');

            return new SdpOrigin
            {
                Username = TryGet(parts, 0),
                SessionId = ulong.Parse(TryGet(parts, 1)),
                SessionVersion = long.Parse(TryGet(parts, 2)),
                NetType = TryGet(parts, 3),
                AddrType = TryGet(parts, 4),
                UnicastAddr = TryGet(parts, 5)
            };
        }

        private string TryGet(string[] parts, int idx)
        {
            return idx < parts.Length ? parts[idx] : null;
        }
    }
}
