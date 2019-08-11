
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace phynixnetapi.Model.TableModels
{
    public class DriverLocation : TableEntity
    {
        public string DriverId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Status { get; set; }
    }
}
