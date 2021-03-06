﻿using DeepQStock.Storage;
using Encog.Engine.Network.Activation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepQStock.Agents
{

    public class LayerParameters : BaseModel
    {
        #region << Constructor >>

        /// <summary>
        /// Initializes a new instance of the <see cref="LayerParameters"/> class.
        /// </summary>
        /// <param name="activationFunction">The activation function.</param>
        /// <param name="bias">if set to <c>true</c> [bias].</param>
        /// <param name="neuronCount">The neuron count.</param>        
        public LayerParameters(IActivationFunction activationFunction, bool bias, int neuronCount)
        {
            ActivationFunction = activationFunction;
            Bias = bias;
            NueronCount = neuronCount;
        }

        #endregion

        #region << Public Properties >>

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LayerParameters"/> is bias.
        /// </summary>
        public bool Bias { get; set; }

        /// <summary>
        /// Gets or sets the nuerons count.
        /// </summary>
        public int NueronCount { get; set; }

        /// <summary>
        /// Gets or sets the activation function.
        /// </summary>        
        [NotMapped]
        public IActivationFunction ActivationFunction { get; set; }

        #endregion
    }


    public class QNetworkParameters : BaseModel
    {
        #region << Constructor >>

        public QNetworkParameters()
        {
            Layers = new List<LayerParameters>();
            TrainingError = 0.00000001;
            MaxIterationPerTrainging = 20;
            HiddenLayersCount = 4;
            NeuronCountForHiddenLayers = HiddenLayersCount * 4;
        }

        #endregion

        #region << Public Properties >>

        /// <summary>
        /// Gets or sets the layers.
        /// </summary>
        [NotMapped]
        public ICollection<LayerParameters> Layers { get; set; }

        /// <summary>
        /// Gets or sets the training error.
        /// </summary>
        public double TrainingError { get; set; }

        /// <summary>
        /// Gets or sets the maximum iteration per trainging.
        /// </summary>
        public int MaxIterationPerTrainging { get; set; }

        /// <summary>
        /// Get or Set the number of hidden layers
        /// </summary>
        public int HiddenLayersCount { get; set; }

        /// <summary>
        /// Get or Set the number of neurons per layer
        /// </summary>
        public int NeuronCountForHiddenLayers { get; set; }

        #endregion
    }
}
