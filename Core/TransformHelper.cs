using System;
using System.Windows.Media;
using FantaziaDesign.Core;

namespace FantaziaDesign.Wpf.Core
{
	public static class TransformHelper
	{
		private static Func<GeneralTransform, Transform> GetAffineTransformProperty;

		public static bool TryGetTransform(GeneralTransform generalTransform, out Transform result)
		{
			if (generalTransform is null)
			{
				result = null;
				return false;
			}

			result = generalTransform as Transform;
			if (result is null)
			{
				if (GetAffineTransformProperty is null)
				{
					GetAffineTransformProperty = ReflectionUtil.BindInstancePropertyGetterToDelegate<GeneralTransform, Transform>(
						"AffineTransform", ReflectionUtil.NonPublicInstance, true
						);
				}
				result = GetAffineTransformProperty(generalTransform);
			}
			return result != null;
		}

		public static bool TryGetTransformMatrix(GeneralTransform generalTransform, out Matrix result)
		{
			result = default(Matrix);

			if (generalTransform is null)
			{
				return false;
			}
			Transform transform = generalTransform as Transform;
			if (transform is null)
			{
				if (GetAffineTransformProperty is null)
				{
					GetAffineTransformProperty = ReflectionUtil.BindInstancePropertyGetterToDelegate<GeneralTransform, Transform>(
						"AffineTransform", ReflectionUtil.NonPublicInstance, true
						);
				}
				transform = GetAffineTransformProperty(generalTransform);
				if (transform is null)
				{
					return false;
				}
			}
			result = transform.Value;
			return true;
		}
	}
}
