using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Subspace.Stun
{
    public interface IStunHandler
    {
        Socket Socket { set; }
        Task ProcessRequestAsync(byte[] requestBytes, IPEndPoint remoteEndPoint);
        Task SendResponseAsync(IPEndPoint remoteEndPoint, StunRecord responseRecord);
    }

    public class StunHandler : IStunHandler
    {
        private readonly IStunUserProvider _stunUserProvider;
        private Socket _socket;

        public StunHandler(IStunUserProvider stunUserProvider)
        {
            _stunUserProvider = stunUserProvider;
        }

        public Socket Socket
        {
            set => _socket = value;
        }

        public async Task ProcessRequestAsync(byte[] requestBytes, IPEndPoint remoteEndPoint)
        {
            var requestRecord = StunRecordReader.Read(requestBytes);

            StunRecord responseRecord = null;
            if (requestRecord.MessageType == StunMessageType.BindingRequest)
            {
                responseRecord = ProcessBindingRequest(remoteEndPoint, requestRecord);
            }
            else if (requestRecord.MessageType == StunMessageType.BindingSuccessResponse)
            {
                responseRecord = ProcessBindingSuccessResponse(requestRecord);
            }

            if (responseRecord != null)
            {
                await SendResponseAsync(remoteEndPoint, responseRecord);
            }
        }

        public async Task SendResponseAsync(IPEndPoint remoteEndPoint, StunRecord responseRecord)
        {
            var ms = new MemoryStream();
            StunRecordWriter.Write(responseRecord, ms);
            var buffer = ms.GetBuffer();

            await _socket.SendToAsync(new ArraySegment<byte>(buffer, 0, (int) ms.Position), SocketFlags.None, remoteEndPoint);
        }

        private static StunRecord ProcessBindingSuccessResponse(StunRecord requestRecord)
        {
            var responseRecord = new StunRecord
            {
                MessageType = StunMessageType.BindingIndication,
                MessageTransactionId = requestRecord.MessageTransactionId
            };
            return responseRecord;
        }

        private StunRecord ProcessBindingRequest(IPEndPoint remoteEndPoint, StunRecord requestRecord)
        {
            var responseRecord = new StunRecord
            {
                MessageType = StunMessageType.BindingSuccessResponse,
                MessageTransactionId = requestRecord.MessageTransactionId,
                StunAttributes = new List<StunAttribute>
                {
                    new XorMappedAddressAttribute
                    {
                        Endpoint = remoteEndPoint
                    },
                    new UsernameAttribute
                    {
                        Username = requestRecord.StunAttributes.OfType<UsernameAttribute>().FirstOrDefault()?.Username
                    }
                }
            };

            if (requestRecord.StunAttributes.FirstOrDefault(l => l.Type == StunAttributeType.Username) is UsernameAttribute
                userRecord)
            {
                var userPassword = _stunUserProvider.GetPassword(userRecord.Username);
                if (userPassword != null)
                {
                    responseRecord.Sign(userPassword);
                }
                else
                {
                    Debug.WriteLine($"User {userRecord.Username} not found.", nameof(StunHandler));
                }
            }

            return responseRecord;
        }
    }
}
