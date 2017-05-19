﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepQStock.DeppRLAgent
{
    public class OnTrainingCompleteArgs : EventArgs
    {
    }

    public class OnTrainingEpochCompleteArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the epoch.
        /// </summary>
        public int Epoch { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public double Error { get; set; }
    }
}
