﻿using DeepQStock.Domain;
using DeepQStock.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeepQStock.Indicators
{
    /// <summary>
    /// The Relative Strength Index (RSI) is a well versed momentum based oscillator which is used to measure the speed (velocity) 
    /// as well as the change (magnitude) of directional price movements. Essentially RSI, when graphed, 
    /// provides a visual mean to monitor both the current, as well as historical, strength and weakness of a particular market.
    /// The strength or weakness is based on closing prices over the duration of a specified trading period creating a reliable 
    /// metric of price and momentum changes. Given the popularity of cash settled instruments (stock indexes) 
    /// and leveraged financial products (the entire field of derivatives); RSI has proven to be a viable indicator of price movements.
    /// https://www.tradingview.com/wiki/Relative_Strength_Index_(RSI)
    /// </summary>
    public class RSI : TechnicalIndicatorBase, ITechnicalIndicator
    {
        #region << Properties >>

        /// <summary>
        /// The number of periods to use for calculate the rsi
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Upward Periods used in the calculation
        /// </summary>
        public ExponentialMovingAverage UpwardPeriods{ get; set; }

        /// <summary>
        /// Upward Periods used in the calculation
        /// </summary>
        public ExponentialMovingAverage DownwardPeriods { get; set; }

        /// <summary>
        /// Previous Period
        /// </summary>
        public Period PreviousPeriod{ get; set; }

        #endregion

        #region << Constructor >> 

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="size"></param>
        public RSI(PeriodType type, int length = 14): base(type)
        {
            Length = length;
            UpwardPeriods = new ExponentialMovingAverage(type, Length);
            DownwardPeriods = new ExponentialMovingAverage(type, Length);
        }

        #endregion

        #region << IStockExchangeIndicator Members >>

        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name { get { return "RSI"; } }       

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<double> Update(Period period)
        {
            double emaU = UpwardPeriods.Value != null ? UpwardPeriods.Value.First() : 0.0;
            double emaD = DownwardPeriods.Value != null ? DownwardPeriods.Value.First() : 0.0;
            double rsi = 0.0;            

            if (PreviousPeriod != null)
            {
                if (PreviousPeriod.Close <= period.Close)
                {
                    emaU = UpwardPeriods.Update(period).First();
                }
                else
                {
                    emaU = DownwardPeriods.Update(period).First();
                }

                if (emaD > 0.0)
                {
                    var rs = emaU / emaD;
                    rsi = 100.0 - (100.0 / (1.0 + rs));
                }                
            }

            PreviousPeriod = period;

            Value = new double[1] { rsi };

            return Value;
        }

        #endregion
    }
}
