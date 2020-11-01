using System.Collections.Concurrent;

namespace Subspace.Stun
{
    public class IceInMemoryStunUserProvider : IStunUserProvider
    {
        private readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

        public void AddUser(string username, string password)
        {
            _users.AddOrUpdate(username, k => password, (k, v) => password);
        }

        /// <summary>
        /// A Binding request serving as a connectivity check MUST utilize the
        /// STUN short-term credential mechanism.  The username for the
        /// credential is formed by concatenating the username fragment provided
        /// by the peer with the username fragment of the agent sending the
        /// request, separated by a colon (":").  The password is equal to the
        /// password provided by the peer.  For example, consider the case where
        /// agent L is the offerer, and agent R is the answerer.  Agent L
        /// included a username fragment of LFRAG for its candidates and a
        /// password of LPASS.  Agent R provided a username fragment of RFRAG and
        /// a password of RPASS.  A connectivity check from L to R utilizes the
        /// username RFRAG:LFRAG and a password of RPASS.  A connectivity check
        /// from R to L utilizes the username LFRAG:RFRAG and a password of
        /// LPASS.  The responses utilize the same usernames and passwords as the
        /// requests (note that the USERNAME attribute is not present in the
        /// response).
        /// 
        /// https://tools.ietf.org/html/rfc5245#section-7.1.2.3
        /// </summary>
        public string GetPassword(string username)
        {
            if (_users.TryGetValue(username.Split(':')[0], out var userPassword))
            {
                return userPassword;
            }

            return null;
        }
    }
}
