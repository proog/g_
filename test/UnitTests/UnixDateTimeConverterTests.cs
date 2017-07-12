using System;
using System.Collections.Generic;
using Games.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Games.UnitTests
{
    public class UnixDateTimeConverterTests
    {
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new UnixDateTimeConverter() },
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        [Fact]
        public void ReadTest()
        {
            // Arrange
            var json = "{\"date\":1499881750}";

            // Act
            var date = JsonConvert.DeserializeObject<DateContainer>(json, jsonSettings).Date;

            // Assert
            Assert.Equal(2017, date.Year);
            Assert.Equal(7, date.Month);
            Assert.Equal(12, date.Day);
            Assert.Equal(17, date.Hour);
            Assert.Equal(49, date.Minute);
            Assert.Equal(10, date.Second);
            Assert.Equal(DateTimeKind.Utc, date.Kind);
        }

        [Fact]
        public void WriteTest()
        {
            // Arrange
            var obj = new DateContainer
            {
                Date = new DateTime(2017, 7, 12, 17, 49, 10, DateTimeKind.Utc)
            };

            // Act
            var json = JsonConvert.SerializeObject(obj, jsonSettings);

            // Assert
            Assert.Equal("{\"date\":1499881750}", json);
        }

        private class DateContainer
        {
            public DateTime Date { get; set; }
        }
    }
}
