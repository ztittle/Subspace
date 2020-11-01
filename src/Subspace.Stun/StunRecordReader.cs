using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;

namespace Subspace.Stun
{
    public static class StunRecordReader
    {
        public static StunRecord Read(byte[] bytes)
        {
            var record = new StunRecord();

            var idx = 0;
            record.MessageType = (StunMessageType)BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(idx));
            idx += 2;
            var stunMessageLength = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(idx));
            idx += 2;
            var stunMessageCookie = BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(idx));
            idx += 4;

            if (stunMessageCookie != StunRecord.MessageCookie)
            {
                throw new InvalidOperationException("Malformed STUN packet");
            }

            record.MessageTransactionId = bytes.AsSpan(idx, 12).ToArray();
            idx += 12;
            record.StunAttributes = new List<StunAttribute>();

            while (idx < bytes.Length)
            {
                var stunAttributeType = (StunAttributeType)BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(idx));
                idx += 2;
                var stunAttributeLength = BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(idx));
                idx += 2;
                var stunAttributePaddingRemainder = (byte)(stunAttributeLength % 4);
                var stunAttributePadding = (byte)(stunAttributePaddingRemainder == 0 ? 0 : 4 - stunAttributePaddingRemainder);
                var stunAttributeValue = bytes.AsSpan(idx, stunAttributeLength).ToArray();
                idx += stunAttributeLength;

                if (stunAttributePadding > 0)
                {
                    idx += stunAttributePadding;
                }

                StunAttribute stunAttribute;

                switch (stunAttributeType)
                {
                    case StunAttributeType.Username:
                        stunAttribute = new UsernameAttribute();
                        break;
                    case StunAttributeType.IceControlled:
                        stunAttribute = new IceControlledAttribute();
                        break;
                    case StunAttributeType.Priority:
                        stunAttribute = new PriorityAttribute();
                        break;
                    case StunAttributeType.MessageIntegrity:
                        stunAttribute = new MessageIntegrityAttribute();
                        break;
                    case StunAttributeType.Fingerprint:
                        stunAttribute = new FingerprintAttribute();
                        break;
                    default:
                        stunAttribute = new StunAttribute
                        {
                            Type = stunAttributeType
                        };
                        break;
                }

                stunAttribute.Value = stunAttributeValue;

                if (stunAttribute.Type != StunAttributeType.MessageIntegrity &&
                    stunAttribute.Type != StunAttributeType.Fingerprint)
                {
                    record.StunAttributes.Add(stunAttribute);
                }
            }

            return record;
        }
    }
}
