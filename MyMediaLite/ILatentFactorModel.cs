// Copyright (C) 2010 Zeno Gantner
//
// This file is part of MyMediaLite.
//
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using MyMediaLite.data_type;
using MyMediaLite.taxonomy;


namespace MyMediaLite
{
	/// <summary>
	/// Interface for latent factor models like matrix factorization
	/// </summary>
	public interface ILatentFactorModel
	{
		/// <summary>Get the latent factors that describe an entity in the latent factor model</summary>
		/// <param name="entity_type">the entity type</param>
		/// <param name="id">the ID of the entity</param>
		/// <returns>an array of doubles that describes the entity in the model</returns>
		double[] GetLatentFactors(EntityType entity_type, int id);

		/// <summary>Get the latent factors that describe all entities of one type in the latent factor model</summary>
		/// <param name="entity_type">the entity type</param>
		/// <returns>the matrix of all latent factors of the given entity type in the model</returns>
		Matrix<double> GetLatentFactors(EntityType entity_type);
		
		/// <summary>Get the bias terms for a given entity type</summary>
		/// <param name="entity_type">the entity type</param>
		/// <returns>an array of doubles containing the entity biases, all values are 0 if there are no bias terms in the model</returns>
		double[] GetEntityBiases(EntityType entity_type);
		
		/// <summary>Get the global bias</summary>
		/// <returns>the global bias, 0 if it does not exist</returns>
		double GetGlobalBias();
	}
}