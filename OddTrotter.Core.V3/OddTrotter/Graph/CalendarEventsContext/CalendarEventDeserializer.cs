namespace OddTrotter.Graph.CalendarEventsContext
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using OddTrotter.Odata.v4_01.ProtocolContext;
    using OddTrotter.Odata.v4_01.StrongConventionContext;
    using OddTrotter.Odata.v4_01.WeakConventionContext;

    internal sealed class CalendarEventDeserializer : IDeserializer<CalendarEvent>
    {
        private readonly IEqualityComparer<string> propertyNameComparer;

        public CalendarEventDeserializer(IEqualityComparer<string> propertyNameComparer)
        {
            this.propertyNameComparer = propertyNameComparer;
        }

        public CalendarEvent Deserialize(OdataObject odataObject)
        {
            var missingProperties = new List<string>();

            const string idPropertyName = "id";
            if (!this.TryGetProperty(odataObject, idPropertyName, out var id))
            {
                missingProperties.Add(idPropertyName);
            }

            const string subjectPropertyName = "subject";
            if (!this.TryGetProperty(odataObject, subjectPropertyName, out var subject))
            {
                missingProperties.Add(subjectPropertyName);
            }

            const string bodyPropertyName = "body";
            if (!this.TryGetProperty(odataObject, bodyPropertyName, out var body))
            {
                missingProperties.Add(bodyPropertyName);
            }

            const string startPropertyName = "start";
            if (!this.TryGetProperty(odataObject, startPropertyName, out var start))
            {
                missingProperties.Add(startPropertyName);
            }

            const string isCancelledPropertyName = "isCancelled";
            if (!this.TryGetProperty(odataObject, isCancelledPropertyName, out var isCancelled))
            {
                missingProperties.Add(isCancelledPropertyName);
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
}
