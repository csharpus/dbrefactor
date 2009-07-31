using System;
using System.Linq.Expressions;

namespace DbRefactor.Tools
{
	public static class ExpressionHelper
	{
		public static object ValueFromExpression(Expression expression)
		{
			if (expression is ConstantExpression)
			{
				return (expression as ConstantExpression).Value;
			}
			var lambda = Expression.Lambda<Func<object>>(
				Expression.Convert(expression, typeof(object)),
				new ParameterExpression[0]);
			return lambda.Compile()();
		}
	}
}
