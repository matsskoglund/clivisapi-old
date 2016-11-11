using Xunit;
using Clivis.Controllers;
using System.Collections.Generic;
using Clivis.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Moq;
using System;

namespace ClivisTests
{
    

    // see example explanation on xUnit.net website:
    // https://xunit.github.io/docs/getting-started-dotnet-core.html
    public class ClimateControllerTests
    {
        Mock<IClimateSource> nibeMock = new Mock<IClimateSource>();
        Mock<IClimateSource> netatmoMock = new Mock<IClimateSource>();

        public IConfigurationRoot Configuration { get; }
        private readonly ClimateController _climateController;
        public ClimateControllerTests()
        {           
             ConfigurationBuilder builder = new ConfigurationBuilder();
             builder.AddUserSecrets();            
           
            Configuration = builder.Build();
            IOptions<AppKeyConfig> options = Options.Create(new AppKeyConfig()
            {    
                UserName = Configuration["NetatmoUserName"],
                Password = Configuration["NetatmoPassword"],
                ClientId = Configuration["NetatmoClientId"],
                ClientSecret = Configuration["NetatmoClientSecret"]
             });                                

            _climateController = new ClimateController(options, nibeMock.Object, netatmoMock.Object);
        }

         [Fact]
        public void ClimateController_NotNull()
        {
            Assert.NotNull(_climateController);
        }

        [Fact]
        public void ClimateController_GetClimateSetsCodeAndReturnNotNull()
        {
            IActionResult res = _climateController.GetClimate("code","state");
            nibeMock.VerifySet(foo => foo.code = "code");

            Assert.NotNull(res);
            Assert.IsType<EmptyResult>(res);
        }

        [Fact]
        public void ClimateController_GetById_WithNibe_As_Source_Calls_CurrentReading_WithConfigs()
        {
            ClimateItem item = new ClimateItem();
            nibeMock.Setup<ClimateItem>(x => x.CurrentReading(It.IsAny<AppKeyConfig>())).Returns(item);
            

            ClimateItem res = _climateController.GetById("Nibe", "clientid", "clientSecret", "redirect_uri", "username", "password");
            nibeMock.Verify(x => x.CurrentReading(It.IsAny<AppKeyConfig>()), Times.AtLeastOnce());
            Assert.Equal(item, res);
        }

        [Fact]
        public void ClimateController_GetById_WithNibeLogin_As_Source_Calls_init_WithConfigs()
        {
            ClimateItem item = new ClimateItem();
            nibeMock.Setup(x => x.init(It.IsAny<AppKeyConfig>()));


            ClimateItem res = _climateController.GetById("NibeLogin", "clientid", "clientSecret", "redirect_uri", "username", "password");
            nibeMock.Verify(x => x.init(It.IsAny<AppKeyConfig>()), Times.AtLeastOnce());            
        }

        [Fact]
        public void ClimateController_GetById_With_Netatmo_As_Source_Calls_CurrentReading_WithConfigs()
        {
            ClimateItem item = new ClimateItem();
            netatmoMock.Setup<ClimateItem>(x => x.CurrentReading(It.IsAny<AppKeyConfig>())).Returns(item);


            ClimateItem res = _climateController.GetById("Netatmo", "clientid", "clientSecret", "redirect_uri", "username", "password");
            netatmoMock.Verify(x => x.CurrentReading(It.IsAny<AppKeyConfig>()), Times.AtLeastOnce());
            Assert.Equal(item, res);
        }

        [Fact]
        public void ClimateController_GetById_With_Wrong_SourceName_Returns_null()
        {           
            ClimateItem res = _climateController.GetById("Nonexisting", "clientid", "clientSecret", "redirect_uri", "username", "password");            
            Assert.Null(res);
        }
    }
}
