﻿using System;
using System.Configuration;
using System.Reflection;

namespace ForceDNS.DataAccess
{
    class DataAccessConfiguration
    {

        internal static String DataAccessPath { get; set; }

        static DataAccessConfiguration()
        {
            String dataAccessFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DataAccessPath = System.IO.Path.Combine(dataAccessFolder, "ForceDNS.Data.xml");
        }
    }
}