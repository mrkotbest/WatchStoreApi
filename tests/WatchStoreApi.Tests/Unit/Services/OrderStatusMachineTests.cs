using WatchStoreApi.Application.Services;
using WatchStoreApi.Domain.Enums;

namespace WatchStoreApi.Tests.Unit.Services;

public class OrderStatusMachineTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Processing, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Processing, OrderStatus.Shipped, true)]
    [InlineData(OrderStatus.Processing, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Refunded, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Delivered, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Shipped, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Refunded, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed, false)]
    [InlineData(OrderStatus.Refunded, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Delivered, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Processing, false)]
    public void CanTransition_ReturnsExpected(OrderStatus from, OrderStatus to, bool expected)
    {
        var result = OrderStatusMachine.CanTransition(from, to);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TerminalStates_HaveNoTransitions()
    {
        foreach (var target in Enum.GetValues<OrderStatus>())
        {
            Assert.False(OrderStatusMachine.CanTransition(OrderStatus.Cancelled, target));
            Assert.False(OrderStatusMachine.CanTransition(OrderStatus.Refunded, target));
        }
    }
}
