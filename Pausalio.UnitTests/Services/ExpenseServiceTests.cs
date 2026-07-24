using AutoMapper;
using Moq;
using Pausalio.Application.DTOs.Expense;
using Pausalio.Application.Services.Implementations;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;
using System.Linq.Expressions;

namespace Pausalio.UnitTests.Services
{
    public class ExpenseServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILocalizationHelper> _localizationHelperMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly IExpenseService _sut;
        public ExpenseServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _localizationHelperMock = new Mock<ILocalizationHelper>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _sut = new ExpenseService(
                _unitOfWorkMock.Object,
                _mapperMock.Object,
                _localizationHelperMock.Object,
                _currentUserServiceMock.Object
            );
        }
        [Fact]
        public async Task GetAllAsync_WhenCompanyIdIsNull_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns((string?)null);
            _localizationHelperMock
                .Setup(x => x.InvalidCompanyId)
                .Returns("Nevalidan ID kompanije.");

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.GetAllAsync());
        }

        [Fact]
        public async Task GetAllAsync_WhenCompanyIdIsValid_ShouldReturnExpenseList()
        {
            // ARRANGE
            var companyGuid = Guid.NewGuid();
            string companyId = companyGuid.ToString();

            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);

            var expenses = new List<Expense>
            {
                new Expense { Id = Guid.NewGuid(), BusinessProfileId = companyGuid, Name = "Expense 1", IsDeleted = false },
                new Expense { Id = Guid.NewGuid(), BusinessProfileId = companyGuid, Name = "Expense 2", IsDeleted = false }
            };

            var expensesToReturn = new List<ExpenseToReturnDto>
            {
                new ExpenseToReturnDto { Id = Guid.NewGuid(), Name = "Expense 1" },
                new ExpenseToReturnDto { Id = Guid.NewGuid(), Name = "Expense 2" }
            };

            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.FindAllAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync(expenses);
            _mapperMock
                .Setup(x => x.Map<IEnumerable<ExpenseToReturnDto>>(expenses))
                .Returns(expensesToReturn);
            // ACT
            var result = await _sut.GetAllAsync();

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
        [Fact]
        public async Task CreateAsync_WhenAmountIsZeroOrNegative_ShouldThrowInvalidOperationException()
        {
            var companyGuid = Guid.NewGuid();
            var companyId = companyGuid.ToString();
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);
            _localizationHelperMock
                .Setup(x => x.AmountMustBePositive)
                .Returns("Iznos mora biti pozitivan.");
            var addExpenseDto = new AddExpenseDto
            {
                Name = "Test Expense",
                Amount = 0
            };
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(addExpenseDto));
        }

        [Fact]
        public async Task CreateAsync_WhenExpenseWithSameNameExists_ShouldThrowException()
        {
            //Arrange
            var companyGuid = Guid.NewGuid();
            var companyId = companyGuid.ToString();
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync(new Expense());
            _localizationHelperMock
                .Setup(x => x.ExpenseAlreadyExists)
                .Returns("Trošak sa istim imenom već postoji.");
            //Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(new AddExpenseDto { Name = "Existing Expense", Amount = 100 }));
        }

        [Fact]
        public async Task CreateAsync_WhenValidDto_ShouldCreateAndReturnExpense()
        {
            //Arrange
            var companyGuid = Guid.NewGuid();
            var companyId = companyGuid.ToString();
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync((Expense?)null);
            var addExpenseDto = new AddExpenseDto
            {
                Name = "New Expense",
                Amount = 100
            };
            var createdExpense = new Expense
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = companyGuid,
                Name = addExpenseDto.Name,
                Amount = addExpenseDto.Amount,
                IsDeleted = false
            };
            var expenseToReturnDto = new ExpenseToReturnDto
            {
                Id = createdExpense.Id,
                Name = createdExpense.Name,
                Amount = createdExpense.Amount
            };
            _mapperMock
                .Setup(x => x.Map<Expense>(addExpenseDto))
                .Returns(createdExpense);
            _mapperMock
                .Setup(x => x.Map<ExpenseToReturnDto>(createdExpense))
                .Returns(expenseToReturnDto);
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.AddAsync(createdExpense))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);
            //Act
            var result = await _sut.CreateAsync(addExpenseDto);
            //Assert
            Assert.NotNull(result);
            Assert.IsType<ExpenseToReturnDto>(result);
            Assert.Equal(result.Name, createdExpense.Name);
            Assert.Equal(result.Amount, createdExpense.Amount);
            _unitOfWorkMock.Verify(x => x.ExpenseRepository.AddAsync(createdExpense), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task UpdateAsync_WhenExpenseIsPaidAndNewStatusIsNotPaid_ShouldThrowInvalidOperationException()
        {
            //Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateExpenseDto
            {
                Name = "Test 1",
                Amount = 100,
                Status = ExpenseStatus.Pending
            };
            var companyGuid = Guid.NewGuid();
            var companyId = companyGuid.ToString();
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);
            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = companyGuid,
                Status = ExpenseStatus.Paid,
                IsDeleted = false
            };
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync(expense);
            _localizationHelperMock
                .Setup(x => x.CannotModifyPaidExpense)
                .Returns("Ne možete menjati plaćene troškove");
            //Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync(id, dto));
            _localizationHelperMock.Verify(x => x.CannotModifyPaidExpense, Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_WhenDtoIsValid_ShouldUpdateExpense()
        {
            //Arrange
            var id = Guid.NewGuid();
            var dto = new UpdateExpenseDto
            {
                Name = "Test 1",
                Amount = 100,
                Status = ExpenseStatus.Paid
            };
            var companyGuid = Guid.NewGuid();
            var companyId = companyGuid.ToString();
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);
            var expense = new Expense
            {
                Id = id,
                BusinessProfileId = companyGuid,
                Status = ExpenseStatus.Pending,
                IsDeleted = false
            };
            _mapperMock
                .Setup(x => x.Map(dto, expense))
                .Returns(expense);
            _unitOfWorkMock
                .SetupSequence(x => x.ExpenseRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync(expense)
                .ReturnsAsync((Expense?)null);
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.Update(expense));
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);
            //Act 
            await _sut.UpdateAsync(id, dto);
            //Assert
            _unitOfWorkMock.Verify(x => x.ExpenseRepository.Update(expense), Times.Once());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_WhenExpenseExists_ShouldDeleteExpense()
        {
            //Arrange
            var id = Guid.NewGuid();
            var companyGuid = Guid.NewGuid();
            var companyId = companyGuid.ToString();
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);
            var expense = new Expense
            {
                Id = id,
                Status = ExpenseStatus.Pending,
                BusinessProfileId = companyGuid,
                IsDeleted = false
            };
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync(expense);
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.Update(expense));
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);
            //Act
            await _sut.DeleteAsync(id);
            //Assert
            Assert.True(expense.IsDeleted);
            _unitOfWorkMock.Verify(x => x.ExpenseRepository.Update(expense), Times.Once());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once());
        }
        [Fact]
        public async Task DeleteAsync_WhenExpenseDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var id = Guid.NewGuid();
            var companyGuid = Guid.NewGuid();
            var companyId = companyGuid.ToString();
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync((Expense?)null);
            _localizationHelperMock
                .Setup(x => x.ExpenseNotFound)
                .Returns("Trošak nije pronađen.");
            //Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync(id));
        }
        [Fact]
        public async Task DeleteAsync_WhenExpenseIsPaid_ShouldThrowInvalidOperationException()
        {
            //Arrange
            var id = Guid.NewGuid();
            var companyGuid = Guid.NewGuid();
            var companyId = companyGuid.ToString();
            _currentUserServiceMock
                .Setup(x => x.GetCompany())
                .Returns(companyId);
            var expense = new Expense
            {
                Id = id,
                Status = ExpenseStatus.Paid,
                BusinessProfileId = companyGuid,
                IsDeleted = false
            };
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync(expense);
            _localizationHelperMock
                .Setup(x => x.CannotDeletePaidExpense)
                .Returns("Ne možete obrisati plaćene troškove.");
            //Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync(id));
        }
    }
}
