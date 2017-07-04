﻿using DeepQStock.Domain;
using DeepQStock.Enums;
using DeepQStock.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepQStock.Indicators
{
    public class TechnicalIndicatorBase : BaseModel, ITechnicalIndicator
    {
        public string ClassType { get; set; }

        /// <summary>
        /// Stock Exchange id
        /// </summary>
        public long StockExchangeId { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public double[] Value { get; set; }

        /// <summary>
        /// Indicator Type
        /// </summary>
        public PeriodType Type { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public virtual string Name { get; }


        public TechnicalIndicatorBase()
        {
            ClassType = GetType().FullName;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<double> Update(Period period)
        {
            return null;
        }
    }
}