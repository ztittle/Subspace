using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Subspace.Rtsp
{
    public interface IRtspClient
    {
        int ReadTimeoutMs { get; set; }
        void AddCredential(Uri rtspUri, NetworkCredential credential);

        /// <summary>
        /// An OPTIONS
        /// request may be issued at any time, e.g., if the client is about to
        /// try a nonstandard request. It does not influence server state.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-10.1
        /// </summary>
        Task<RtspOptionsResponse> OptionsAsync(Uri rtspUri);

        /// <summary>
        /// The DESCRIBE method retrieves the description of a presentation or
        /// media object identified by the request URL from a server. It may use
        /// the Accept header to specify the description formats that the client
        /// understands. The server responds with a description of the requested
        /// resource. The DESCRIBE reply-response pair constitutes the media
        /// initialization phase of RTSP.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-10.2
        /// </summary>
        Task<RtspDescribeResponse> DescribeAsync(Uri rtspUri);

        /// <summary>
        /// The SETUP request for a URI specifies the transport mechanism to be
        /// used for the streamed media. A client can issue a SETUP request for a
        /// stream that is already playing to change transport parameters, which
        /// a server MAY allow. If it does not allow this, it MUST respond with
        /// error "455 Method Not Valid In This State". For the benefit of any
        /// intervening firewalls, a client must indicate the transport
        /// parameters even if it has no influence over these parameters, for
        /// example, where the server advertises a fixed multicast address.
        ///
        /// https://tools.ietf.org/html/rfc2326#section-10.4
        /// </summary>
        Task<RtspSetupResponse> SetupAsync(Uri rtspControlUri, int rtpPort, int rtcpPort);

        /// <summary>
        /// The PLAY method tells the server to start sending data via the
        /// mechanism specified in SETUP. A client MUST NOT issue a PLAY request
        /// until any outstanding SETUP requests have been acknowledged as
        /// successful.
        /// 
        /// The PLAY request positions the normal play time to the beginning of
        /// the range specified and delivers stream data until the end of the
        /// range is reached. PLAY requests may be pipelined (queued); a server
        /// MUST queue PLAY requests to be executed in order. That is, a PLAY
        /// request arriving while a previous PLAY request is still active is
        /// delayed until the first has been completed.
        /// 
        /// This allows precise editing.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-10.5
        /// </summary>
        ///
        /// <param name="session">
        /// Session identifiers are opaque strings of arbitrary length. Linear
        /// white space must be URL-escaped. A session identifier MUST be chosen
        /// randomly and MUST be at least eight octets long to make guessing it
        /// more difficult. (See Section 16.)
        /// 
        /// session-id   =   1*( ALPHA | DIGIT | safe )
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-3.4
        /// </param>
        /// 
        /// <param name="nptRange">
        /// Normal play time (NPT) indicates the stream absolute position
        /// relative to the beginning of the presentation. The timestamp consists
        /// of a decimal fraction. The part left of the decimal may be expressed
        /// in either seconds or hours, minutes, and seconds. The part right of
        /// the decimal point measures fractions of a second.
        /// 
        /// The beginning of a presentation corresponds to 0.0 seconds. Negative
        /// values are not defined. The special constant now is defined as the
        /// current instant of a live event. It may be used only for live events.
        /// 
        /// NPT is defined as in DSM-CC: "Intuitively, NPT is the clock the
        /// viewer associates with a program. It is often digitally displayed on
        /// a VCR. NPT advances normally when in normal play mode (scale = 1),
        /// advances at a faster rate when in fast scan forward (high positive
        /// scale ratio), decrements when in scan reverse (high negative scale
        /// ratio) and is fixed in pause mode. NPT is (logically) equivalent to
        /// SMPTE time codes." [5]
        /// 
        /// npt-range    =   ( npt-time "-" [ npt-time ] ) | ( "-" npt-time )
        /// npt-time     =   "now" | npt-sec | npt-hhmmss
        /// npt-sec      =   1*DIGIT [ "." *DIGIT ]
        /// npt-hhmmss   =   npt-hh ":" npt-mm ":" npt-ss [ "." *DIGIT ]
        /// npt-hh       =   1*DIGIT     ; any positive number
        /// npt-mm       =   1*2DIGIT    ; 0-59
        /// npt-ss       =   1*2DIGIT    ; 0-59
        ///
        ///     Examples:
        /// npt=123.45-125
        /// npt=12:05:35.3-
        /// npt=now-
        /// 
        /// The syntax conforms to ISO 8601. The npt-sec notation is optimized
        /// for automatic generation, the ntp-hhmmss notation for consumption
        /// by human readers. The "now" constant allows clients to request to
        /// receive the live feed rather than the stored or time-delayed
        /// version. This is needed since neither absolute time nor zero time
        /// are appropriate for this case.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-3.6
        /// </param>
        Task<RtspPlayResponse> PlayAsync(Uri rtspUri, string session, string nptRange);

        /// <summary>
        /// The TEARDOWN request stops the stream delivery for the given URI,
        /// freeing the resources associated with it. If the URI is the
        /// presentation URI for this presentation, any RTSP session identifier
        /// associated with the session is no longer valid. Unless all transport
        /// parameters are defined by the session description, a SETUP request
        /// has to be issued before the session can be played again.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-10.7
        /// </summary>
        Task<RtspResponseMessage> TeardownAsync(Uri rtspUri, string session);

        Task<RtspResponseMessage> SendAsync(RtspRequestMessage requestMessage);
        void Dispose();
    }

    /// <summary>
    /// https://tools.ietf.org/html/rfc2326
    /// </summary>
    public class RtspClient : IDisposable, IRtspClient
    {
        private readonly ConcurrentDictionary<Uri, NetworkCredential> _credentials =
            new ConcurrentDictionary<Uri, NetworkCredential>();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        private readonly Dictionary<string, TcpClient> _tcpClientMap = new Dictionary<string, TcpClient>();
        private bool _disposed;

        /// <summary>
        /// The CSeq field specifies the sequence number for an RTSP request-
        /// response pair. This field MUST be present in all requests and
        /// responses. For every RTSP request containing the given sequence
        /// number, there will be a corresponding response having the same
        /// number.  Any retransmitted request must contain the same sequence
        /// number as the original (i.e. the sequence number is not incremented
        /// for retransmissions of the same request).
        ///
        /// https://tools.ietf.org/html/rfc2326#section-12.17
        /// </summary>
        private volatile int _cSeq;

        public int WriteTimeoutMs { get; set; } = 2000;
        public int ReadTimeoutMs { get; set; } = 2000;

        private string _userAgent = "Subspace/1.0.0";

        public void AddCredential(Uri rtspUri, NetworkCredential credential)
        {
            _credentials.AddOrUpdate(rtspUri, k => credential, (k, v) => credential);
        }

        /// <summary>
        /// An OPTIONS
        /// request may be issued at any time, e.g., if the client is about to
        /// try a nonstandard request. It does not influence server state.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-10.1
        /// </summary>
        public async Task<RtspOptionsResponse> OptionsAsync(Uri rtspUri)
        {
            var requestMessage = new RtspRequestMessage(rtspUri, "OPTIONS");

            var response = await SendAsync(requestMessage);

            return new RtspOptionsResponse
            {
                AllowedMethods = response.Headers.Get("Public").Split(", "),
                ResponseMessage = response
            };
        }

        /// <summary>
        /// The DESCRIBE method retrieves the description of a presentation or
        /// media object identified by the request URL from a server. It may use
        /// the Accept header to specify the description formats that the client
        /// understands. The server responds with a description of the requested
        /// resource. The DESCRIBE reply-response pair constitutes the media
        /// initialization phase of RTSP.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-10.2
        /// </summary>
        public async Task<RtspDescribeResponse> DescribeAsync(Uri rtspUri)
        {
            var requestMessage = new RtspRequestMessage(rtspUri, "DESCRIBE")
            {
                Headers = { new KeyValuePair<string, string>("Accept", "application/sdp") }
            };

            var response = await SendAsync(requestMessage);

            using var sr = new StreamReader(response.GetResponseStream());

            return new RtspDescribeResponse
            {
                ResponseMessage = response, 
                Sdp = await sr.ReadToEndAsync()
            };
        }

        /// <summary>
        /// The SETUP request for a URI specifies the transport mechanism to be
        /// used for the streamed media. A client can issue a SETUP request for a
        /// stream that is already playing to change transport parameters, which
        /// a server MAY allow. If it does not allow this, it MUST respond with
        /// error "455 Method Not Valid In This State". For the benefit of any
        /// intervening firewalls, a client must indicate the transport
        /// parameters even if it has no influence over these parameters, for
        /// example, where the server advertises a fixed multicast address.
        ///
        /// https://tools.ietf.org/html/rfc2326#section-10.4
        /// </summary>
        public async Task<RtspSetupResponse> SetupAsync(Uri rtspControlUri, int rtpPort, int rtcpPort)
        {
            var requestMessage = new RtspRequestMessage(rtspControlUri, "SETUP")
            {
                Headers = { new KeyValuePair<string, string>("Transport", $"RTP/AVP;unicast;client_port={rtpPort}-{rtcpPort}") }
            };

            var response = await SendAsync(requestMessage);

            var sessionHeader = response.Headers.Get("Session");
            var transportHeader = response.Headers.Get("Transport");

            var sessionKeyValues = KeyValueParser.ParsePairs(sessionHeader, ';');
            var session = sessionKeyValues[0].Key;
            var sessionTimeout = sessionKeyValues.FirstOrDefault(l => l.Key == "timeout").Value;

            var transportKeyValues = KeyValueParser.ParsePairs(transportHeader, ';');

            var serverPortRange = transportKeyValues.FirstOrDefault(l => l.Key == "server_port").Value;
            var ssrc = transportKeyValues.FirstOrDefault(l => l.Key == "ssrc").Value;

            return new RtspSetupResponse
            {
                Session = session,
                SessionTimeoutSeconds = int.Parse(sessionTimeout),
                ServerPorts = serverPortRange?.Split('-').Select(int.Parse).ToArray(),
                Ssrc = Convert.ToUInt32(ssrc, 16),
                ResponseMessage = response
            };
        }

        /// <summary>
        /// The PLAY method tells the server to start sending data via the
        /// mechanism specified in SETUP. A client MUST NOT issue a PLAY request
        /// until any outstanding SETUP requests have been acknowledged as
        /// successful.
        /// 
        /// The PLAY request positions the normal play time to the beginning of
        /// the range specified and delivers stream data until the end of the
        /// range is reached. PLAY requests may be pipelined (queued); a server
        /// MUST queue PLAY requests to be executed in order. That is, a PLAY
        /// request arriving while a previous PLAY request is still active is
        /// delayed until the first has been completed.
        /// 
        /// This allows precise editing.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-10.5
        /// </summary>
        ///
        /// <param name="session">
        /// Session identifiers are opaque strings of arbitrary length. Linear
        /// white space must be URL-escaped. A session identifier MUST be chosen
        /// randomly and MUST be at least eight octets long to make guessing it
        /// more difficult. (See Section 16.)
        /// 
        /// session-id   =   1*( ALPHA | DIGIT | safe )
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-3.4
        /// </param>
        /// 
        /// <param name="nptRange">
        /// Normal play time (NPT) indicates the stream absolute position
        /// relative to the beginning of the presentation. The timestamp consists
        /// of a decimal fraction. The part left of the decimal may be expressed
        /// in either seconds or hours, minutes, and seconds. The part right of
        /// the decimal point measures fractions of a second.
        /// 
        /// The beginning of a presentation corresponds to 0.0 seconds. Negative
        /// values are not defined. The special constant now is defined as the
        /// current instant of a live event. It may be used only for live events.
        /// 
        /// NPT is defined as in DSM-CC: "Intuitively, NPT is the clock the
        /// viewer associates with a program. It is often digitally displayed on
        /// a VCR. NPT advances normally when in normal play mode (scale = 1),
        /// advances at a faster rate when in fast scan forward (high positive
        /// scale ratio), decrements when in scan reverse (high negative scale
        /// ratio) and is fixed in pause mode. NPT is (logically) equivalent to
        /// SMPTE time codes." [5]
        /// 
        /// npt-range    =   ( npt-time "-" [ npt-time ] ) | ( "-" npt-time )
        /// npt-time     =   "now" | npt-sec | npt-hhmmss
        /// npt-sec      =   1*DIGIT [ "." *DIGIT ]
        /// npt-hhmmss   =   npt-hh ":" npt-mm ":" npt-ss [ "." *DIGIT ]
        /// npt-hh       =   1*DIGIT     ; any positive number
        /// npt-mm       =   1*2DIGIT    ; 0-59
        /// npt-ss       =   1*2DIGIT    ; 0-59
        ///
        ///     Examples:
        /// npt=123.45-125
        /// npt=12:05:35.3-
        /// npt=now-
        /// 
        /// The syntax conforms to ISO 8601. The npt-sec notation is optimized
        /// for automatic generation, the ntp-hhmmss notation for consumption
        /// by human readers. The "now" constant allows clients to request to
        /// receive the live feed rather than the stored or time-delayed
        /// version. This is needed since neither absolute time nor zero time
        /// are appropriate for this case.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-3.6
        /// </param>
        public async Task<RtspPlayResponse> PlayAsync(Uri rtspUri, string session, string nptRange)
        {
            var requestMessage = new RtspRequestMessage(rtspUri, "PLAY")
            {
                Headers =
                {
                    new KeyValuePair<string, string>("Session", session),
                    new KeyValuePair<string, string>("Range", $"npt={nptRange}")
                }
            };

            var response = await SendAsync(requestMessage);

            var rtpInfoHeader = response.Headers.Get("RTP-Info");

            var rtpInfoParts = rtpInfoHeader.Split(',');
            var rtpInfos = new List<RtpInfo>();

            foreach (var rtpInfoPart in rtpInfoParts)
            {
                var pairs = KeyValueParser.ParsePairs(rtpInfoPart, ';');

                var rtpInfo = new RtpInfo
                {
                    Url = pairs.First(l => l.Key == "url").Value,
                    Seq = Convert.ToUInt32(pairs.First(l => l.Key == "seq").Value),
                    RtpTime = Convert.ToUInt32(pairs.FirstOrDefault(l => l.Key == "rtptime").Value)
                };

                rtpInfos.Add(rtpInfo);
            }

            return new RtspPlayResponse
            {
                RtpInfo = rtpInfos.ToArray(),
                ResponseMessage = response
            };
        }

        /// <summary>
        /// The TEARDOWN request stops the stream delivery for the given URI,
        /// freeing the resources associated with it. If the URI is the
        /// presentation URI for this presentation, any RTSP session identifier
        /// associated with the session is no longer valid. Unless all transport
        /// parameters are defined by the session description, a SETUP request
        /// has to be issued before the session can be played again.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-10.7
        /// </summary>
        public async Task<RtspResponseMessage> TeardownAsync(Uri rtspUri, string session)
        {
            var requestMessage = new RtspRequestMessage(rtspUri, "TEARDOWN")
            {
                Headers =
                {
                    new KeyValuePair<string, string>("Session", session)
                }
            };

            var response = await SendAsync(requestMessage);

            return response;
        }

        public async Task<RtspResponseMessage> SendAsync(RtspRequestMessage requestMessage)
        {
            RtspResponseMessage responseMessage;

            responseMessage = await SendAndAwaitResponseAsync(requestMessage);

            if (responseMessage.StatusCode == RtspStatusCode.Unauthorized)
            {
                var wwwAuthenticateRawResponseHeader = responseMessage.Headers.Get("WWW-Authenticate");

                var wwwAuthenticateResponseHeader = ParseWwwAuthenticateResponseHeader(wwwAuthenticateRawResponseHeader);

                if (wwwAuthenticateResponseHeader.Challenge == ChallengeType.Digest)
                {
                    _credentials.TryGetValue(requestMessage.RtspUri, out var creds);

                    if (creds is null)
                    {
                        throw new InvalidOperationException("Authentication is required, but credentials are missing.");
                    }

                    var md5 = MD5.Create();

                    // https://tools.ietf.org/html/rfc7616#section-3.4.1
                    var hA1 = md5.ComputeHashString($"{creds.UserName}:{wwwAuthenticateResponseHeader.Realm}:{creds.Password}");
                    var hA2 = md5.ComputeHashString($"{requestMessage.Method}:{requestMessage.RtspUri.OriginalString}");
                    var response = md5.ComputeHashString($"{hA1}:{wwwAuthenticateResponseHeader.Nonce}:{hA2}");

                    var authorizationHeaderVal = $"Digest username=\"{creds.UserName}\", realm=\"{wwwAuthenticateResponseHeader.Realm}\", nonce=\"{wwwAuthenticateResponseHeader.Nonce}\", uri=\"{requestMessage.RtspUri.OriginalString}\", response=\"{response}\"";

                    requestMessage.Headers.Add(new KeyValuePair<string, string>("Authorization", authorizationHeaderVal));
                }

                responseMessage = await SendAndAwaitResponseAsync(requestMessage);
            }

            if ((int)responseMessage.StatusCode >= 400)
            {
                throw new WebException(responseMessage.ReasonPhrase, null,  WebExceptionStatus.ProtocolError, responseMessage);
            }

            return responseMessage;
        }

        private static WWWAuthenticateResponseHeader ParseWwwAuthenticateResponseHeader(string authHeaderValue)
        {
            var authenticateResponseHeader = new WWWAuthenticateResponseHeader();

            var firstSpace = authHeaderValue.IndexOf(' ');

            var challenge = authHeaderValue.Substring(0, firstSpace);

            authenticateResponseHeader.Challenge = challenge switch
            {
                "Digest" => ChallengeType.Digest,
                _ => throw new NotSupportedException($"{challenge} is not supported."),
            };

            var directives = authHeaderValue.Substring(firstSpace + 1).Split(", ");

            foreach (var directive in directives)
            {
                var directiveParts = directive.Split("=");

                var key = directiveParts[0];
                var val = directiveParts[1].Trim('"');

                switch (key)
                {
                    case "realm":
                        authenticateResponseHeader.Realm = val;
                        break;
                    case "nonce":
                        authenticateResponseHeader.Nonce = val;
                        break;
                }
            }

            return authenticateResponseHeader;
        }

        private async Task<RtspResponseMessage> SendAndAwaitResponseAsync(RtspRequestMessage requestMessage)
        {

            try
            {
                return await SendAndAwaitResponseInternalAsync(requestMessage);
            }
            catch (SocketException se)
            {
                Debug.WriteLine($"Connection reset. Retrying... {se}", nameof(RtspClient));
                return await SendAndAwaitResponseInternalAsync(requestMessage);
            }
            catch (IOException ioe) when (ioe.InnerException is SocketException se)
            {
                Debug.WriteLine($"Connection reset. Retrying... {se}", nameof(RtspClient));
                return await SendAndAwaitResponseInternalAsync(requestMessage);
            }
        }

        private async Task<RtspResponseMessage> SendAndAwaitResponseInternalAsync(RtspRequestMessage requestMessage)
        {
            var tcpClient = await GetTcpClientAsync(requestMessage.RtspUri);

            var stream = tcpClient.GetStream();

            stream.WriteTimeout = WriteTimeoutMs;

            await WriteToNetworkStreamAsync(requestMessage, stream);

            stream.ReadTimeout = ReadTimeoutMs;

            var responseMessage = await ReceiveFromNetworkStreamAsync(stream);

            responseMessage.SetResponseUri(requestMessage.RtspUri);

            return responseMessage;
        }

        public static ValueTask WriteLineAsync(Stream stream, string line = null)
        {
            line = line + "\r\n";

            var buffer = Encoding.ASCII.GetBytes(line);
            return stream.WriteAsync(buffer);
        }

        private async Task WriteToNetworkStreamAsync(RtspRequestMessage requestMessage, NetworkStream stream)
        {
            await WriteLineAsync(stream, $"{requestMessage.Method} {requestMessage.RtspUri.OriginalString} RTSP/1.0");

            Interlocked.Increment(ref _cSeq);
            await WriteLineAsync(stream, $"CSeq: {_cSeq}");
            await WriteLineAsync(stream, $"User-Agent: {_userAgent}");
            if (requestMessage.Headers != null)
            {
                foreach (var header in requestMessage.Headers)
                {
                    await WriteLineAsync(stream, $"{header.Key}: {header.Value}");
                }
            }

            var requestContent = requestMessage.Content;

            if (requestContent.Length > 0)
            {
                await WriteLineAsync(stream, $"Content-Length: {requestContent.Length}");
            }

            await WriteLineAsync(stream);

            if (requestContent.Length > 0)
            {
                await WriteLineAsync(stream, $"Content-Length: {requestContent.Length}");
                await stream.WriteAsync(requestContent.GetBuffer());
            }

            await stream.FlushAsync();
        }

        private async Task<RtspResponseMessage> ReceiveFromNetworkStreamAsync(NetworkStream stream)
        {
            using var cts = new CancellationTokenSource(ReadTimeoutMs);
            // todo: replace streamReader with raw unbuffered stream reads
            using var streamReader = new StreamReader(stream, Encoding.ASCII, false, 1024, true);

            var reading = true;
            var readingResponseStatus = true;
            var readingHeaders = true;
            var hasContent = false;

            var responseMessage = new RtspResponseMessage();

            var contentLength = 0;

            while (reading)
            {
                if (readingResponseStatus)
                {
                    var statusLine = await streamReader.ReadLineAsync();

                    var statusLineSpaceIdx = statusLine.IndexOf(' ');

                    var statusLineVersion = statusLine.Substring(0, statusLineSpaceIdx);
                    var statusCodeReason = statusLine.Substring(statusLineSpaceIdx + 1);

                    var statusLineVersionParts = statusLineVersion.Split('/');

                    var rtspVersion = statusLineVersionParts[1];
                    if (rtspVersion != "1.0")
                    {
                        throw new NotSupportedException($"Rtsp server version {rtspVersion} not supported.");
                    }

                    var statusLineReasonSpaceIdx = statusCodeReason.IndexOf(' ');
                    var statusCodeStr = statusCodeReason.Substring(0, statusLineReasonSpaceIdx);
                    if (int.TryParse(statusCodeStr, out var statusCode))
                    {
                        responseMessage.StatusCode = (RtspStatusCode)statusCode;
                    }

                    if (statusCodeReason.Length > statusLineReasonSpaceIdx)
                        responseMessage.ReasonPhrase = statusCodeReason.Substring(statusLineReasonSpaceIdx + 1);

                    readingResponseStatus = false;
                    continue;
                }

                if (readingHeaders)
                {
                    var line = await streamReader.ReadLineAsync();

                    if (line == string.Empty)
                    {
                        readingHeaders = false;
                        continue;
                    }

                    var headerParts = line.Split(": ");
                    var header = new KeyValuePair<string, string>(headerParts[0], headerParts[1]);
                    responseMessage.Headers.Add(header.Key, header.Value);

                    if (header.Key == "Content-Length")
                    {
                        hasContent = true;
                        int.TryParse(header.Value, out contentLength);
                    }

                    continue;
                }

                if (hasContent && contentLength > 0)
                {
                    var contentBytes = new char[contentLength];

                    await streamReader.ReadBlockAsync(contentBytes, cts.Token);
                    responseMessage.Content = new MemoryStream(Encoding.ASCII.GetBytes(contentBytes));
                }

                reading = false;
            }

            return responseMessage;
        }

        private async Task<TcpClient> GetTcpClientAsync(Uri rtspUri)
        {
            var locker = _locks.GetOrAdd(rtspUri.Host, k => new SemaphoreSlim(1, 1));

            await locker.WaitAsync(CancellationToken.None);

            var hostAddresses = await Dns.GetHostAddressesAsync(rtspUri.Host);
            var hostAddress = hostAddresses.FirstOrDefault();

            if (hostAddress is null)
            {
                throw new WebException($"Unknown host '{rtspUri.Host}'", WebExceptionStatus.NameResolutionFailure);
            }

            if (!_tcpClientMap.TryGetValue(rtspUri.Host, out var tcpClient))
            {
                tcpClient = new TcpClient(hostAddress.AddressFamily);
                _tcpClientMap.Add(rtspUri.Host, tcpClient);

                await tcpClient.ConnectAsync(hostAddress, rtspUri.Port);
            }
            else if (tcpClient.Connected == false)
            {
                tcpClient.Dispose();
                _tcpClientMap.Remove(rtspUri.Host);
                tcpClient = new TcpClient(hostAddress.AddressFamily);
                _tcpClientMap.Add(rtspUri.Host, tcpClient);

                await tcpClient.ConnectAsync(hostAddress, rtspUri.Port);
            }

            locker.Release();

            return tcpClient;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Parallel.ForEach(_tcpClientMap.Values, tcpClient => tcpClient.Dispose());
            }

            _disposed = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
