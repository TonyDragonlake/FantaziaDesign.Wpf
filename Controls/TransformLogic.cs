using System.Windows.Media;

namespace FantaziaDesign.Wpf.Controls
{
	//public class TransformLogic
	//{
	//	//<ScaleTransform/>
	//	//<SkewTransform/>
	//	//<RotateTransform/>
	//	//<TranslateTransform/>

	//	Matrix[] m_mat;

	//	public TransformLogic()
	//	{
	//		m_mat = new Matrix[4];
	//		for (int i = 0; i < 4; i++)
	//		{
	//			m_mat[i] = Matrix.Identity;
	//		}
	//	}

	//	internal Matrix ScaleMat { get => m_mat[0]; }
	//	internal Matrix SkewMat { get => m_mat[1]; }
	//	internal Matrix RotateMat { get => m_mat[2]; }
	//	internal Matrix TranslateMat { get => m_mat[3]; }
		
	//	public Matrix AffineMatrix
	//	{
	//		get
	//		{
	//			var result = Matrix.Identity;
	//			if (AllowScale)
	//			{
	//				result.Append(m_mat[0]);
	//			}
	//			if (AllowSkew)
	//			{
	//				result.Append(m_mat[1]);
	//			}
	//			if (AllowRotate)
	//			{
	//				result.Append(m_mat[2]);
	//			}
	//			if (AllowTranslate)
	//			{
	//				result.Append(m_mat[3]);
	//			}
	//			return result;
	//		}
	//	}

	//	public bool AllowScale { get; set; }
	//	public bool AllowSkew { get; set; }
	//	public bool AllowRotate { get; set; }
	//	public bool AllowTranslate { get; set; }

	//	private Matrix UndoInternal(int index)
	//	{
	//		var result = Invert(m_mat[index]);
	//		m_mat[index] = Matrix.Identity;
	//		return result;
	//	}

	//	public Matrix UndoAll()
	//	{
	//		var result = Invert(AffineMatrix);
	//		for (int i = 0; i < 4; i++)
	//		{
	//			m_mat[i] = Matrix.Identity;
	//		}
	//		return result;
	//	}

	//	public void MoveItem(double deltaX, double deltaY)
	//	{
	//		var mat = m_mat[3];
	//		mat.OffsetX += deltaX;
	//		mat.OffsetY += deltaY;
	//		m_mat[3] = mat;
	//	}

	//	public void MoveOrigin(double offsetX, double offsetY)
	//	{
	//		var mat = m_mat[3];
	//		mat.OffsetX = offsetX;
	//		mat.OffsetY = offsetY;
	//		m_mat[3] = mat;
	//	}

	//	public Matrix UndoMove()
	//	{
	//		return UndoInternal(3);
	//	}

	//	public void RotateItem(double angle, double centerX = 0, double centerY = 0)
	//	{
	//		var mat = m_mat[2];
	//		mat.RotateAt(angle, centerX, centerY);
	//		m_mat[2] = mat;
	//	}

	//	public void RotateOrigin(double angle, double centerX = 0, double centerY = 0)
	//	{
	//		var mat = Matrix.Identity;
	//		mat.RotateAt(angle, centerX, centerY);
	//		m_mat[2] = mat;
	//	}

	//	public Matrix UndoRotate()
	//	{
	//		return UndoInternal(2);
	//	}

	//	public void SkewItem(double skewX, double skewY, double centerX = 0, double centerY = 0)
	//	{
	//		var mat = m_mat[1];
	//		Skew(ref mat, skewX, skewY, centerX, centerY);
	//		m_mat[1] = mat;
	//	}

	//	public void SkewOrigin(double skewX, double skewY, double centerX = 0, double centerY = 0)
	//	{
	//		var mat = Matrix.Identity;
	//		Skew(ref mat, skewX, skewY, centerX, centerY);
	//		m_mat[1] = mat;
	//	}

	//	public Matrix UndoSkew()
	//	{
	//		return UndoInternal(1);
	//	}

	//	public void ScaleItem(double scaleX, double scaleY, double centerX = 0, double centerY = 0)
	//	{
	//		var mat = m_mat[0];
	//		if (centerX == 0 && centerY == 0)
	//		{
	//			mat.Scale(scaleX, scaleY);
	//		}
	//		else
	//		{
	//			mat.ScaleAt(scaleX, scaleY, centerX, centerY);
	//		}
	//		m_mat[1] = mat;
	//	}

	//	public void ScaleOrigin(double scaleX, double scaleY, double centerX = 0, double centerY = 0)
	//	{
	//		var mat = Matrix.Identity;
	//		if (centerX == 0 && centerY == 0)
	//		{
	//			mat.Scale(scaleX, scaleY);
	//		}
	//		else
	//		{
	//			mat.ScaleAt(scaleX, scaleY, centerX, centerY);
	//		}
	//		m_mat[1] = mat;
	//	}

	//	public Matrix UndoScale()
	//	{
	//		return UndoInternal(0);
	//	}

	//	public static Matrix Invert(Matrix matrix)
	//	{
	//		matrix.Invert();
	//		return matrix;
	//	}

	//	public static Matrix Transform(Matrix matrix, double offsetX, double offsetY)
	//	{
	//		if (offsetX != 0 || offsetY != 0)
	//		{
	//			matrix.Translate(offsetX, offsetY);
	//		}
	//		return matrix;
	//	}

	//	public static Matrix Rotate(Matrix matrix, double angle, double centerX = 0, double centerY = 0)
	//	{
	//		matrix.RotateAt(angle, centerX, centerY);
	//		return matrix;
	//	}

	//	public static void Skew(ref Matrix matrix, double skewX, double skewY, double centerX = 0, double centerY = 0)
	//	{
	//		bool flag = centerX != 0.0 || centerY != 0.0;
	//		if (flag)
	//		{
	//			matrix.Translate(-centerX, -centerY);
	//		}
	//		matrix.Skew(skewX, skewY);
	//		if (flag)
	//		{
	//			matrix.Translate(centerX, centerY);
	//		}
	//	}

	//	public static Matrix Skew(Matrix matrix, double skewX, double skewY, double centerX = 0, double centerY = 0)
	//	{
	//		bool flag = centerX != 0.0 || centerY != 0.0;
	//		if (flag)
	//		{
	//			matrix.Translate(-centerX, -centerY);
	//		}
	//		matrix.Skew(skewX, skewY);
	//		if (flag)
	//		{
	//			matrix.Translate(centerX, centerY);
	//		}
	//		return matrix;
	//	}

	//	public static Matrix Scale(Matrix matrix, double scaleX, double scaleY, double centerX = 0, double centerY = 0)
	//	{
	//		if (centerX == 0 && centerY == 0)
	//		{
	//			matrix.Scale(scaleX, scaleY);
	//		}
	//		else
	//		{
	//			matrix.ScaleAt(scaleX, scaleY, centerX, centerY);
	//		}
	//		return matrix;
	//	}
	//}
}
