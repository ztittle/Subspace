using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

namespace Subspace.Rtp.Srtp
{
    public class SrtpEncryptor
    {
        /// <summary>
        ///     A cryptographic context SHALL be uniquely identified by the triplet
        /// context identifier:
        /// 
        /// context id = &lt;SSRC, destination network address, destination
        /// transport port number&gt;
        /// </summary>
        private readonly ConcurrentDictionary<SrtpStreamContextKey, SrtpStreamContext> _streams = new ConcurrentDictionary<SrtpStreamContextKey, SrtpStreamContext>();

        /// <summary>
        /// The encryption transforms defined in SRTP map the SRTP packet index
        /// and secret key into a pseudo-random keystream segment.  Each
        /// keystream segment encrypts a single RTP packet.  The process of
        /// encrypting a packet consists of generating the keystream segment
        /// corresponding to the packet, and then bitwise exclusive-oring that
        /// keystream segment onto the payload of the RTP packet to produce the
        /// Encrypted Portion of the SRTP packet.  
        /// </summary>
        public byte[] Encrypt(SrtpPolicy srtpPolicy, RtpPacket rtp, IPEndPoint remoteEndPoint)
        {
            if (rtp.RawBytes.Length < rtp.Payload.Offset)
            {
                throw new InvalidOperationException("invalid rtp bytes");
            }

            /*3.3.  SRTP Packet Processing

               The following applies to SRTP.  SRTCP is described in Section 3.4.

               Assuming initialization of the cryptographic context(s) has taken
               place via key management, the sender SHALL do the following to
               construct an SRTP packet:

               1. Determine which cryptographic context to use as described in
                  Section 3.2.3.
             */

            var srtpStream = DetermineCryptoStreamContext(rtp, remoteEndPoint);

            /*
               2. Determine the index of the SRTP packet using the rollover counter,
                  the highest sequence number in the cryptographic context, and the
                  sequence number in the RTP packet, as described in Section 3.3.1.
            */
            var packetIndex = DetermineSrtpPacketIndex(rtp, srtpStream);

            /*
               3. Determine the master key and master salt.  This is done using the
                  index determined in the previous step or the current MKI in the
                  cryptographic context, according to Section 8.1.

               4. Determine the session keys and session salt (if they are used by
                  the transform) as described in Section 4.3, using master key,
                  master salt, key_derivation_rate, and session key-lengths in the
                  cryptographic context with the index, determined in Steps 2 and 3.
            */

            DeriveKeys(srtpStream, packetIndex, srtpPolicy.MasterKey, srtpPolicy.MasterSalt);

            var derivedKeys = srtpStream.DerivedKeys;

            /*
               5. Encrypt the RTP payload to produce the Encrypted Portion of the
                  packet (see Section 4.1, for the defined ciphers).  This step uses
                  the encryption algorithm indicated in the cryptographic context,
                  the session encryption key and the session salt (if used) found in
                  Step 4 together with the index found in Step 2.
             */

            var rtpBody = rtp.Payload.ToArray();

            var encryptedRtpPayload = EncryptPayloadWithAesCtrMode(derivedKeys, rtp.SynchronizationSource, rtpBody, packetIndex);

            var rtpHeaderLength = rtp.Payload.Offset;
            var encryptedHeaderPayload =
                rtp.RawBytes.Take(rtpHeaderLength)
                    .Concat(encryptedRtpPayload.Take(rtpBody.Length))
                    .ToArray();

            var hash = HashUtils.HmacSha1(
                encryptedHeaderPayload
                    .Concat(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(srtpStream.RollOverCounter))).ToArray(),
                derivedKeys.AuthKey);

            var encryptedBytes = encryptedHeaderPayload
                .Concat(hash.Take(10))
                .ToArray();

            return encryptedBytes;
        }

