using System;
using System.IO;
using System.Net;

namespace Subspace.Rtsp
{
    public class RtspResponseMessage : WebResponse
    {
        private Uri _responseUri;

        /// <summary>
        /// The Status-Code element is a 3-digit integer result code of the
        /// attempt to understand and satisfy the request. These codes are fully
        /// defined in Section 11. The Reason-Phrase is intended to give a short
        /// textual description of the Status-Code. The Status-Code is intended
        /// for use by automata and the Reason-Phrase is intended for the human
        /// user. The client is not required to examine or display the Reason-
        /// Phrase.
        /// 
        /// The first digit of the Status-Code defines the class of response. The
        /// last two digits do not have any categorization role. There are 5
        /// values for the first digit:
        /// 
        /// * 1xx: Informational - Request received, continuing process
        /// * 2xx: Success - The action was successfully received, understood,
        /// and accepted
        /// * 3xx: Redirection - Further action must be taken in order to
        /// complete the request
        /// * 4xx: Client Error - The request contains bad syntax or cannot be
        /// fulfilled
        /// * 5xx: Server Error - The server failed to fulfill an apparently
        /// valid request
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-7.1.1
        /// </summary>
        public RtspStatusCode StatusCode { get; internal set; }

        /// <summary>
        /// The Status-Code is intended
        /// for use by automata and the Reason-Phrase is intended for the human
        /// user. The client is not required to examine or display the Reason-
        /// Phrase.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-7.1.1
        /// </summary>
        public string ReasonPhrase { get; internal set; }
        public override Uri ResponseUri => _responseUri;

        internal void SetResponseUri(Uri responseUri) => _responseUri = responseUri;

        public override WebHeaderCollection Headers { get; } = new WebHeaderCollection();

        public override string ContentType => Headers.Get("Content-Type");

        public override long ContentLength
        {
            get
            {
                var contentLength = Headers.Get("Content-Length");

                if (contentLength is null) return 0;

                return long.Parse(contentLength);
            }
        }

        public override Stream GetResponseStream() => Content;

        public override void Close()
        {
            GetResponseStream()?.Close();
        }

        internal Stream Content { get; set; }
    }
}
