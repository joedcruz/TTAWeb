using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TTAWeb
{
    public class WebMenuModel
    {
        public int ItemId { get; set; }

        public string DisplayName { get; set; }

        public int OrderNo { get; set; }

        public int ParentId { get; set; }

        public string FormId { get; set; }
    }
}
