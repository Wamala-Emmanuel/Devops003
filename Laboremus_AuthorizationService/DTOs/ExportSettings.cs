using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.DTOs
{
    public class ExportSettings
    {
        public const string ConfigurationName = "ExportSettings";

        public string Culture { get; set; }
        
        public string FolderPath { get; set; }

        public int Buffer { get; set; }
        
        public int RequestLimit { get; set; }

        public int PageSize { get; set; }

        public int DelayInHours { get; set; }

        public int DaysBack { get; set; }

        public double Offset { get; set; }
    }
}
