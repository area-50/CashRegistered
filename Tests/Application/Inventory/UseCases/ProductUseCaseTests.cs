using Application.Financial.Interfaces;
using Application.Inventory.Interfaces;
using Application.Inventory.UseCases;
using Domain.Inventory.Entities;

using Domain.Inventory.Repositories;
using FluentAssertions;
using Moq;
using Shared.Abstractions;
using Shared.Inventory.Request;
using Shared.Notifications;
using Xunit;

namespace Tests.Application.Inventory.UseCases;

public class ProductUseCaseTests
{
    private readonly Mock<ITagUseCase> _tagUseCaseMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IStockBalanceUseCase> _stockBalanceUseCaseMock;
    private readonly Mock<IWarehouseUseCase> _warehouseUseCaseMock;
    private readonly NotificationContext _notificationContext;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProductUseCase _productUseCase;

    public ProductUseCaseTests()
    {
        _tagUseCaseMock = new Mock<ITagUseCase>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _stockBalanceUseCaseMock = new Mock<IStockBalanceUseCase>();
        _warehouseUseCaseMock = new Mock<IWarehouseUseCase>();
        _notificationContext = new NotificationContext();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _productUseCase = new ProductUseCase(
            _tagUseCaseMock.Object,
            _productRepositoryMock.Object,
            _stockBalanceUseCaseMock.Object,
            _warehouseUseCaseMock.Object,
            _notificationContext,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task CreateProduct_ShouldInitializeStockBalanceForEachWarehouse()
    {
        var request = new CreateProductRequest
        {
            Sku = "SKU001",
            Name = "Product 1",
            CategoryId = 1,
            BaseUomId = 1
        };

        var warehouses = new List<Warehouse>
        {
            new("Warehouse 1", "Physical"),
            new("Warehouse 2", "Physical")
        };

        _tagUseCaseMock.Setup(x => x.GetTagByIds(It.IsAny<IList<int>>())).ReturnsAsync(new List<Tag>());
        _warehouseUseCaseMock.Setup(x => x.ListAll()).ReturnsAsync(warehouses);
        _productRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Product>()))
            .Callback<Product>(p => typeof(BaseEntity).GetProperty("Id")?.SetValue(p, 1));

        var result = await _productUseCase.CreateProduct(request);

        result.Id.Should().NotBe(0);
        _stockBalanceUseCaseMock.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<StockBalance>>(s => s.Count() == warehouses.Count)), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Exactly(2));
        _notificationContext.IsInvalid.Should().BeFalse();
    }
}
