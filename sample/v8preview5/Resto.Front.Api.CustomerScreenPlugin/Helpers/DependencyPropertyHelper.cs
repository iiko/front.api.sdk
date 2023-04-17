using System;
using System.Linq.Expressions;
using System.Windows;
using Resto.Front.Api.Attributes.JetBrains;

namespace Resto.Front.Api.CustomerScreen.Helpers
{
    public static class DependencyPropertyHelper<TDepObj> where TDepObj : DependencyObject
    {
        public static DependencyProperty Register<TProp>([NotNull] Expression<Func<TDepObj, TProp>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var memberExpr = (MemberExpression)(expression.Body);
            return DependencyProperty.Register(memberExpr.Member.Name, typeof(TProp), typeof(TDepObj));
        }

        public static DependencyProperty Register<TProp>([NotNull] Expression<Func<TDepObj, TProp>> expression, TProp defaultValue, [NotNull] ValidateValueCallback validateValueCallback)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (validateValueCallback == null)
                throw new ArgumentNullException(nameof(validateValueCallback));

            var memberExpr = (MemberExpression)(expression.Body);
            return DependencyProperty.Register(memberExpr.Member.Name, typeof(TProp), typeof(TDepObj), new PropertyMetadata(defaultValue), validateValueCallback);
        }

        public static DependencyPropertyKey RegisterReadOnly<TProp>([NotNull] Expression<Func<TDepObj, TProp>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var memberExpr = (MemberExpression)(expression.Body);
            return DependencyProperty.RegisterReadOnly(memberExpr.Member.Name, typeof(TProp), typeof(TDepObj), new PropertyMetadata());
        }
    }
}
