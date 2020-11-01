using System;
using System.Net;

namespace Subspace.Rtp
{
    /// <summary>
    /// The RTP header has the following format:
    /// 
    /// 0                   1                   2                   3
    /// 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |V=2|P|X|  CC   |M|     PT      |       sequence number         |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |                           timestamp                           |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// |           synchronization source (SSRC) identifier            |
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
    /// |            contributing source (CSRC) identifiers             |
    /// |                             ....                              |
    /// +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    /// 
    /// The first twelve octets are present in every RTP packet, while the
    /// list of CSRC identifiers is present only when inserted by a mixer.
    /// 
    /// https://tools.ietf.org/html/rfc3550#section-5.1
    /// </summary>
    public class RtpPacket
    {
        public IPEndPoint RemoteEndPoint { get; set; }

        /// <summary>
        /// version (V): 2 bits
        /// 
        /// This field identifies the version of RTP.  The version defined by
        /// this specification is two (2).  (The value 1 is used by the first
        /// draft version of RTP and the value 0 is used by the protocol
        /// initially implemented in the "vat" audio tool.)
        /// </summary>
        public byte Version { get; set; }

        /// <summary>
        /// padding (P): 1 bit
        /// 
        /// If the padding bit is set, the packet contains one or more
        /// additional padding octets at the end which are not part of the
        /// payload.  The last octet of the padding contains a count of how
        /// many padding octets should be ignored, including itself.  Padding
        /// may be needed by some encryption algorithms with fixed block sizes
        /// or for carrying several RTP packets in a lower-layer protocol data
        /// unit.
        /// </summary>
        public bool Padding { get; set; }

        /// <summary>
        /// extension (X): 1 bit
        /// If the extension bit is set, the fixed header MUST be followed by
        /// exactly one header extension, with a format defined in Section
        /// 5.3.1.
        /// </summary>
        public bool Extension { get; set; }

        /// <summary>
        /// CSRC count (CC): 4 bits
        /// 
        /// The CSRC count contains the number of CSRC identifiers that follow
        /// the fixed header.
        /// </summary>
        public byte ContributingSourceCount { get; set; }

        /// <summary>
        /// marker (M): 1 bit
        /// 
        /// The interpretation of the marker is defined by a profile.  It is
        /// intended to allow significant events such as frame boundaries to
        /// be marked in the packet stream.  A profile MAY define additional
        /// marker bits or specify that there is no marker bit by changing the
        /// number of bits in the payload type field (see Section 5.3).
        /// </summary>
        public bool Marker { get; set; }

        /// <summary>
        /// payload type (PT): 7 bits
        /// 
        /// This field identifies the format of the RTP payload and determines
        /// its interpretation by the application.  A profile MAY specify a
        /// default static mapping of payload type codes to payload formats.
        /// Additional payload type codes MAY be defined dynamically through
        /// non-RTP means (see Section 3).  A set of default mappings for
        /// audio and video is specified in the companion RFC 3551 [1].  An
        /// RTP source MAY change the payload type during a session, but this
        /// field SHOULD NOT be used for multiplexing separate media streams
        /// (see Section 5.2).
        /// 
        /// A receiver MUST ignore packets with payload types that it does not
        /// understand.
        /// </summary>
        public byte PayloadType { get; set; }

        /// <summary>
        /// sequence number: 16 bits
        /// 
        /// The sequence number increments by one for each RTP data packet
        /// sent, and may be used by the receiver to detect packet loss and to
        /// restore packet sequence.  The initial value of the sequence number
        /// SHOULD be random (unpredictable) to make known-plaintext attacks
        /// on encryption more difficult, even if the source itself does not
        /// encrypt according to the method in Section 9.1, because the
        /// packets may flow through a translator that does.  Techniques for
        /// choosing unpredictable numbers are discussed in [17].
        /// </summary>
        public ushort SequenceNumber { get; set; }

        /// <summary>
        /// timestamp: 32 bits
        /// 
        /// The timestamp reflects the sampling instant of the first octet in
        /// the RTP data packet.  The sampling instant MUST be derived from a
        /// clock that increments monotonically and linearly in time to allow
        /// synchronization and jitter calculations (see Section 6.4.1).  The
        /// resolution of the clock MUST be sufficient for the desired
        /// synchronization accuracy and for measuring packet arrival jitter
        /// (one tick per video frame is typically not sufficient).  The clock
        /// frequency is dependent on the format of data carried as payload
        /// and is specified statically in the profile or payload format
        /// specification that defines the format, or MAY be specified
        /// dynamically for payload formats defined through non-RTP means.  If
        /// RTP packets are generated periodically, the nominal sampling
        /// instant as determined from the sampling clock is to be used, not a
        /// reading of the system clock.  As an example, for fixed-rate audio
        /// the timestamp clock would likely increment by one for each
        /// sampling period.  If an audio application reads blocks covering
        /// 160 sampling periods from the input device, the timestamp would be
        /// increased by 160 for each such block, regardless of whether the
        /// block is transmitted in a packet or dropped as silent.
        /// </summary>
        public uint Timestamp { get; set; }

        /// <summary>
        /// SSRC: 32 bits
        /// 
        /// The SSRC field identifies the synchronization source.  This
        /// identifier SHOULD be chosen randomly, with the intent that no two
        /// synchronization sources within the same RTP session will have the
        /// same SSRC identifier.  An example algorithm for generating a
        /// random identifier is presented in Appendix A.6.  Although the
        /// probability of multiple sources choosing the same identifier is
        /// low, all RTP implementations must be prepared to detect and
        /// resolve collisions.  Section 8 describes the probability of
        /// collision along with a mechanism for resolving collisions and
        /// detecting RTP-level forwarding loops based on the uniqueness of
        /// the SSRC identifier.  If a source changes its source transport
        /// address, it must also choose a new SSRC identifier to avoid being
        /// interpreted as a looped source (see Section 8.2).
        /// </summary>
        public uint SynchronizationSource { get; set; }

        /// <summary>
        /// CSRC list: 0 to 15 items, 32 bits each
        /// 
        /// The CSRC list identifies the contributing sources for the payload
        /// contained in this packet.  The number of identifiers is given by
        /// the CC field.  If there are more than 15 contributing sources,
        /// only 15 can be identified.  CSRC identifiers are inserted by
        /// mixers (see Section 7.1), using the SSRC identifiers of
        /// contributing sources.  For example, for audio packets the SSRC
        /// identifiers of all sources that were mixed together to create a
        /// packet are listed, allowing correct talker indication at the
        /// receiver.
        /// </summary>
        public int[] ContributingSources { get; set; }

        /// <summary>
        /// The data transported by RTP in a packet, for
        /// example audio samples or compressed video data. 
        /// </summary>
        public ArraySegment<byte> Payload { get; set; }

        /// <summary>
        /// The original raw packet bytes
        /// </summary>
        public byte[] RawBytes { get; set; }
    }
}
