using System;

namespace Kaye.Profiles
{
    public readonly struct Profile
    {
        readonly public String Username, Password, Hash;
        readonly public HashAlg Algorithm;

        public Profile(String Username, String Password)
        {
            this.Username = Username;
            this.Password = Password;
            this.Hash = String.Empty;
            this.Algorithm = HashAlg.None;
        }

        public Profile(String Username, String Hash, HashAlg Algorithm)
        {
            this.Username = Username;
            this.Password = String.Empty;
            this.Hash = Hash;
            this.Algorithm = Algorithm;
        }
    }
}