using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Sample03.E3SClient;

namespace Sample03
{
	public class ExpressionToFTSRequestTranslator : ExpressionVisitor
	{
		StringBuilder resultString;
        List<Statement> queries;

        public List<Statement> Translate(Expression exp)
		{
			Visit(exp);

			return queries;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.DeclaringType == typeof(Queryable)
				&& node.Method.Name == "Where")
			{
                for (int i = 1; i < node.Arguments.Count; i++)
                {
                    Visit(node.Arguments[i]);
                    if (resultString.Length > 0)
                    {
                        queries.Add(new Statement { Query = resultString.ToString() });
                        resultString.Clear();
                    }
                }				
				return node;
			}
            if (node.Method.DeclaringType == typeof(String)
                && node.Method.Name == "StartsWith")
            {
                base.VisitMethodCall(node);
                resultString.Replace(")", "*)");
                return node;
            }
            if (node.Method.DeclaringType == typeof(String)
                && node.Method.Name == "Contains")
            {
                base.VisitMethodCall(node);
                resultString.Replace(")", "*)");
                resultString.Replace("(", "(*");
                return node;
            }
            if (node.Method.DeclaringType == typeof(String)
                && node.Method.Name == "EndsWith")
            {
                base.VisitMethodCall(node);
                resultString.Replace("(", "(*");
                return node;
            }
            return base.VisitMethodCall(node);
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			switch (node.NodeType)
			{
				case ExpressionType.Equal:
                    if (!(node.Left.NodeType == ExpressionType.MemberAccess
                        || node.Left.NodeType == ExpressionType.Constant))
                        throw new NotSupportedException(string.Format("Left operand should be constant, property or field", node.NodeType));

                    if (!(node.Right.NodeType == ExpressionType.MemberAccess
                        || node.Right.NodeType == ExpressionType.Constant))
                        throw new NotSupportedException(string.Format("Right operand should be constant, property or field", node.NodeType));

                    if (node.Right.NodeType == ExpressionType.MemberAccess && node.Left.NodeType == ExpressionType.Constant)
                    {
                        Visit(node.Right);
                        Visit(node.Left);                       
                        break;
                    }
                    if (node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)
                    {
                        Visit(node.Left);
                        Visit(node.Right);
                        break;
                    }
                    throw new NotSupportedException(string.Format("Where the constant buddy?", node.NodeType));

                case ExpressionType.AndAlso:
                    base.Visit(node.Left);
                    if (resultString.Length > 0)
                    {
                        queries.Add(new Statement { Query = resultString.ToString() });
                        resultString.Clear();
                    }
                    base.Visit(node.Right);
                    if (resultString.Length > 0)
                    {
                        queries.Add(new Statement { Query = resultString.ToString() });
                        resultString.Clear();
                    }
                    break;

                default:
					throw new NotSupportedException(string.Format("Operation {0} is not supported", node.NodeType));
			};

			return node;
		}
      
        protected override Expression VisitMember(MemberExpression node)
		{
			resultString.Append(node.Member.Name).Append(":");

			return base.VisitMember(node);
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
            resultString.Append("(").Append(node.Value).Append(")");

            return node;
		}
       
        public ExpressionToFTSRequestTranslator()
        {
            resultString = new StringBuilder();
            queries = new List<Statement>();
        }
	}
}
