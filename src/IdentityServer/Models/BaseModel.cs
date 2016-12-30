using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspIdentityClient.Models
{
    public class BaseModel
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string error { get; set; }

        public object data { get; set; }
    }
}