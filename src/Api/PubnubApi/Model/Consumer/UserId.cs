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

        public override string ToString()
        {
            return _userId;
        }
    }
}
