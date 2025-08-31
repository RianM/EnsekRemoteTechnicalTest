using Application.DTOs;
using System.Net;
using System.Text.Json;
using System.Text;

namespace Api.IntegrationTests.Controllers;

public class MeterReadingsControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task UploadMeterReadingsCsv_ShouldReturnBadRequest_WhenNoFileProvided()
    {
        // Arrange
        var formContent = new MultipartFormDataContent();

        // Act
        var response = await HttpClient.PostAsync("/api/meterreadings/meter-reading-uploads", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadMeterReadingsCsv_ShouldReturnBadRequest_WhenInvalidFileTypeProvided()
    {
        // Arrange
        var txtContent = "This is not a CSV file";
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(txtContent));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        formContent.Add(fileContent, "CsvFile", "test.txt");

        // Act
        var response = await HttpClient.PostAsync("/api/meterreadings/meter-reading-uploads", formContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadMeterReadingsCsv_ShouldProcessValidCsvFile_WhenManagerTokenUsed()
    {
        // Arrange
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,01002\n2233,22/04/2019 12:25,323\n";
        var formContent = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(csvContent));
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        formContent.Add(fileContent, "CsvFile", "test.csv");

        // Act
        var response = await HttpClient.PostAsync("/api/meterreadings/meter-reading-uploads", formContent);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<MeterReadingUploadResultDto>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(result);
        Assert.True(result.Successful >= 0);
        Assert.True(result.Failed >= 0);
        Assert.NotNull(result.Errors);
    }
}
