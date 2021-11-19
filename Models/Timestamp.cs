using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrlyBot
{
    [Serializable]
    public class msgTimestamp
    {
        public string UserID { get; set; }
        public string ChannelID { get; set; }
        public DateTime Stamp { get; set; }
        public msgTimestamp(string uid, string cid)
        {
            UserID = uid;
            ChannelID = cid;
            Stamp = DateTime.Now;
        }
    }
}
