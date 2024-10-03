using Xunit;
using agroApp.Domain.Entities;

namespace Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void Connection_ShouldHaveValidUserId_WhenSet()
        {
            // Arrange
            var userId = 1;
            var connection = new Connection { UserId = userId };

            // Act
            var actualUserId = connection.UserId;

            // Assert
            Assert.Equal(userId, actualUserId);
        }

        [Fact]
        public void Connection_ShouldThrowArgumentException_WhenUserIdIsZero()
        {
            // Arrange
            var connection = new Connection();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => connection.UserId = 0);
        }

        [Fact]
        public void Connection_ShouldHaveValidConnectedUserId_WhenSet()
        {
            // Arrange
            var connectedUserId = 2;
            var connection = new Connection { ConnectedUserId = connectedUserId };

            // Act
            var actualConnectedUserId = connection.ConnectedUserId;

            // Assert
            Assert.Equal(connectedUserId, actualConnectedUserId);
        }

        [Fact]
        public void Connection_ShouldThrowArgumentException_WhenConnectedUserIdIsZero()
        {
            // Arrange
            var connection = new Connection();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => connection.ConnectedUserId = 0);
        }

        [Fact]
        public void Connection_ShouldHaveValidUser()
        {
            // Arrange
            var user = new User();
            var connection = new Connection { User = user };

            // Act
            var actualUser = connection.User;

            // Assert
            Assert.Equal(user, actualUser);
        }

        [Fact]
        public void Connection_ShouldHaveValidConnectedUser()
        {
            // Arrange
            var connectedUser = new User();
            var connection = new Connection { ConnectedUser = connectedUser };

            // Act
            var actualConnectedUser = connection.ConnectedUser;

            // Assert
            Assert.Equal(connectedUser, actualConnectedUser);
        }
    }
}