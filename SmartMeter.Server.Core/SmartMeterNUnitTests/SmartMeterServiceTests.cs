using NUnit.Framework;
using Moq;
using SmartMeter.Server.Core.Services;
using SmartMeter.Server.Core.Data;
using SmartMeter.Server.Core.Models;
using SmartMeter.Server.Core.DTOs;
using SmartMeter.Server.Core.Authentication;
using SmartMeter.Server.Core.Logging;
using CoreLogger = SmartMeter.Server.Core.Logging.Logger;

namespace SmartMeterTests
{
    [TestFixture]
    public class SmartMeterServiceTests
    {
        private Mock<IReadingRepository> _mockReadingRepo;
        private Mock<ITokenRepository> _mockTokenRepo;
        private Mock<ITariffRepository> _mockTariffRepo;
        private Mock<IJWTHelper> _mockJwtHelper;
        private CoreLogger _logger;
        private SmartMeterService _service;

        [SetUp]
        public void Setup()
        {
            // Initialising the mocks
            _mockReadingRepo = new Mock<IReadingRepository>();
            _mockTokenRepo = new Mock<ITokenRepository>();
            _mockTariffRepo = new Mock<ITariffRepository>();
            _mockJwtHelper = new Mock<IJWTHelper>();
            _logger = new CoreLogger();

            _service = new SmartMeterService(
                _mockReadingRepo.Object,
                _mockTokenRepo.Object,
                _mockTariffRepo.Object,
                _mockJwtHelper.Object,
                _logger
            );
        }

        [Test] // Tests that a valid meter reading is processed successfully
        public void ProcessReading_ValidMessage_ReturnsTrue()
        {
            // Arrange
            var testData = new MeterData
            {
                MeterId = "METER-1234",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = new EnergyConsumption
                {
                    TotalConsumption = 100,
                    PeakConsumption = 40,
                    OffPeakConsumption = 60
                },
                Tariff = new TariffJson
                {
                    StandardRate = 0.15m,
                    PeakRate = 0.20m,
                    OffPeakRate = 0.10m,
                    StandingCharge = 0.30m,
                    Timestamp = DateTime.UtcNow
                },
                Jwt = "valid-token"
            };

            string validJson = System.Text.Json.JsonSerializer.Serialize(testData);

            _mockJwtHelper.Setup(x => x.ValidateToken(It.IsAny<string>()))
                .Returns(true);

            // Act
            bool result = _service.ProcessReading(validJson, out MeterData? meterData);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(meterData.MeterId, Is.EqualTo("METER-1234"));
        }

        [Test] // Tests that an invalid JWT token causes processing to fail
        public void ProcessReading_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var testData = new MeterData
            {
                MeterId = "METER-1234",
                Jwt = "invalid-token"
            };
            string json = System.Text.Json.JsonSerializer.Serialize(testData);

            _mockJwtHelper.Setup(x => x.ValidateToken(It.IsAny<string>()))
                .Returns(false);

            // Act
            bool result = _service.ProcessReading(json, out MeterData? meterData);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test] // Tests that bill calculation works correctly with valid meter data
        public void CalculateBill_ValidData_ReturnsCorrectTotal()
        {
            // Arrange
            var meterData = new MeterData
            {
                MeterId = "METER-1234",
                EnergyConsumption = new EnergyConsumption
                {
                    TotalConsumption = 100,
                    PeakConsumption = 40,
                    OffPeakConsumption = 30
                },
                Tariff = new TariffJson
                {
                    StandardRate = 0.15m,
                    PeakRate = 0.20m,
                    OffPeakRate = 0.10m
                }
            };

            // Act
            string billJson = _service.CalculateBill(meterData);

            // Assert
            Assert.That(billJson, Does.Contain("METER-1234"));
            Assert.That(billJson, Does.Contain("TotalBill"));
        }

        [Test] // Tests that invalid JSON format is handled correctly
        public void ProcessReading_InvalidFormat_ReturnsFalse()
        {
            // Arrange
            string invalidJson = "{ invalid json }";

            // Act
            bool result = _service.ProcessReading(invalidJson, out MeterData? meterData);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(meterData, Is.Null);
        }

        [Test] // Tests that reading data is properly saved to the repository
        public void AddReading_ValidData_VerifyRepositoryCalls()
        {
            // Arrange
            var testData = new MeterData
            {
                MeterId = "METER-1234",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = new EnergyConsumption
                {
                    TotalConsumption = 100,
                    PeakConsumption = 40,
                    OffPeakConsumption = 60
                },
                Tariff = new TariffJson
                {
                    StandardRate = 0.15m,
                    PeakRate = 0.20m,
                    OffPeakRate = 0.10m,
                    StandingCharge = 0.30m,
                    Timestamp = DateTime.UtcNow
                },
                Jwt = "valid-token"
            };
            string json = System.Text.Json.JsonSerializer.Serialize(testData);

            _mockJwtHelper.Setup(x => x.ValidateToken(It.IsAny<string>()))
                .Returns(true);

            _mockReadingRepo.Setup(x => x.AddReading(It.IsAny<Reading>()));
            _mockTokenRepo.Setup(x => x.AddToken(It.IsAny<JWToken>()));
            _mockTariffRepo.Setup(x => x.AddTariff(It.IsAny<Tariff>()));

            // Act
            bool result = _service.ProcessReading(json, out MeterData? meterData);

            // Assert
            Assert.That(result, Is.True);
            _mockReadingRepo.Verify(x => x.AddReading(It.IsAny<Reading>()), Times.Once);
        }

        [Test] // Tests that tariff data is properly saved to the repository
        public void ProcessReading_ValidTariffData_SavesTariff()
        {
            // Arrange
            var testData = new MeterData
            {
                MeterId = "METER-1234",
                Timestamp = DateTime.UtcNow,
                EnergyConsumption = new EnergyConsumption
                {
                    TotalConsumption = 100,
                    PeakConsumption = 40,
                    OffPeakConsumption = 60
                },
                Tariff = new TariffJson
                {
                    StandardRate = 0.15m,
                    PeakRate = 0.20m,
                    OffPeakRate = 0.10m,
                    StandingCharge = 0.30m,
                    Timestamp = DateTime.UtcNow
                },
                Jwt = "valid-token"
            };
            string json = System.Text.Json.JsonSerializer.Serialize(testData);

            _mockJwtHelper.Setup(x => x.ValidateToken(It.IsAny<string>()))
                .Returns(true);

            _mockReadingRepo.Setup(x => x.AddReading(It.IsAny<Reading>()));
            _mockTokenRepo.Setup(x => x.AddToken(It.IsAny<JWToken>()));
            _mockTariffRepo.Setup(x => x.AddTariff(It.IsAny<Tariff>()));

            // Act
            bool result = _service.ProcessReading(json, out MeterData? meterData);

            // Assert
            Assert.That(result, Is.True);
            _mockTariffRepo.Verify(x => x.AddTariff(It.IsAny<Tariff>()), Times.Once);
        }
    }
}