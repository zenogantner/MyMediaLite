/*
 * SVM.NET Library
 * Copyright (C) 2008 Matthew Johnson
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using System;
using System.Linq;
using System.Collections.Generic;

namespace SVM
{
    /// <summary>
    /// Contains all of the types of SVM this library can model.
    /// </summary>
    public enum SvmType { 
        /// <summary>
        /// C-SVC.
        /// </summary>
        C_SVC, 
        /// <summary>
        /// nu-SVC.
        /// </summary>
        NU_SVC, 
        /// <summary>
        /// one-class SVM
        /// </summary>
        ONE_CLASS, 
        /// <summary>
        /// epsilon-SVR
        /// </summary>
        EPSILON_SVR, 
        /// <summary>
        /// nu-SVR
        /// </summary>
        NU_SVR 
    };
    /// <summary>
    /// Contains the various kernel types this library can use.
    /// </summary>
    public enum KernelType { 
        /// <summary>
        /// Linear: u'*v
        /// </summary>
        LINEAR, 
        /// <summary>
        /// Polynomial: (gamma*u'*v + coef0)^degree
        /// </summary>
        POLY, 
        /// <summary>
        /// Radial basis function: exp(-gamma*|u-v|^2)
        /// </summary>
        RBF, 
        /// <summary>
        /// Sigmoid: tanh(gamma*u'*v + coef0)
        /// </summary>
        SIGMOID,
        /// <summary>
        /// Precomputed kernel
        /// </summary>
        PRECOMPUTED,
    };

    /// <summary>
    /// This class contains the various parameters which can affect the way in which an SVM
    /// is learned.  Unless you know what you are doing, chances are you are best off using
    /// the default values.
    /// </summary>
	[Serializable]
	public class Parameter : ICloneable
	{
        private SvmType _svmType;
        private KernelType _kernelType;
        private int _degree;
        private double _gamma;
        private double _coef0;

        private double _cacheSize;
        private double _C;
        private double _eps;

        private Dictionary<int, double> _weights;
        private double _nu;
        private double _p;
        private bool _shrinking;
        private bool _probability;

        /// <summary>
        /// Default Constructor.  Gives good default values to all parameters.
        /// </summary>
        public Parameter()
        {
            _svmType = SvmType.C_SVC;
            _kernelType = KernelType.RBF;
            _degree = 3;
            _gamma = 0; // 1/k
            _coef0 = 0;
            _nu = 0.5;
            _cacheSize = 40;
            _C = 1;
            _eps = 1e-3;
            _p = 0.1;
            _shrinking = true;
            _probability = false;
            _weights = new Dictionary<int, double>();
        }

        /// <summary>
        /// Type of SVM (default C-SVC)
        /// </summary>
        public SvmType SvmType
        {
            get
            {
                return _svmType;
            }
            set
            {
                _svmType = value;
            }
        }
        /// <summary>
        /// Type of kernel function (default Polynomial)
        /// </summary>
        public KernelType KernelType
        {
            get
            {
                return _kernelType;
            }
            set
            {
                _kernelType = value;
            }
        }
        /// <summary>
        /// Degree in kernel function (default 3).
        /// </summary>
        public int Degree
        {
            get
            {
                return _degree;
            }
            set
            {
                _degree = value;
            }
        }
        /// <summary>
        /// Gamma in kernel function (default 1/k)
        /// </summary>
        public double Gamma
        {
            get
            {
                return _gamma;
            }
            set
            {
                _gamma = value;
            }
        }
        /// <summary>
        /// Zeroeth coefficient in kernel function (default 0)
        /// </summary>
        public double Coefficient0
        {
            get
            {
                return _coef0;
            }
            set
            {
                _coef0 = value;
            }
        }
		
        /// <summary>
        /// Cache memory size in MB (default 100)
        /// </summary>
        public double CacheSize
        {
            get
            {
                return _cacheSize;
            }
            set
            {
                _cacheSize = value;
            }
        }
        /// <summary>
        /// Tolerance of termination criterion (default 0.001)
        /// </summary>
        public double EPS
        {
            get
            {
                return _eps;
            }
            set
            {
                _eps = value;
            }
        }
        /// <summary>
        /// The parameter C of C-SVC, epsilon-SVR, and nu-SVR (default 1)
        /// </summary>
        public double C
        {
            get
            {
                return _C;
            }
            set
            {
                _C = value;
            }
        }

        /// <summary>
        /// Contains custom weights for class labels.  Default weight value is 1.
        /// </summary>
        public Dictionary<int,double> Weights
        {
            get{
                return _weights;
            }
        }

        /// <summary>
        /// The parameter nu of nu-SVC, one-class SVM, and nu-SVR (default 0.5)
        /// </summary>
        public double Nu
        {
            get
            {
                return _nu;
            }
            set
            {
                _nu = value;
            }
        }
        /// <summary>
        /// The epsilon in loss function of epsilon-SVR (default 0.1)
        /// </summary>
        public double P
        {
            get
            {
                return _p;
            }
            set
            {
                _p = value;
            }
        }
        /// <summary>
        /// Whether to use the shrinking heuristics, (default True)
        /// </summary>
        public bool Shrinking
        {
            get
            {
                return _shrinking;
            }
            set
            {
                _shrinking = value;
            }
        }
        /// <summary>
        /// Whether to train an SVC or SVR model for probability estimates, (default False)
        /// </summary>
        public bool Probability
        {
            get
            {
                return _probability;
            }
            set
            {
                _probability = value;
            }
        }


        #region ICloneable Members
        /// <summary>
        /// Creates a memberwise clone of this parameters object.
        /// </summary>
        /// <returns>The clone (as type Parameter)</returns>
        public object Clone()
        {
            return base.MemberwiseClone();
        }

        #endregion
    }
}