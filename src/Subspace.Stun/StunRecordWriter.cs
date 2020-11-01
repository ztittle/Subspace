using System;
using System.Buffers.Binary;
using System.IO;

namespace Subspace.Stun
{
    public class StunRecordWriter
    {
        public static void Write(StunRecord record, Stream stream)
        {
            var padLen = record.MessageIntegrity is null ? 8 : 32;

            var bytes = new byte[StunConstants.RecordHeaderLength + record.MessageLength + padLen];

            var idx = 0;

            BinaryPrimitives.WriteInt16BigEndian(bytes.AsSpan(idx), (short)record.MessageType);
            idx += 2;
            BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(idx), (ushort)(record.MessageLength + padLen /*size of MessageIntegrity and Fingerprint*/));
            idx += 2;
            BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(idx), StunRecord.MessageCookie);
            idx += 4;
            record.MessageTransactionId.CopyTo(bytes.AsSpan(idx));
            idx += record.MessageTransactionId.Length;

            foreach (var attr in record.StunAttributes)
            {
                BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(idx), (ushort)attr.Type);
                idx += 2;
                BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(idx), attr.Length);
                idx += 2;
                if (attr.Value != null)
                {
                    attr.Value.CopyTo(bytes.AsSpan(idx));
                    idx += attr.Value.Length;
                }
                idx += attr.Padding;
            }

            var miRec = record.MessageIntegrity;
            if (miRec != null)
            {
                BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(idx), (ushort)miRec.Type);
                idx += 2;
                BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(idx), miRec.Length);
                idx += 2;
                miRec.Value.CopyTo(bytes.AsSpan(idx));
                idx += miRec.Value.Length;
            }

            var fiRec = record.Fingerprint;
            BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(idx), (ushort)fiRec.Type);
            idx += 2;
            BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(idx), fiRec.Length);
            idx += 2;
            fiRec.Value.CopyTo(bytes.AsSpan(idx));

            stream.Write(bytes);
        }
    }
}
