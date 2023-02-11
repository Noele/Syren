using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.DataTypes
{
    public class ImageRequest
    {
        public string prompt { get; set; }
        public int n { get; set; }
        public string size { get; set; }
        public string response_format { get; set; }
        public string user { get; set; }
    }
}
