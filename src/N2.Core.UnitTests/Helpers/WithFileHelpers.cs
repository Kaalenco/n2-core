using System.IO.Abstractions;
using System.Text;

using Moq;

using N2.Core.Helpers;

namespace N2.Core.UnitTests.Helpers;

[TestClass]
public class WithFileHelpers
{
    private readonly Mock<IFileInfo> fileInfoMock = new();
    private readonly Mock<IFileInfoFactory> fileInfoFactoryMock = new();

    private const string expectedHash = "BFD76C0EBBD006FEE583410547C1887B0292BE76D582D96C242D2A792723E3FD6FD061F9D5CFD13B8F961358E6ADBA4A";

    [TestInitialize]
    public void Setup()
    {
        // No setup needed for the mocks
    }

    private sealed class TestStream : FileSystemStream
    {
        public TestStream() : base(new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")), "c:\\temp\\testfile.txt", false)
        {
        }
    }

    [TestMethod]
    public void ComputeSha384HashWhenFileInfoExistsReturnsHash()
    {
        // Arrange

        using TestStream testStream = new();
        fileInfoMock.Setup(f => f.Exists).Returns(true);
        fileInfoMock.Setup(f => f.OpenRead()).Returns(testStream);

        //var sha384Mock = new Mock<SHA384>();
        //sha384Mock.Setup(s => s.ComputeHash(streamMock.Object)).Returns(Convert.FromHexString(expectedHash));

        // Act
        string actualHash = fileInfoMock.Object.ComputeSha384Hash();

        // Assert
        Assert.AreEqual(expectedHash, actualHash);
    }

    [TestMethod]
    public void ComputeSha384HashWhenFileInfoDoesNotExistThrowsException()
    {
        // Arrange
        fileInfoMock.Setup(f => f.Exists).Returns(false);
        fileInfoMock.Setup(f => f.FullName).Returns("nonexistentfile.txt");

        // Act
        Assert.Throws<NotSupportedException>(() => fileInfoMock.Object.ComputeSha384Hash());
    }

    [TestMethod]
    public void ComputeSha384HashWhenUsingFileInfoFactoryReturnsHash()
    {
        // Arrange
        string filePath = "c:\\temp\\testfile.txt";
        using TestStream testStream = new();

        fileInfoMock.Setup(f => f.Exists).Returns(true);
        fileInfoMock.Setup(f => f.OpenRead()).Returns(testStream);
        fileInfoFactoryMock.Setup(f => f.New(filePath)).Returns(fileInfoMock.Object);

        //var sha384Mock = new Mock<SHA384>();
        //sha384Mock.Setup(s => s.ComputeHash(streamMock.Object)).Returns(Convert.FromHexString(expectedHash));

        // Act
        string actualHash = FileHelpers.ComputeSha384Hash(fileInfoFactoryMock.Object, filePath);

        // Assert
        Assert.AreEqual(expectedHash, actualHash);
    }

    [TestMethod]
    public void ComputeSha384HashWhenUsingStreamReturnsHash()
    {
        // Arrange
        using FileSystemStream streamMock = new TestStream();

        //var sha384Mock = new Mock<SHA384>();
        //sha384Mock.Setup(s => s.ComputeHash(streamMock.Object)).Returns(Convert.FromHexString(expectedHash));

        // Act
        string actualHash = FileHelpers.ComputeSha384Hash(streamMock);

        // Assert
        Assert.AreEqual(expectedHash, actualHash);
    }
}