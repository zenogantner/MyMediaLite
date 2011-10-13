public class SimpleMatrixFactorization : RatingPredictor, IIterativeModel
{
	Matrix<double> user_factors;
	Matrix<double> item_factors;

	public uint NumFactors { get; set;}
	public double LearnRate { get; set; }
	public virtual double Regularization { get; set; }
	public uint NumIter { get; set; }

	public override void Train()
	{
		// init factor matrices
		user_factors = new Matrix<double>(MaxUserID + 1, NumFactors);
		item_factors = new Matrix<double>(MaxItemID + 1, NumFactors);
		MatrixUtils.InitNormal(user_factors, 0, 0.1);
		MatrixUtils.InitNormal(item_factors, 0, 0.1);

		// learn model parameters
		for (uint current_iter = 0; current_iter < NumIter; current_iter++)
			Iterate();
	}

	public void Iterate()
	{
		foreach (int index in ratings.RandomIndex)
		{
			int u = ratings.Users[index];
			int i = ratings.Items[index];

			double p = Predict(u, i);
			double err = ratings[index] - p;

			for (int f = 0; f < NumFactors; f++)
			{
				double u_f = user_factors[u, f];
				double i_f = item_factors[i, f];
				MatrixUtils.Inc(user_factors, u, f, LearnRate * (err * i_f - Regularization * u_f));
				MatrixUtils.Inc(item_factors, i, f, LearnRate * (err * u_f - Regularization * i_f));
			}
		}
	}

	public override double Predict(int user_id, int item_id)
	{
		return MatrixUtils.RowScalarProduct(user_factors, user_id, item_factors, item_id);
	}
}

