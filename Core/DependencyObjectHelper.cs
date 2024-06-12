using System;
using System.Windows;
using LinqExpression = System.Linq.Expressions.Expression;
using FantaziaDesign.Core;

namespace FantaziaDesign.Wpf.Core
{
	public static class DependencyObjectHelper
	{
		private static Action<DependencyObject, DependencyProperty, object, PropertyMetadata, bool, bool, byte, bool> setValueCommon;

		public static PropertyMetadata SetupPropertyChange(this DependencyObject dObj, DependencyPropertyKey key, out DependencyProperty dp)
		{
			if (dObj is null)
			{
				throw new ArgumentNullException(nameof(dObj));
			}

			if (key is null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			dp = key.DependencyProperty;
			return dp.GetMetadata(dObj.DependencyObjectType);
		}

		private static void BuildSetValueCommon()
		{
			/*
			 * DependencyProperty dp, 
			 * object value, 
			 * PropertyMetadata metadata, 
			 * bool coerceWithDeferredReference, 
			 * bool coerceWithCurrentValue, 
			 * OperationType operationType, 
			 * bool isInternal
			 * 
			 * Action<DependencyObject, DependencyProperty, object, PropertyMetadata, bool,bool,byte,bool>
			 */
			var typeOfDependencyObject = typeof(DependencyObject);
			var typeOfDependencyProperty = typeof(DependencyProperty);
			var typeOfObject = typeof(object);
			var typeOfPropertyMetadata = typeof(PropertyMetadata);
			var typeOfBoolean = typeof(bool);
			var typeOfByte = typeof(byte);
			var typeOfOperationType = typeOfDependencyObject.Assembly.GetType("System.Windows.OperationType");

			var dObjExpr = LinqExpression.Parameter(typeOfDependencyObject, "dObj");

			var dpExpr = LinqExpression.Parameter(typeOfDependencyProperty, "dp");
			var valueExpr = LinqExpression.Parameter(typeOfObject, "value");
			var metadataExpr = LinqExpression.Parameter(typeOfPropertyMetadata, "metadata");
			var coerceWithDeferredReferenceExpr = LinqExpression.Parameter(typeOfBoolean, "coerceWithDeferredReference");
			var coerceWithCurrentValueExpr = LinqExpression.Parameter(typeOfBoolean, "coerceWithCurrentValue");
			var operationTypeByteExpr = LinqExpression.Parameter(typeOfByte, "operationTypeByte");
			var operationTypeExpr = LinqExpression.Convert(operationTypeByteExpr, typeOfOperationType);
			var isInternalExpr = LinqExpression.Parameter(typeOfBoolean, "isInternal");

			var body = LinqExpression.Block(
				LinqExpression.Call(
					dObjExpr,
					typeOfDependencyObject.GetMethod("SetValueCommon", ReflectionUtil.NonPublicInstance),
					dpExpr,
					valueExpr,
					metadataExpr,
					coerceWithDeferredReferenceExpr,
					coerceWithCurrentValueExpr,
					operationTypeExpr,
					isInternalExpr
					)
				);
			var function = LinqExpression.Lambda<Action<DependencyObject, DependencyProperty, object, PropertyMetadata, bool, bool, byte, bool>>(
				body,
				dObjExpr,
				dpExpr,
				valueExpr,
				metadataExpr,
				coerceWithDeferredReferenceExpr,
				coerceWithCurrentValueExpr,
				operationTypeByteExpr,
				isInternalExpr
				);
			setValueCommon = function.Compile();
		}

		public static void SetDependencyPropertyValueCommon(this DependencyObject dObj, DependencyProperty dp, object value, PropertyMetadata metadata, bool coerceWithDeferredReference, bool coerceWithCurrentValue, byte operationType, bool isInternal)
		{
			if (dObj is null)
			{
				throw new ArgumentNullException(nameof(DependencyObject));
			}

			if (dp is null)
			{
				throw new ArgumentNullException(nameof(DependencyProperty));
			}

			if (metadata is null)
			{
				throw new ArgumentNullException(nameof(PropertyMetadata));
			}

			if (setValueCommon is null)
			{
				BuildSetValueCommon();
			}
			setValueCommon(dObj, dp, value, metadata, coerceWithDeferredReference, coerceWithCurrentValue, operationType, isInternal);
		}

		public static void SetCurrentValue(this DependencyObject dObj, DependencyPropertyKey key, object value)
		{
			if (dObj is null)
			{
				throw new ArgumentNullException(nameof(dObj));
			}

			if (key is null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			dObj.VerifyAccess();
			var dp = key.DependencyProperty;
			var metadata = dp.GetMetadata(dObj.DependencyObjectType);
			if (setValueCommon is null)
			{
				BuildSetValueCommon();
			}
			setValueCommon(dObj, dp, value, metadata, false, true, 0, false);
		}

		public static void SetValueUnsafe(this DependencyObject dObj, DependencyProperty dp, object value)
		{
			if (dObj is null)
			{
				throw new ArgumentNullException(nameof(dObj));
			}

			if (dp is null)
			{
				throw new ArgumentNullException(nameof(dp));
			}

			dObj.VerifyAccess();
			var metadata = dp.GetMetadata(dObj.DependencyObjectType);
			if (setValueCommon is null)
			{
				BuildSetValueCommon();
			}
			setValueCommon(dObj, dp, value, metadata, false, false, 0, false);
		}

		public static void SetCurrentValueUnsafe(this DependencyObject dObj, DependencyProperty dp, object value)
		{
			if (dObj is null)
			{
				throw new ArgumentNullException(nameof(dObj));
			}

			if (dp is null)
			{
				throw new ArgumentNullException(nameof(dp));
			}

			dObj.VerifyAccess();
			PropertyMetadata metadata = dp.GetMetadata(dObj.DependencyObjectType);
			if (setValueCommon is null)
			{
				BuildSetValueCommon();
			}
			setValueCommon(dObj, dp, value, metadata, false, true, 0, false);
		}
	}
}
