using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPasswordHasher
    {
        public string Hash(string plainText);
        public bool Verify(string plainText, string hash);
    }
}
