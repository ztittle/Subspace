using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Subspace.Stun.StringPrep;

namespace Subspace.Stun
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc5389
    /// </summary>
    public class StunRecord
    {
        public StunMessageType MessageType { get; set; }

        /*
           The message length MUST contain the size, in bytes, of the message
           not including the 20-byte STUN header.  Since all STUN attributes are
           padded to a multiple of 4 bytes, the last 2 bits of this field are
           always zero.  This provides another way to distinguish STUN packets
           from packets of other protocols.
        */
        public ushort MessageLength
        {
            get
            {
                return (ushort)StunAttributes.Sum(l => l.Length + l.Padding + 4 /*size of length and type*/);
            }
        }

        /*
           The magic cookie field MUST contain the fixed value 0x2112A442 in
           network byte order.  In RFC 3489 [RFC3489], this field was part of
           the transaction ID; placing the magic cookie in this location allows
           a server to detect if the client will understand certain attributes
           that were added in this revised specification.  In addition, it aids
           in distinguishing STUN packets from packets of other protocols when
           STUN is multiplexed with those other protocols on the same port.
         */
        public static readonly int MessageCookie = 0x2112A442;

        /*
           The transaction ID is a 96-bit identifier, used to uniquely identify
           STUN transactions.  For request/response transactions, the
           transaction ID is chosen by the STUN client for the request and
           echoed by the server in the response.  For indications, it is chosen
           by the agent sending the indication.  It primarily serves to
           correlate requests with responses, though it also plays a small role
           in helping to prevent certain types of attacks.  The server also uses
           the transaction ID as a key to identify each transaction uniquely
           across all clients.  As such, the transaction ID MUST be uniformly
           and randomly chosen from the interval 0 .. 2**96-1, and SHOULD be
           cryptographically random.  Resends of the same request reuse the same
           transaction ID, but the client MUST choose a new transaction ID for
           new transactions unless the new request is bit-wise identical to the
           previous request and sent from the same transport address to the same
           IP address.  Success and error responses MUST carry the same
           transaction ID as their corresponding request.  When an agent is
           acting as a STUN server and STUN client on the same port, the
           transaction IDs in requests sent by the agent have no relationship to
           the transaction IDs in requests received by the agent.
         */
        public byte[] MessageTransactionId { get; set; }

        /*
          Following the STUN fixed portion of the header are zero or more
          attributes.  Each attribute is TLV (Type-Length-Value) encoded.  The
          details of the encoding, and of the attributes themselves are given
          in Section 15.
         */
        public List<StunAttribute> StunAttributes { get; set; } = new List<StunAttribute>();

        private byte[] GetAsBytes(int padLen = 32 /*size of MessageIntegrity and Fingerprint*/)
        {
            var bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes((short)MessageType).Reverse().ToArray());
            bytes.AddRange(BitConverter.GetBytes((ushort)(MessageLength + padLen)).Reverse().ToArray());
            bytes.AddRange(BitConverter.GetBytes(MessageCookie).Reverse().ToArray());
            bytes.AddRange(MessageTransactionId);
            
            foreach(var attr in StunAttributes)
            {
                bytes.AddRange(BitConverter.GetBytes((ushort)attr.Type).Reverse().ToArray());
                bytes.AddRange(BitConverter.GetBytes(attr.Length).Reverse().ToArray());
                if (attr.Length > 0)
                {
                    bytes.AddRange(attr.Value);
                    bytes.AddRange(new byte[attr.Padding]);
                }
            }

            return bytes.ToArray();
        }

        public void Sign(string password)
        {
            var messageBytes = GetAsBytes(24);

            var saslPassword = SaslPrep.Process(password);

            MessageIntegrity = new MessageIntegrityAttribute
            {
                Value = new HMACSHA1(Encoding.UTF8.GetBytes(saslPassword)).ComputeHash(messageBytes)
            };
        }

        /*
           15.4.  MESSAGE-INTEGRITY

           The MESSAGE-INTEGRITY attribute contains an HMAC-SHA1 [RFC2104] of
           the STUN message.  The MESSAGE-INTEGRITY attribute can be present in
           any STUN message type.  Since it uses the SHA1 hash, the HMAC will be
           20 bytes.  The text used as input to HMAC is the STUN message,
           including the header, up to and including the attribute preceding the
           MESSAGE-INTEGRITY attribute.  With the exception of the FINGERPRINT
           attribute, which appears after MESSAGE-INTEGRITY, agents MUST ignore
           all other attributes that follow MESSAGE-INTEGRITY.

           The key for the HMAC depends on whether long-term or short-term
           credentials are in use.  For long-term credentials, the key is 16
           bytes:

           key = MD5(username ":" realm ":" SASLprep(password))

           That is, the 16-byte key is formed by taking the MD5 hash of the
           result of concatenating the following five fields: (1) the username,
           with any quotes and trailing nulls removed, as taken from the
           USERNAME attribute (in which case SASLprep has already been applied);
           (2) a single colon; (3) the realm, with any quotes and trailing nulls
           removed; (4) a single colon; and (5) the password, with any trailing
           nulls removed and after processing using SASLprep.  For example, if
           the username was 'user', the realm was 'realm', and the password was
           'pass', then the 16-byte HMAC key would be the result of performing
           an MD5 hash on the string 'user:realm:pass', the resulting hash being
           0x8493fbc53ba582fb4c044c456bdc40eb.

           For short-term credentials:

           key = SASLprep(password)

           where MD5 is defined in RFC 1321 [RFC1321] and SASLprep() is defined
           in RFC 4013 [RFC4013].

           The structure of the key when used with long-term credentials
           facilitates deployment in systems that also utilize SIP.  Typically,
           SIP systems utilizing SIP's digest authentication mechanism do not
           actually store the password in the database.  Rather, they store a
           value called H(A1), which is equal to the key defined above.

           Based on the rules above, the hash used to construct MESSAGE-
           INTEGRITY includes the length field from the STUN message header.
           Prior to performing the hash, the MESSAGE-INTEGRITY attribute MUST be
           inserted into the message (with dummy content).  The length MUST then
           be set to point to the length of the message up to, and including,
           the MESSAGE-INTEGRITY attribute itself, but excluding any attributes
           after it.  Once the computation is performed, the value of the
           MESSAGE-INTEGRITY attribute can be filled in, and the value of the
           length in the STUN header can be set to its correct value -- the
           length of the entire message.  Similarly, when validating the
           MESSAGE-INTEGRITY, the length field should be adjusted to point to
           the end of the MESSAGE-INTEGRITY attribute prior to calculating the
           HMAC.  Such adjustment is necessary when attributes, such as
           FINGERPRINT, appear after MESSAGE-INTEGRITY.
         */
        public MessageIntegrityAttribute MessageIntegrity { get; private set; }

        /*
         * 15.5.  FINGERPRINT

           The FINGERPRINT attribute MAY be present in all STUN messages.  The
           value of the attribute is computed as the CRC-32 of the STUN message
           up to (but excluding) the FINGERPRINT attribute itself, XOR'ed with
           the 32-bit value 0x5354554e (the XOR helps in cases where an
           application packet is also using CRC-32 in it).  The 32-bit CRC is
           the one defined in ITU V.42 [ITU.V42.2002], which has a generator
           polynomial of x32+x26+x23+x22+x16+x12+x11+x10+x8+x7+x5+x4+x2+x+1.
           When present, the FINGERPRINT attribute MUST be the last attribute in
           the message, and thus will appear after MESSAGE-INTEGRITY.

           The FINGERPRINT attribute can aid in distinguishing STUN packets from
           packets of other protocols.  See Section 8.

           As with MESSAGE-INTEGRITY, the CRC used in the FINGERPRINT attribute
           covers the length field from the STUN message header.  Therefore,
           this value must be correct and include the CRC attribute as part of
           the message length, prior to computation of the CRC.  When using the
           FINGERPRINT attribute in a message, the attribute is first placed
           into the message with a dummy value, then the CRC is computed, and
           then the value of the attribute is updated.  If the MESSAGE-INTEGRITY
           attribute is also present, then it must be present with the correct
           message-integrity value before the CRC is computed, since the CRC is
           done over the value of the MESSAGE-INTEGRITY attribute as well.
        */
        public FingerprintAttribute Fingerprint
        {
            get
            {
                var messageBytes = GetAsBytes();

                var additionalBytes = new List<byte>();

                var attr = MessageIntegrity;
                if (attr != null)
                {
                    additionalBytes.AddRange(BitConverter.GetBytes((ushort)attr.Type).Reverse().ToArray());
                    additionalBytes.AddRange(BitConverter.GetBytes(attr.Length).Reverse().ToArray());
                    additionalBytes.AddRange(attr.Value);

                    messageBytes = messageBytes.Concat(additionalBytes).ToArray();
                }

                var crc32 = Force.Crc32.Crc32Algorithm.Compute(messageBytes);
                return new FingerprintAttribute
                {
                    Value = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(crc32 ^ 0x5354554e)))
                };
            }
        }
    }

    public enum StunMessageType : short
    {
        BindingRequest = 0x0001,
        BindingSuccessResponse = 0x0101,
        BindingIndication = 0x0011
    }

    public class StunAttribute
    {
        public virtual StunAttributeType Type { get; set; }
        public ushort Length => (ushort)(Value is null ? 0 : Value.Length);
        public byte Padding
        {
            get
            {
                var stunAttributePaddingRemainder = (byte)(Length % 4);
                var stunAttributePadding = (byte)(stunAttributePaddingRemainder == 0 ? 0 : 4 - stunAttributePaddingRemainder);
                return stunAttributePadding;
            }
        }
        public byte[] Value { get; set; }
    }

    public class UsernameAttribute : StunAttribute
    {
        public override StunAttributeType Type => StunAttributeType.Username;
        public string Username
        {
            get => Encoding.UTF8.GetString(Value);
            set => Value = value is null ? null : Encoding.UTF8.GetBytes(value);
        }
    }

    public class IceControlledAttribute : StunAttribute
    {
        public override StunAttributeType Type => StunAttributeType.IceControlled;
        public byte[] TieBreaker
        {
            get => Value;
        }
    }

    public class IceControllingAttribute : StunAttribute
    {
        public override StunAttributeType Type => StunAttributeType.IceControlling;
        public ulong TieBreaker
        {
            get => BitConverter.ToUInt64(Value);
            set => Value = BitConverter.GetBytes(value);
        }
    }

    public class PriorityAttribute : StunAttribute
    {
        public override StunAttributeType Type => StunAttributeType.Priority;
        public int Priority
        {
            get => BitConverter.ToInt32(Value);
            set => Value = BitConverter.GetBytes(value);
        }
    }

    public class MessageIntegrityAttribute : StunAttribute
    {
        public override StunAttributeType Type => StunAttributeType.MessageIntegrity;
        public string HmacSha1
        {
            get => string.Concat(Value.Select(l => l.ToString("x2")));
        }
    }

    public class FingerprintAttribute : StunAttribute
    {
        public override StunAttributeType Type => StunAttributeType.Fingerprint;
        public int Crc32
        {
            get => BitConverter.ToInt32(Value);
            set => Value = BitConverter.GetBytes(value);
        }
    }

    /*
       15.2.  XOR-MAPPED-ADDRESS

       The XOR-MAPPED-ADDRESS attribute is identical to the MAPPED-ADDRESS
       attribute, except that the reflexive transport address is obfuscated
       through the XOR function.

       The format of the XOR-MAPPED-ADDRESS is:

        0                   1                   2                   3
        0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |x x x x x x x x|    Family     |         X-Port                |
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        |                X-Address (Variable)
        +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

       Figure 6: Format of XOR-MAPPED-ADDRESS Attribute

       The Family represents the IP address family, and is encoded
       identically to the Family in MAPPED-ADDRESS.

       X-Port is computed by taking the mapped port in host byte order,
       XOR'ing it with the most significant 16 bits of the magic cookie, and
       then the converting the result to network byte order.  If the IP
       address family is IPv4, X-Address is computed by taking the mapped IP
       address in host byte order, XOR'ing it with the magic cookie, and
       converting the result to network byte order.  If the IP address
       family is IPv6, X-Address is computed by taking the mapped IP address
       in host byte order, XOR'ing it with the concatenation of the magic
       cookie and the 96-bit transaction ID, and converting the result to
       network byte order.

       The rules for encoding and processing the first 8 bits of the
       attribute's value, the rules for handling multiple occurrences of the
       attribute, and the rules for processing address families are the same
       as for MAPPED-ADDRESS.

       Note: XOR-MAPPED-ADDRESS and MAPPED-ADDRESS differ only in their
       encoding of the transport address.  The former encodes the transport
       address by exclusive-or'ing it with the magic cookie.  The latter
       encodes it directly in binary.  RFC 3489 originally specified only
       MAPPED-ADDRESS.  However, deployment experience found that some NATs
       rewrite the 32-bit binary payloads containing the NAT's public IP
       address, such as STUN's MAPPED-ADDRESS attribute, in the well-meaning
       but misguided attempt at providing a generic ALG function.  Such
       behavior interferes with the operation of STUN and also causes
       failure of STUN's message-integrity checking.
     */
    public class XorMappedAddressAttribute : StunAttribute
    {
        public override StunAttributeType Type => StunAttributeType.XorMappedAddress;
        public IPEndPoint Endpoint
        {
            //get => BitConverter.ToInt32(Value);
            set
            {
                var val = new byte[8];

                val[0] = 0;
                val[1] = 1; // ipV4

                var portBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(value.Port ^ (StunRecord.MessageCookie >> 16))));
                val[2] = portBytes[0];
                val[3] = portBytes[1];
                //IP (XOR-d): e1baa564

                var ipv4int = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(value.Address.GetAddressBytes(), 0));
                var ipBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)(ipv4int ^ StunRecord.MessageCookie)));
                val[4] = ipBytes[0];
                val[5] = ipBytes[1];
                val[6] = ipBytes[2];
                val[7] = ipBytes[3];

                Value = val;
            }
        }
    }

    public enum StunAttributeType : ushort
    {
        MappedAddress = 0x0001,
        Username = 0x0006,
        MessageIntegrity = 0x0008,
        ErrorCode = 0x0009,
        UnknownAttributes = 0x000A,
        Realm = 0x0014,
        Nonce = 0x0015,
        XorMappedAddress = 0x0020,
        Software = 0x8022,
        AlternateServer = 0x8023,
        Fingerprint = 0x8028,
        Priority = 0x0024,
        UseCandidate = 0x0025,
        IceControlled = 0x8029,
        IceControlling = 0x802A
    }
}
