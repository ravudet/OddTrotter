﻿namespace Fx.OdataPocRoot.GraphContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Fx.OdataPocRoot.Graph;
    using Fx.OdataPocRoot.Odata;
    using Fx.OdataPocRoot.Odata.UriExpressionNodes.Common;
    using Fx.OdataPocRoot.Odata.UriExpressionNodes.Select;
    using OddTrotter.GraphClient;

    public sealed class CalendarContext : IInstanceContext<Calendar>
    {
        private readonly IGraphClient graphClient;

        private readonly RelativeUri calendarUri;

        private readonly Select? select;

        public CalendarContext(IGraphClient graphClient, RelativeUri calendarUri)
            : this(graphClient, calendarUri, null)
        {
        }

        private CalendarContext(IGraphClient graphClient, RelativeUri calendarUri, Select? select)
        {
            this.graphClient = graphClient;
            this.calendarUri = calendarUri;
            this.select = select;
        }

        public async Task<Calendar> Evaluate()
        {
            using (var httpResponseMessage = await this.graphClient.GetAsync(this.calendarUri).ConfigureAwait(false))
            {
                httpResponseMessage.EnsureSuccessStatusCode();
                var httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                var calendar = JsonSerializer.Deserialize<Calendar>(httpResponseContent);
                if (calendar ==null)
                {
                    throw new Exception("TODO null calendar");
                }

                return calendar;
            }
        }

        public IInstanceContext<Calendar> Select<TProperty>(Expression<Func<Calendar, TProperty>> selector)
        {
            var select = LinqToOdata.Select(selector);
            if (this.select != null)
            {
                select = new Select(this.select.SelectItems.Concat(select.SelectItems));
            }

            return new CalendarContext(this.graphClient, this.calendarUri, select);
        }
    }

    public static class LinqToOdata
    {
        public static Select Select<TType, TProperty>(Expression<Func<TType, TProperty>> selector)
        {
            if (selector.Body is MemberExpression memberExpression)
            {
                return TraverseSelect<TType>(memberExpression, Enumerable.Empty<MemberExpression>());
            }
            else
            {
                throw new Exception("TODO only member expressions are allowed");
            }
        }

        private static Select TraverseSelect<TType>(MemberExpression expression, IEnumerable<MemberExpression> previousExpressions)
        {
            if (expression.Expression?.NodeType != ExpressionType.Parameter)
            {
                if (expression.Expression is MemberExpression memberExpression)
                {
                    return TraverseSelect<TType>(memberExpression, previousExpressions.Append(expression));
                }
                else
                {
                    throw new Exception("TODO i don't think you can actually get here");
                }
            }
            else
            {
                //// TODO if you can refactor this to be recursive, it'd probably be more readable
                var propertyNames = GetPropertyNames<TType>();
                if (propertyNames.Contains(expression.Member.Name))
                {
                    SelectProperty selectPath =
                        new SelectProperty.PrimitiveProperty(
                            new PrimitiveProperty.PrimitiveNonKeyProperty(
                                new OdataIdentifier(expression.Member.Name)
                            )
                        );
                    if (!previousExpressions.Any())
                    {
                        return new Select(
                            new[] 
                            { 
                                new SelectItem.PropertyPath.Second(selectPath),
                            });
                    }
                    else
                    {
                        //// TODO the expression ends up with the path backwards...
                        foreach (var previousExpression in previousExpressions)
                        {
                            var subPropertyNames = GetPropertyNames(previousExpression.Member.DeclaringType!); //// TODO nullable declaring type?
                            if (subPropertyNames.Contains(previousExpression.Member.Name))
                            {
                                selectPath = new SelectProperty.FullSelectPath.SelectPropertyNode(
                                    new SelectPath.First(
                                        new OdataIdentifier(previousExpression.Member.Name)
                                    ),
                                    selectPath);
                            }
                            else
                            {
                                throw new Exception("TODO property name not found; you could get here if the memberexpression was manually instantiated or if the type has members defined that are not marked as odata properties");
                            }
                        }

                        return new Select(
                            new[]
                            {
                                new SelectItem.PropertyPath.Second(selectPath),
                            });
                    }

                }
                else
                {
                    throw new Exception("TODO property name not found; you could get here if the memberexpression was manually instantiated or if the type has members defined that are not marked as odata properties");
                }
            }
        }

        private static IEnumerable<string> GetPropertyNames<TType>()
        {
            return GetPropertyNames(typeof(TType));
        }

        private static IEnumerable<string> GetPropertyNames(Type type)
        {
            if (type == typeof(Calendar))
            {
                yield return nameof(Calendar.Id);
                yield return nameof(Calendar.Events);
                yield return nameof(Calendar.Foo);
            }
            else if (type == typeof(Event))
            {
                yield return nameof(Event.Id);
                yield return nameof(Event.Body);
                yield return nameof(Event.End);
                yield return nameof(Event.IsCancelled);
                yield return nameof(Event.ResponseStatus);
                yield return nameof(Event.Start);
                yield return nameof(Event.Subject);
                yield return nameof(Event.Type);
                yield return nameof(Event.WebLink);
            }
            else if (type == typeof(ItemBody))
            {
                yield return nameof(ItemBody.Content);
            }
            else if (type == typeof(DateTimeTimeZone))
            {
                yield return nameof(DateTimeTimeZone.DateTime);
                yield return nameof(DateTimeTimeZone.TimeZone);
            }
            else if (type == typeof(ResponseStatus))
            {
                yield return nameof(ResponseStatus.Response);
                yield return nameof(ResponseStatus.Time);
            }
            else
            {
                throw new Exception("TODO actually implement this in a general way");
            }
        }
    }
}
