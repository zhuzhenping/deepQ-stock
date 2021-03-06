﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepQStock.Storage
{
    public class DeepQStockDbConfiguration : DbConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeepQStockDbConfiguration"/> class.
        /// </summary>
        public DeepQStockDbConfiguration()
        {
            var connString = new System.Data.Entity.Infrastructure.SqlConnectionFactory();
            
            SetDefaultConnectionFactory(connString);
            SetProviderServices("System.Data.SqlClient", System.Data.Entity.SqlServer.SqlProviderServices.Instance);
            SetDatabaseInitializer(new MigrateDatabaseToLatestVersion<DeepQStockContext, Migrations.Configuration>());
        }
    }
}
