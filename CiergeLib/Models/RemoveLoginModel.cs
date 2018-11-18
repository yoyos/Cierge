using System;
using System.Collections.Generic;
using System.Text;

namespace CiergeLib.Models
{
    public class RemoveLoginModel
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
    }
}
