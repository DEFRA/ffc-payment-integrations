using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace FFC.Payment.Integrations.Function.Validation
{
    /// <summary>
    /// Methods and schema for validating incoming message
    /// </summary>
    public static class ValidateMessage
    {
        const string schemaJson = @"{
        ""$schema"": ""http://json-schema.org/draft-07/schema#"",
        ""type"": ""object"",
        ""properties"": {
            ""apiLink"": {
                ""type"": ""string""
            },
            ""documentType"": {
                ""type"": ""string""
            },
            ""frn"": {
                ""type"": ""integer""
            },
            ""sbi"": {
                ""type"": ""integer""
            },
            ""scheme"": {
                ""type"": ""string""
            }
        },
        ""required"": [
            ""apiLink"",
            ""documentType"",
            ""frn"",
            ""sbi"",
            ""scheme""
        ]
        }";

        /// <summary>
        /// Determines if message is valid according to the schema
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public static bool IsValid(string notification)
        {
            var schema = JSchema.Parse(schemaJson);
            var parseNotification = JObject.Parse(notification);
            return parseNotification.IsValid(schema);
        }

        /// <summary>
        /// Retrieves the error messages for an invalid message
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public static IList<string> GetValidationErrors(string notification)
        {
            IList<string> errors;
            var schema = JSchema.Parse(schemaJson);
            var parseNotification = JObject.Parse(notification);
            parseNotification.IsValid(schema, out errors);
            return errors;
        }
    }
}
