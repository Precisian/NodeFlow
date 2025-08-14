using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class NodeModel
    {
        public string TaskName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Assignee { get; set; }
        public string NodeTitle { get; set; }
        public string NodeHeaderColor { get; set; } = "LightGray";
    }
}