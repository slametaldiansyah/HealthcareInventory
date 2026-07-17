using FluentAssertions;
using OrderInventory.Domain.Entities;

namespace OrderInventory.UnitTests;

public class InventoryItemTests
{
    [Fact]
    public void Reserve_IncreasesReserved_WhenStockAvailable()
    {
        var item = new InventoryItem { Sku = "A1", ActualQty = 10, ReservedQty = 0 };

        item.Reserve(2);

        item.ReservedQty.Should().Be(2);
        item.ActualQty.Should().Be(10);
        item.AvailableQty.Should().Be(8);
    }

    [Fact]
    public void Reserve_Throws_WhenInsufficientAvailable()
    {
        var item = new InventoryItem { Sku = "A1", ActualQty = 5, ReservedQty = 4 };

        var act = () => item.Reserve(2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient available stock*");
        item.ReservedQty.Should().Be(4);
    }

    [Fact]
    public void CommitReservation_MovesReservedToActual()
    {
        var item = new InventoryItem { Sku = "A1", ActualQty = 10, ReservedQty = 2 };

        item.CommitReservation(2);

        item.ActualQty.Should().Be(8);
        item.ReservedQty.Should().Be(0);
    }

    [Fact]
    public void ReleaseReservation_ReturnsReservedStock()
    {
        var item = new InventoryItem { Sku = "A1", ActualQty = 10, ReservedQty = 2 };

        item.ReleaseReservation(2);

        item.ActualQty.Should().Be(10);
        item.ReservedQty.Should().Be(0);
    }
}