        private static byte[] EncryptPayloadWithAesCtrMode(SrtpDerivedkeys derivedKeys, uint ssrc, byte[] rtpBody, int packetIndex)
        {
            var sessionKey = derivedKeys.SessionKey;
            var cipherSalt = derivedKeys.CipherSalt;

            // where the 128-bit integer value IV SHALL be defined by the SSRC, the
            // SRTP packet index i, and the SRTP session salting key k_s, as below.
            // 
            //  IV = (k_s * 2^16) XOR (SSRC * 2^64) XOR (i * 2^16)

            var iv =
                cipherSalt.ShiftLeft(16)
                    .Xor(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ssrc)).ShiftLeft(64))
                    .Xor(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(packetIndex)).ShiftLeft(16));

            var encrypted = AesCounterMode.Encrypt(sessionKey, iv, rtpBody);

            return encrypted;
        }

        /// <summary>
        /// SRTP implementations use an "implicit" packet index for sequencing,
        /// i.e., not all of the index is explicitly carried in the SRTP packet.
        /// For the pre-defined transforms, the index i is used in replay
        /// protection (Section 3.3.2), encryption (Section 4.1), message
        /// authentication (Section 4.2), and for the key derivation (Section
        /// 4.3).
        /// 
        /// https://tools.ietf.org/html/rfc3711#section-3.3.1
        /// </summary>
        private static int DetermineSrtpPacketIndex(RtpPacket rtp, SrtpStreamContext srtpStreamContext)
        {
            if (rtp.SequenceNumber - srtpStreamContext.PreviousSequenceNumber < ushort.MinValue)
            {
                srtpStreamContext.RollOverCounter++;
            }

            srtpStreamContext.PreviousSequenceNumber = rtp.SequenceNumber;

            var packetIndex = srtpStreamContext.RollOverCounter << 16 | rtp.SequenceNumber;

            return packetIndex;
        }

        /// <summary>
        /// Recall that an RTP session for each participant is defined [RFC3550]
        /// by a pair of destination transport addresses (one network address
        /// plus a port pair for RTP and RTCP), and that a multimedia session is
        /// defined as a collection of RTP sessions.  For example, a particular
        /// multimedia session could include an audio RTP session, a video RTP
        /// session, and a text RTP session.
        /// 
        /// A cryptographic context SHALL be uniquely identified by the triplet
        /// context identifier:
        /// 
        /// context id = &lt;SSRC, destination network address, destination
        /// transport port number&gt;
        /// 
        /// where the destination network address and the destination transport
        /// port are the ones in the SRTP packet.  It is assumed that, when
        /// presented with this information, the key management returns a context
        /// with the information as described in Section 3.2.
        /// 
        /// As noted above, SRTP and SRTCP by default share the bulk of the
        /// parameters in the cryptographic context.  Thus, retrieving the crypto
        /// context parameters for an SRTCP stream in practice may imply a
        /// binding to the correspondent SRTP crypto context.  It is up to the
        /// implementation to assure such binding, since the RTCP port may not be
        /// directly deducible from the RTP port only.  Alternatively, the key
        /// management may choose to provide separate SRTP- and SRTCP- contexts,
        /// duplicating the common parameters (such as master key(s)).  The
        /// latter approach then also enables SRTP and SRTCP to use, e.g.,
        /// distinct transforms, if so desired.  Similar considerations arise
        /// when multiple SRTP streams, forming part of one single RTP session,
        /// share keys and other parameters.
        /// 
        /// If no valid context can be found for a packet corresponding to a
        /// certain context identifier, that packet MUST be discarded.
        ///
        /// https://tools.ietf.org/html/rfc3711#section-3.2.3
        /// </summary>
        private SrtpStreamContext DetermineCryptoStreamContext(RtpPacket rtp, IPEndPoint remoteEndPoint)
        {
            var key = new SrtpStreamContextKey(rtp.SynchronizationSource, remoteEndPoint);

            if (_streams.TryGetValue(key, out var srtpStream))
            {
                return srtpStream;
            }

            srtpStream = new SrtpStreamContext
            {
                SynchronizationSource = rtp.SynchronizationSource
            };
            _streams.AddOrUpdate(key, k => srtpStream, (k, v) => srtpStream);

            return srtpStream;
        }

        /// <summary>
        /// Regardless of the encryption or message authentication transform that
        /// is employed (it may be an SRTP pre-defined transform or newly
        /// introduced according to Section 6), interoperable SRTP
        /// implementations MUST use the SRTP key derivation to generate session
        /// keys.  Once the key derivation rate is properly signaled at the start
        /// of the session, there is no need for extra communication between the
        /// parties that use SRTP key derivation.
        ///
        /// https://tools.ietf.org/html/rfc3711#section-4.3
        /// </summary>
        private void DeriveKeys(SrtpStreamContext srtpStreamContext, int packetIndex, byte[] masterKey, byte[] masterSalt)
        {
            /*
             * At least one initial key derivation SHALL be performed by SRTP, i.e.,
               the first key derivation is REQUIRED.  Further applications of the
               key derivation MAY be performed, according to the
               "key_derivation_rate" value in the cryptographic context.  The key
               derivation function SHALL initially be invoked before the first
               packet and then, when r > 0, a key derivation is performed whenever
               index mod r equals zero.  This can be thought of as "refreshing" the
               session keys.  The value of "key_derivation_rate" MUST be kept fixed
               for the lifetime of the associated master key.
             */
            /*
               Let "a DIV t" denote integer division of a by t, rounded down, and
               with the convention that "a DIV 0 = 0" for all a.  We also make the
               convention of treating "a DIV t" as a bit string of the same length
               as a, and thus "a DIV t" will in general have leading zeros.

               Let r = index DIV key_derivation_rate (with DIV as defined above).
            */

            var kdr = srtpStreamContext.KeyDerivationRate;

            var r = (uint)(kdr == 0 ? 0 : packetIndex / kdr);

            var shouldDeriveKeys = srtpStreamContext.DerivedKeys is null || r > 0 && packetIndex % r == 0;

            if (!shouldDeriveKeys)
            {
                return;
            }

            var derivedKeys = new SrtpDerivedkeys();

            derivedKeys.SessionKey = DeriveKey(masterKey, masterSalt, r, SrtpEncryptionKeyLabel.SrtpEncryptionKey, SrtpConstants.SrtpDefaultEncryptionSessionKeyLength);

            derivedKeys.AuthKey = DeriveKey(masterKey, masterSalt, r, SrtpEncryptionKeyLabel.SrtpMessageAuthenticationKey, SrtpConstants.SrtpDefaultAuthSessionKeyLength);

            derivedKeys.CipherSalt = DeriveKey(masterKey, masterSalt, r, SrtpEncryptionKeyLabel.SrtpSaltingKey, SrtpConstants.SrtpDefaultSaltSessionKeyLength);

            srtpStreamContext.DerivedKeys = derivedKeys;
        }

        private static byte[] DeriveKey(byte[] masterKey, byte[] masterSalt, uint r, SrtpEncryptionKeyLabel label, int keyLength)
        {
            /*
               Key derivation SHALL be defined as follows in terms of <label>, an
               8-bit constant (see below), master_salt and key_derivation_rate, as
               determined in the cryptographic context, and index, the packet index
               (i.e., the 48-bit ROC || SEQ for SRTP):
            
               *  Let r = index DIV key_derivation_rate (with DIV as defined above).
               
               *  Let key_id = <label> || r.
               
               *  Let x = key_id XOR master_salt, where key_id and master_salt are
               aligned so that their least significant bits agree (right-
               alignment).
             */

            var keyId = new byte[7];
            keyId[0] = (byte)label;

            var rBytes = BitConverter.GetBytes(r);
            rBytes.CopyTo(keyId, 1);

            var x = new byte[masterSalt.Length];

            var keyIdStart = x.Length - keyId.Length;

            for (var i = 0; i < masterSalt.Length; i++)
            {
                if (i < keyIdStart)
                {
                    x[i] = masterSalt[i];
                }
                else
                {
                    x[i] = (byte)(keyId[i - keyIdStart] ^ masterSalt[i]);
                }
            }

            var derivedEncryptionKey = PseudoRandomFunction(masterKey, x, keyLength);

            return derivedEncryptionKey;
        }

        /// <summary>
        /// 4.3.3.  AES-CM PRF
        /// 
        /// The currently defined PRF, keyed by 128, 192, or 256 bit master key,
        /// has input block size m = 128 and can produce n-bit outputs for n up
        /// to 2^23.  PRF_n(k_master,x) SHALL be AES in Counter Mode as described
        /// in Section 4.1.1, applied to key k_master, and IV equal to (x*2^16),
        /// and with the output keystream truncated to the n first (left-most)
        /// bits.  (Requiring n/128, rounded up, applications of AES.)
        ///
        /// https://tools.ietf.org/html/rfc3711#section-4.3.3
        /// </summary>
        private static byte[] PseudoRandomFunction(byte[] masterKey, byte[] x, int keyLength)
        {
            var iv = new byte[SrtpConstants.SrtpPseudoRandomFunctionInputBlockSize];

            x.CopyTo(iv, 0);

            return AesCounterMode.GenerateKeystreamSegment(masterKey, iv, keyLength);
        }
    }
}
