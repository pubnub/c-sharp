using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PubnubApi
{
    public class UserId
    {
        private string _userId;
        public UserId(string value)
        {
            _userId = value;
        }

        public static implicit operator string(UserId self)
        {
            return self?.ToString();
        }
        public static implicit operator UserId(string value)
        {
            return new UserId(value);
        }
        public override string ToString()
        {
            return _userId;
        }
    }
}
