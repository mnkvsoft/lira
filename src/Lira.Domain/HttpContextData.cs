namespace Lira.Domain;

record HttpContextData(RuleExecutingContext RuleExecutingContext, IResponseWriter ResponseWriter);
