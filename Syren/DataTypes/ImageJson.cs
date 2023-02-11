using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syren.Syren.DataTypes
{
    public class ImageJson
    {
        public class Data
        {
            public string url { get; set; }
        }

        public class ImageRoot
        {
            public int created { get; set; }
            public List<Data> data { get; set; }
        }


    }
}
