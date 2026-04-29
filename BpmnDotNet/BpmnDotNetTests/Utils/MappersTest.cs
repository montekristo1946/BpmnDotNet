using BpmnDotNet.Utils;

namespace BpmnDotNetTests.Utils;

public class MappersTest
{
     [Fact]
    public void Map_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string x = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Mappers.Map(x));
    }

    [Fact]
    public void Map_WithValidIntegerString_ReturnsInteger()
    {
        // Arrange
        string x = "123";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void Map_WithValidDoubleString_ReturnsTruncatedInteger()
    {
        // Arrange
        string x = "123.78";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(123, result); // (int)123.78 = 123
    }

    [Fact]
    public void Map_WithValidFloatString_CultureInvariant()
    {
        // Arrange
        string x = "456.89";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(456, result);
    }

    [Fact]
    public void Map_WithNegativeNumber_ReturnsNegativeInteger()
    {
        // Arrange
        string x = "-42";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(-42, result);
    }

    [Fact]
    public void Map_WithNegativeFloat_ReturnsTruncatedNegativeInteger()
    {
        // Arrange
        string x = "-42.99";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(-42, result); // (int)-42.99 = -42 (truncates toward zero)
    }

    [Fact]
    public void Map_WithScientificNotation_ReturnsInteger()
    {
        // Arrange
        string x = "1.23e2";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(123, result); // 1.23e2 = 123
    }

    [Fact]
    public void Map_WithScientificNotationNegative_ReturnsInteger()
    {
        // Arrange
        string x = "1.23e-2";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(0, result); // 0.0123 -> 0
    }

    [Fact]
    public void Map_WithZero_ReturnsZero()
    {
        // Arrange
        string x = "0";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Map_WithDecimalPointFormat_HandlesInvariantCulture()
    {
        // Arrange
        string x = "789.12";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(789, result);
    }

    [Fact]
    public void Map_WithLeadingAndTrailingSpaces_ReturnsInteger()
    {
        // Arrange
        string x = "   123   ";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(123, result);
    }

    [Fact]
    public void Map_WithPlusSign_ReturnsInteger()
    {
        // Arrange
        string x = "+123";

        // Act
        var result = Mappers.Map(x);

        // Assert
        Assert.Equal(123, result);
    }
    

    [Theory]
    [InlineData("abc")]
    [InlineData("12.34.56")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("INF")]
    [InlineData("123abc")]
    [InlineData("abc123")]
    public void Map_WithInvalidFormat_ThrowsArgumentException(string invalidInput)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Mappers.Map(invalidInput));
        Assert.Contains(invalidInput, exception.Message);
        Assert.Contains("[Mappers] Could not convert", exception.Message);
    }

    [Theory]
    [InlineData("2147483647", 2147483647)] // Max int
    [InlineData("-2147483648", -2147483648)] // Min int
    [InlineData("  0  ", 0)]
    [InlineData("  123.45  ", 123)]
    [InlineData("-123.99", -123)]
    public void Map_WithVariousValidInputs_ReturnsExpectedResult(string input, int expected)
    {
        // Act
        var result = Mappers.Map(input);

        // Assert
        Assert.Equal(expected, result);
    }
}