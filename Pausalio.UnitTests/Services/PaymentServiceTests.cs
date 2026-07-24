using AutoMapper;
using Moq;
using Pausalio.Application.DTOs.Payment;
using Pausalio.Application.Services.Implementations;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Domain.Entities;
using Pausalio.Infrastructure.Repositories.Implementations;
using Pausalio.Infrastructure.Repositories.Interfaces;
using Pausalio.Shared.Enums;
using Pausalio.Shared.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using System.Text;

namespace Pausalio.UnitTests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILocalizationHelper> _localizationHelperMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IExchangeRateService> _exchangeRateServiceMock;
        private readonly PaymentService _paymentService;
        public PaymentServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();
            _localizationHelperMock = new Mock<ILocalizationHelper>();
            _currentUserServiceMock= new Mock<ICurrentUserService>();
            _exchangeRateServiceMock = new Mock<IExchangeRateService>();
            _currentUserServiceMock
        .Setup(x => x.GetCompany())
        .Returns(Guid.NewGuid().ToString());
            _paymentService = new PaymentService(
                _unitOfWorkMock.Object, _mapperMock.Object, _localizationHelperMock.Object, _currentUserServiceMock.Object, _exchangeRateServiceMock.Object);
        }
        [Fact]
        public async Task CreateAsync_WhenAmountIsZeroOrNegative_ShouldThrowInvalidOperationException()
        {
            //Arrange
            var dto = new AddPaymentDto
            {
                Amount=0,
                BankAccountId=Guid.NewGuid(),
                Currency=Shared.Enums.Currency.USD
            };
            _localizationHelperMock
                .Setup(x => x.AmountMustBePositive)
                .Returns("Iznos mora biti pozitivan.");
            //Act
            //Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>_paymentService.CreateAsync(dto));
            _localizationHelperMock.Verify(x => x.AmountMustBePositive, Times.Once());
        }
        [Fact]
        public async Task CreateAsync_WhenEntityIdIsEmpty_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var dto = new AddPaymentDto
            {
                Amount = 10,
                EntityId = Guid.Empty, 
                Currency = Shared.Enums.Currency.RSD
            };

            _localizationHelperMock
                .Setup(x => x.EntityIdRequired)
                .Returns("EntityId je obavezan.");

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentService.CreateAsync(dto));

            // Assert
            Assert.Equal("EntityId je obavezan.", exception.Message);
        }
        [Fact]
        public async Task CreateAsync_WhenCurrencyIsNotRSD_ShouldFetchExchangeRateAndCalculateAmountRSD()
        {
            //Arrange
            var dto = new AddPaymentDto 
            {
                Currency=Shared.Enums.Currency.USD,
                Amount=100,
                EntityId=Guid.NewGuid(),
                PaymentType= Shared.Enums.PaymentType.ExpensePayment
            };
            _exchangeRateServiceMock
                .Setup(x => x.GetExchangeRateAsync(dto.Currency))
                .Returns(It.IsAny<Task<decimal?>>);
            //Act
            var result = _paymentService.CreateAsync(dto);
            //Assert
            _exchangeRateServiceMock.Verify(x => x.GetExchangeRateAsync(dto.Currency), Times.Once());

        }
        [Fact]
        public async Task CreateAsync_InvoicePayment_WhenInvoiceNotFound_ShouldThrowKeyNotFoundException()
        {
            //Arrange
            var addPaymentDto = new AddPaymentDto
            {
                Currency = Shared.Enums.Currency.RSD,
                Amount = 100,
                EntityId = Guid.NewGuid(),
                PaymentType = Shared.Enums.PaymentType.InvoicePayment,
                Description="Test 1",
                BankAccountId=Guid.NewGuid(),
                ReferenceNumber="Test 1"
            };
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = Guid.NewGuid(),
                PaymentType = addPaymentDto.PaymentType,
                Amount = addPaymentDto.Amount,
                Currency = addPaymentDto.Currency,
                BankAccountId = addPaymentDto.BankAccountId,
                ReferenceNumber = addPaymentDto.ReferenceNumber,
                Description = addPaymentDto.Description,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            var invoice = new Invoice
            {

            };
            _unitOfWorkMock
                .Setup(x => x.InvoiceRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Invoice, bool>>>()))
                .ReturnsAsync((Invoice?)null);
            _localizationHelperMock
                .Setup(x => x.InvoiceNotFound)
                .Returns("Račun nije pronadjen.");
            //Act
            var result = await Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _paymentService.CreateAsync(addPaymentDto));

            //Assert
            Assert.Equal("Račun nije pronadjen.", result.Message);
            
        }
        [Fact]
        public async Task CreateAsync_InvoicePayment_WhenPaymentExceedsRemainingAmount_ShouldThrowInvalidOperationException()
        {
            //Arrange
            var addPaymentDto = new AddPaymentDto
            {
                Currency = Shared.Enums.Currency.RSD,
                Amount = 100,
                EntityId = Guid.NewGuid(),
                PaymentType = Shared.Enums.PaymentType.InvoicePayment,
                Description = "Test 1",
                BankAccountId = Guid.NewGuid(),
                ReferenceNumber = "Test 1"
            };
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = Guid.NewGuid(),
                PaymentType = addPaymentDto.PaymentType,
                Amount = addPaymentDto.Amount,
                Currency = addPaymentDto.Currency,
                BankAccountId = addPaymentDto.BankAccountId,
                ReferenceNumber = addPaymentDto.ReferenceNumber,
                Description = addPaymentDto.Description,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                AmountRSD = addPaymentDto.Amount
            };
            var invoice = new Invoice
            {
                TotalAmount=50,
                TotalAmountRSD=50,
                AmountToPay=50
            };
            _unitOfWorkMock
                .Setup(x => x.InvoiceRepository.FindFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Invoice, bool>>>()))
                .ReturnsAsync(invoice);
            _localizationHelperMock
                .Setup(x => x.PaymentExceedsRemainingAmount)
                .Returns("Plaćanje prevazilazi preostali iznos.");
            //Act
            var result = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => _paymentService.CreateAsync(addPaymentDto));

            //Assert
            Assert.Equal("Plaćanje prevazilazi preostali iznos.", result.Message);
        }
        [Fact]
        public async Task CreateAsync_InvoicePayment_WhenValid_ShouldUpdateInvoiceStatusAndCreatePayment()
        {
            // Arrange
            var entityId = Guid.NewGuid();

            var addPaymentDto = new AddPaymentDto
            {
                Currency = Shared.Enums.Currency.RSD,
                Amount = 100,
                EntityId = entityId,
                PaymentType = Shared.Enums.PaymentType.InvoicePayment,
                Description = "Test 1",
                BankAccountId = Guid.NewGuid(),
                ReferenceNumber = "Test 1"
            };

            var invoice = new Invoice
            {
                Id = entityId,
                TotalAmount = 150,
                TotalAmountRSD = 150,
                AmountToPay = 150,
                PaymentStatus = PaymentStatus.Unpaid
            };

            var paymentToReturnDto = new PaymentToReturnDto
            {
                Id = Guid.NewGuid(),
                Amount = addPaymentDto.Amount
            };

            _unitOfWorkMock
                .Setup(x => x.InvoiceRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Invoice, bool>>>()))
                .ReturnsAsync(invoice);

            _unitOfWorkMock
                .Setup(x => x.PaymentRepository.AddAsync(It.IsAny<Payment>())) 
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            _mapperMock
                .Setup(x => x.Map<PaymentToReturnDto>(It.IsAny<Payment>()))
                .Returns(paymentToReturnDto);

            // Act
            var result = await _paymentService.CreateAsync(addPaymentDto); 

            // Assert
            Assert.Equal(50, invoice.AmountToPay); 
            Assert.Equal(PaymentStatus.PartiallyPaid, invoice.PaymentStatus);

            _unitOfWorkMock.Verify(x => x.InvoiceRepository.Update(invoice), Times.Once());
            _unitOfWorkMock.Verify(x => x.PaymentRepository.AddAsync(It.Is<Payment>(p =>
                p.Amount == addPaymentDto.Amount &&
                p.InvoiceId == entityId
            )), Times.Once()); 

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once());

            Assert.NotNull(result);
        }
        [Fact]
        public async Task CreateAsync_TaxPayment_WhenFutureObligation_ShouldThrowInvalidOperationException()
        {
            //Arrange
            var addPaymentDto = new AddPaymentDto
            {
                Currency = Shared.Enums.Currency.RSD,
                Amount = 100,
                EntityId = Guid.NewGuid(),
                PaymentType = Shared.Enums.PaymentType.TaxPayment,
                Description = "Test 1",
                BankAccountId = Guid.NewGuid(),
                ReferenceNumber = "Test 1"
            };
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = Guid.NewGuid(),
                PaymentType = addPaymentDto.PaymentType,
                Amount = addPaymentDto.Amount,
                Currency = addPaymentDto.Currency,
                BankAccountId = addPaymentDto.BankAccountId,
                ReferenceNumber = addPaymentDto.ReferenceNumber,
                Description = addPaymentDto.Description,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                AmountRSD = addPaymentDto.Amount
            };
            var taxObligation = new TaxObligation
            {
                Month=1,
                Year=DateTime.Now.Year+1
            };
            _unitOfWorkMock
                .Setup(x => x.TaxObligationRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<TaxObligation, bool>>>()))
                .ReturnsAsync(taxObligation);
            _localizationHelperMock
                .Setup(x => x.CannotMarkFutureObligationAsPaid)
                .Returns("Ne mozete oznaciti buduce obaveze kao placene!");
            //Act
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentService.CreateAsync(addPaymentDto));

            //Assert
            Assert.Equal("Ne mozete oznaciti buduce obaveze kao placene!", result.Message);
        }
        [Fact]
        public async Task CreateAsync_TaxPayment_WhenNotPaidInFull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var entityId = Guid.NewGuid();

            var addPaymentDto = new AddPaymentDto
            {
                Currency = Shared.Enums.Currency.RSD,
                Amount = 100, 
                EntityId = entityId,
                PaymentType = Shared.Enums.PaymentType.TaxPayment,
                Description = "Porez test",
                BankAccountId = Guid.NewGuid(),
                ReferenceNumber = "12345"
            };

            var taxObligation = new TaxObligation
            {
                Id = entityId,
                Month = 5,
                Year = 2026,
                TotalAmount = 101
            };

            _unitOfWorkMock
                .Setup(x => x.TaxObligationRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<TaxObligation, bool>>>()))
                .ReturnsAsync(taxObligation);

            _localizationHelperMock
                .Setup(x => x.TaxObligationMustBePaidInFull)
                .Returns("Porez mora biti placen u potpunosti!");

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentService.CreateAsync(addPaymentDto));

            // Assert
            Assert.Equal("Porez mora biti placen u potpunosti!", exception.Message);
        }
        [Fact]
        public async Task CreateAsync_ExpensePayment_WhenAmountMismatch_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var entityId = Guid.NewGuid();

            var addPaymentDto = new AddPaymentDto
            {
                Currency = Shared.Enums.Currency.RSD,
                Amount = 100,
                EntityId = entityId,
                PaymentType = Shared.Enums.PaymentType.ExpensePayment,
                Description = "Porez test",
                BankAccountId = Guid.NewGuid(),
                ReferenceNumber = "12345"
            };

            var expense = new Expense
            {
                Id=entityId,
                Amount=101,
                Name="Test 1",
                ReferenceNumber=Guid.NewGuid().ToString()
            };
            _unitOfWorkMock
                .Setup(x => x.ExpenseRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Expense, bool>>>()))
                .ReturnsAsync(expense);

            _localizationHelperMock
               .Setup(x => x.ExpenseMustBePaidInFull)
               .Returns("Trosak mora biti placen u potpunosti!");

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _paymentService.CreateAsync(addPaymentDto));

            // Assert
            Assert.Equal("Trosak mora biti placen u potpunosti!", exception.Message);

        }
        [Fact]
        public async Task GetByIdAsync_WhenPaymentDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _unitOfWorkMock
                .Setup(x => x.PaymentRepository.FindPaymentWithEntitiesAsync(It.IsAny<Expression<Func<Payment, bool>>>() ))
                .ReturnsAsync((Payment?)null);

            // Act
            var result = await _paymentService.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
            _unitOfWorkMock.Verify(x => x.PaymentRepository.FindPaymentWithEntitiesAsync(It.IsAny<Expression<Func<Payment, bool>>>()), Times.Once());
        }
        [Fact]
        public async Task GetSummaryAsync_ShouldCalculateCorrectTotalsAndCounts()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new Payment { PaymentType = PaymentType.InvoicePayment, AmountRSD = 100 },
                new Payment { PaymentType = PaymentType.InvoicePayment, AmountRSD = 50 },
                new Payment { PaymentType = PaymentType.TaxPayment, AmountRSD = 75 },
                new Payment { PaymentType = PaymentType.ExpensePayment, AmountRSD = 25 },
                new Payment { PaymentType = PaymentType.TaxPayment, AmountRSD = 25 }
            };

            _unitOfWorkMock
                .Setup(x => x.PaymentRepository.FindAllAsync(It.IsAny<Expression<Func<Payment, bool>>>() ))
                .ReturnsAsync(payments);

            // Act
            var summary = await _paymentService.GetSummaryAsync();

            // Assert
            Assert.Equal(150, summary.TotalInvoicePayments);
            Assert.Equal(100, summary.TotalTaxPayments);
            Assert.Equal(25, summary.TotalExpensePayments);
            Assert.Equal(2, summary.CountInvoicePayments);
            Assert.Equal(2, summary.CountTaxPayments);
            Assert.Equal(1, summary.CountExpensePayments);
        }
        [Fact]
        public async Task DeleteAsync_WhenPaymentNotFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            _unitOfWorkMock
                .Setup(x => x.PaymentRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Payment, bool>>>() ))
                .ReturnsAsync((Payment?)null);

            _localizationHelperMock
                .Setup(x => x.PaymentNotFound)
                .Returns("Placanje nije pronadjeno.");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _paymentService.DeleteAsync(Guid.NewGuid()));
            Assert.Equal("Placanje nije pronadjeno.", ex.Message);
        }
        [Fact]
        public async Task DeleteAsync_WhenInvoicePaymentDeleted_ShouldRevertInvoiceAmountToPayAndStatus()
        {
            // Arrange
            var invoiceId = Guid.NewGuid();
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = Guid.NewGuid(),
                PaymentType = PaymentType.InvoicePayment,
                InvoiceId = invoiceId,
                Amount = 100
            };

            var invoice = new Invoice
            {
                Id = invoiceId,
                TotalAmount = 150,
                AmountToPay = 50,
                PaymentStatus = PaymentStatus.PartiallyPaid,
                InvoiceStatus = InvoiceStatus.Finished
            };

            _unitOfWorkMock
                .Setup(x => x.PaymentRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Payment, bool>>>() ))
                .ReturnsAsync(payment);

            _unitOfWorkMock
                .Setup(x => x.InvoiceRepository.GetByIdAsync(invoiceId))
                .ReturnsAsync(invoice);

            _unitOfWorkMock
                .Setup(x => x.PaymentRepository.Remove(It.IsAny<Payment>()));

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _paymentService.DeleteAsync(payment.Id);

            // Assert
            Assert.Equal(150, invoice.AmountToPay);
            Assert.Equal(PaymentStatus.Unpaid, invoice.PaymentStatus);
            Assert.Equal(InvoiceStatus.Draft, invoice.InvoiceStatus);

            _unitOfWorkMock.Verify(x => x.InvoiceRepository.Update(invoice), Times.Once());
            _unitOfWorkMock.Verify(x => x.PaymentRepository.Remove(payment), Times.Once());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once());
        }
        [Fact]
        public async Task DeleteAsync_WhenTaxPaymentDeleted_ShouldRevertTaxObligationToPending()
        {
            // Arrange
            var taxId = Guid.NewGuid();
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BusinessProfileId = Guid.NewGuid(),
                PaymentType = PaymentType.TaxPayment,
                TaxObligationId = taxId,
                Amount = 200
            };

            var taxObligation = new TaxObligation
            {
                Id = taxId,
                Status = TaxObligationStatus.Paid,
                PaidDate = DateTime.UtcNow
            };

            _unitOfWorkMock
                .Setup(x => x.PaymentRepository.FindFirstOrDefaultAsync(It.IsAny<Expression<Func<Payment, bool>>>() ))
                .ReturnsAsync(payment);

            _unitOfWorkMock
                .Setup(x => x.TaxObligationRepository.GetByIdAsync(taxId))
                .ReturnsAsync(taxObligation);

            _unitOfWorkMock
                .Setup(x => x.PaymentRepository.Remove(It.IsAny<Payment>()));

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _paymentService.DeleteAsync(payment.Id);

            // Assert
            Assert.Equal(TaxObligationStatus.Pending, taxObligation.Status);
            Assert.Null(taxObligation.PaidDate);

            _unitOfWorkMock.Verify(x => x.TaxObligationRepository.Update(taxObligation), Times.Once());
            _unitOfWorkMock.Verify(x => x.PaymentRepository.Remove(payment), Times.Once());
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once());
        }
    }
}
