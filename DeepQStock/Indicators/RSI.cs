﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeepQStock.Indicators
{
    public class RSI : IStockExchangeIndicator
    {
        #region << IStockExchangeIndicator Members >>

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<float> GetValue(Period period)
        {
            return null;
        }

        #endregion
    }
}
