using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Moq;

namespace FFC.Payment.Integrations.Function.Tests.Helpers
{
	public class ConfigHelper
	{
		public static Mock<IConfiguration> SetupConfigForSectionAccess(List<KeyValuePair<string, string>> configItems)
		{
            var mockConfiguration = new Mock<IConfiguration>();

            if (configItems != null && configItems.Count > 0)
            {
                foreach (var configItem in configItems)
                {
                    var sectionMock = new Mock<IConfigurationSection>();
                    sectionMock.Setup(s => s.Value).Returns(configItem.Value);
                    mockConfiguration.Setup(x => x.GetSection(configItem.Key)).Returns(sectionMock.Object);
                }
            }

            return mockConfiguration;
        }
    }
}

