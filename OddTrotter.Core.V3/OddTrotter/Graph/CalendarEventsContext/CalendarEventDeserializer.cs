namespace OddTrotter.Graph.CalendarEventsContext
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Fx.Either;
    
    using OddTrotter.Odata.v4_01.ProtocolContext;
    using OddTrotter.Odata.v4_01.StrongConventionContext;
    using OddTrotter.Odata.v4_01.WeakConventionContext;

    internal sealed class BodyStructureDeserializer : IDeserializer<BodyStructure>
    {
        private readonly IEqualityComparer<string> propertyNameComparer;

        public BodyStructureDeserializer(IEqualityComparer<string> propertyNameComparer)
        {
            this.propertyNameComparer = propertyNameComparer;
        }

        public BodyStructure Deserialize(OdataObject odataObject)
        {
            var missingProperties = new List<string>();

            const string contentPropertyName = "content";
            if (this.TryGetProperty(odataObject, contentPropertyName, out var contentProperty))
            {
                var content = contentProperty.Apply(
                    @string => Either.Right<DeserializationException>().Left(@string.Value),
                    @object => Either.Left<string>().Right(new DeserializationException("TODO")),
                    collection => Either.Left<string>().Right(new DeserializationException("TODO")));

                var contentValue = content.throwright //// TODO implement this
            }
            else
            {
                missingProperties.Add(contentPropertyName);
            }

            if (missingProperties.Any())
            {
                throw new DeserializationException("TODO");
            }
        }

        private bool TryGetProperty(OdataObject odataObject, string propertyName, [MaybeNullWhen(false)] out OdataPropertyValue odataPropertyValue)
        {
            var properties = odataObject.Properties.Where(property => this.propertyNameComparer.Equals(property.Name, propertyName));
            if (properties.TrySingle(out var property))
            {
                odataPropertyValue = property.Value;
                return true;
            }
            else
            {
                odataPropertyValue = default;
                return false;
            }
        }
    }

    internal sealed class CalendarEventDeserializer : IDeserializer<CalendarEvent>
    {
        private readonly IDeserializer<BodyStructure> bodyStructureDeserializer;
        private readonly IEqualityComparer<string> propertyNameComparer;

        public CalendarEventDeserializer(
            IDeserializer<BodyStructure> bodyStructureDeserializer, 
            IEqualityComparer<string> propertyNameComparer)
        {
            this.bodyStructureDeserializer = bodyStructureDeserializer;
            this.propertyNameComparer = propertyNameComparer;
        }

        public CalendarEvent Deserialize(OdataObject odataObject)
        {
            var missingProperties = new List<string>();

            const string idPropertyName = "id";
            if (!this.TryGetProperty(odataObject, idPropertyName, out var idProperty))
            {
                missingProperties.Add(idPropertyName);
            }

            const string subjectPropertyName = "subject";
            if (!this.TryGetProperty(odataObject, subjectPropertyName, out var subjectProperty))
            {
                missingProperties.Add(subjectPropertyName);
            }

            const string bodyPropertyName = "body";
            if (!this.TryGetProperty(odataObject, bodyPropertyName, out var bodyProperty))
            {
                missingProperties.Add(bodyPropertyName);
            }

            const string startPropertyName = "start";
            if (!this.TryGetProperty(odataObject, startPropertyName, out var startProperty))
            {
                missingProperties.Add(startPropertyName);
            }

            const string isCancelledPropertyName = "isCancelled";
            if (!this.TryGetProperty(odataObject, isCancelledPropertyName, out var isCancelledProperty))
            {
                missingProperties.Add(isCancelledPropertyName);
            }

            if (missingProperties.Any())
            {
                throw new DeserializationException("TODO");
            }

            var id = idProperty!.Apply(
                @string => Either.Right<DeserializationException>().Left(@string.Value), //// TODO can you use `refeither` for this?
                @object => Either.Left<string>().Right(new DeserializationException("TODO")),
                colleciton => Either.Left<string>().Right(new DeserializationException("TODO")));
            var subject = subjectProperty!.Apply(
                @string => Either.Right<DeserializationException>().Left(@string.Value),
                @object => Either.Left<string>().Right(new DeserializationException("TODO")),
                colleciton => Either.Left<string>().Right(new DeserializationException("TODO")));
            var body = bodyProperty!.Apply(
                @string => Either.Left<BodyStructure>().Right(new DeserializationException("TODO")),
                @object => @object.Try(_ => this.bodyStructureDeserializer.Deserialize(_.Value)),
                colleciton => Either.Left<BodyStructure>().Right(new DeserializationException("TODO")));
            var start = startProperty!
                .Apply(
                    @string => Either.Right<DeserializationException>().Left(@string.Value),
                    @object => Either.Left<string>().Right(new DeserializationException("TODO")),
                    colleciton => Either.Left<string>().Right(new DeserializationException("TODO")))
                .SelectLeft(@string => @string
                    .Try(DateTimeOffset.Parse)
                    .SelectRight(exception => new DeserializationException("TODO", exception)))
                .SelectManyLeft();
            var isCancelled = isCancelledProperty!
                .Apply(
                    @string => Either.Right<DeserializationException>().Left(@string.Value),
                    @object => Either.Left<string>().Right(new DeserializationException("TODO")),
                    colleciton => Either.Left<string>().Right(new DeserializationException("TODO")))
                .SelectLeft(@string => @string
                    .Try(bool.Parse)
                    .SelectRight(exception => new DeserializationException("TODO", exception)))
                .SelectManyLeft();

            //// TODO do you want to do *all* validations before returning? for example, should you validate the *existing* properties even if some of them are missing, that way you give the most comprehensive error result?
            
            var state = (new CalendarEventBuilder(), new List<DeserializationException>());
            id.Apply(
                (value, ref state) =>
                {
                    state.Item1.Id = value;
                },
                (exception, ref state) =>
                {
                    state.Item2.Add(exception);
                },
                ref state);
            subject.Apply(
                (value, ref state) =>
                {
                    state.Item1.Subject = value;
                },
                (exception, ref state) =>
                {
                    state.Item2.Add(exception);
                },
                ref state);
            body.Apply(
                (value, ref state) =>
                {
                    state.Item1.Body = value;
                },
                (exception, ref state) =>
                {
                    state.Item2.Add(exception);
                },
                ref state);
            start.Apply(
                (value, ref state) =>
                {
                    state.Item1.Start = value;
                },
                (exception, ref state) =>
                {
                    state.Item2.Add(exception);
                },
                ref state);
            isCancelled.Apply(
                (value, ref state) =>
                {
                    state.Item1.IsCancelled = value;
                },
                (exception, ref state) =>
                {
                    state.Item2.Add(exception);
                },
                ref state);

            if (state.Item2.Any())
            {
                throw new DeserializationException("TODO", new AggregateException(state.Item2));
            }

            return state.Item1.Build();
        }

        private sealed class CalendarEventBuilder
        {
            public string? Id { get; set; }

            public string? Subject { get; set; }

            public BodyStructure? Body { get; set; }

            public TimeStructure? Start { get; set; }

            public bool? IsCancelled { get; set; }

            public CalendarEvent Build()
            {
                ArgumentNullException.ThrowIfNull(this.Id);
                ArgumentNullException.ThrowIfNull(this.Subject);
                ArgumentNullException.ThrowIfNull(this.Body);
                ArgumentNullException.ThrowIfNull(this.Start);
                ArgumentNullException.ThrowIfNull(this.IsCancelled);
                
                return new CalendarEvent(
                    this.Id,
                    this.Subject,
                    this.Body,
                    this.Start,
                    this.IsCancelled.Value);
            }
        }
        
        private bool TryGetProperty(OdataObject odataObject, string propertyName, [MaybeNullWhen(false)] out OdataPropertyValue odataPropertyValue)
        {
            var properties = odataObject.Properties.Where(property => this.propertyNameComparer.Equals(property.Name, propertyName));
            if (properties.TrySingle(out var property))
            {
                odataPropertyValue = property.Value;
                return true;
            }
            else
            {
                odataPropertyValue = default;
                return false;
            }
        }
    }

    internal static class Extensions
    {
        public static IEither<TResult, Exception> Try<TValue, TResult>(this TValue value, Func<TValue, TResult> toTry)
        {
            TResult result;
            try
            {
                result = toTry(value);
            }
            catch (Exception exception)
            {
                return Either.Left<TResult>().Right(exception);
            }

            return Either.Right<Exception>().Left(result);
        }
    }
}
