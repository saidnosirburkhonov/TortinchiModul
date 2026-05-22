using System;
using System.Collections.Generic;
using System.Text;

namespace hello_tg_bot
{
    public class UserData
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDate { get; set; }
        public List<string> History { get; set; } = new List<string>();
    }
}
