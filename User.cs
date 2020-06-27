using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoSystem;

namespace UserGroup
{
    public class User
    {
        private string UserName;
        private Crypto crypto;

        public User(string uname)
        {
            this.UserName = uname;
            this.crypto = new Crypto(uname);
        }

        public User(string uname, BigInt privateKey, BigInt publicKey)
        {
            UserName = uname;
            this.crypto = new Crypto(publicKey, privateKey);
        }
        public string GetUserName()
        {
            return this.UserName;
        }
        public BigInt[] GetUserKeys()
        {
            return this.crypto.GetKeys();
        }
        
        public string[] Encrypt(string m)
        {
            return crypto.EncodeData(m);
        }

        public string Decrypt(string[] s)
        {
            return crypto.DecodeData(s);
        }
    }
}
