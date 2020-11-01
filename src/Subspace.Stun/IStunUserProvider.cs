namespace Subspace.Stun
{
    public interface IStunUserProvider
    {
        void AddUser(string username, string password);
        string GetPassword(string username);
    }
}
