using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.DataTypes
{
    public class AiPrivateConversation
    {
        public ulong id { get; set; }
        public List<AiRequestMessages> messages { get; set; }
    }
    public class AiRequestMessages {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class AiRequest
    {
        public List<AiRequestMessages> messages { get; set; }

        public string model { get; set; }
 
        public int max_tokens { get; set; }
        public double temperature { get; set; }

    }
}
