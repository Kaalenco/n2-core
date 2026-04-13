using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using N2.Core.Commands;
using N2.Core.PubSub;

namespace N2.Core.UnitTests.PubSub;

/// <summary>
/// Test implementation of IItemChanged for testing purposes.
/// </summary>
internal sealed class TestItemChanged : IItemChanged
{
	public string Name { get; set; } = string.Empty;
	public int Value { get; set; }
    public DateTime DateTime { get; set; } = DateTime.UtcNow;	
    public Type Type => this.GetType();
    public Guid Uuid { get; } = Guid.NewGuid();
}

/// <summary>
/// Test implementation of INotifyChangeListener for testing purposes.
/// </summary>
internal sealed class TestChangeListener : INotifyChangeListener
{
	public List<object> ReceivedItems { get; } = new();
	public int NotificationCount { get; private set; }

	public void OnItemModified(IItemChanged item)
	{
		ReceivedItems.Add(item);
		NotificationCount++;
	}
}

/// <summary>
/// Test implementation of INotifyChangeListener that throws an exception.
/// </summary>
internal sealed class FaultyChangeListener : INotifyChangeListener
{
	public bool WasCalled { get; private set; }

	public void OnItemModified(IItemChanged item)
	{
		WasCalled = true;
		throw new InvalidOperationException("Listener failed to process item");
	}
}

[TestClass]
public class WithInMemoryChangeService
{
	private static NullLogger<InMemoryChangeService> CreateLogger()
	{
		return NullLogger<InMemoryChangeService>.Instance;
	}

	[TestMethod]
	public void ConstructorInitializesService()
	{
		InMemoryChangeService sut = new(CreateLogger());

		Assert.IsNotNull(sut);
	}

	[TestMethod]
	public void AddSubscriptionWithValidListenerSucceeds()
	{
		InMemoryChangeService sut = new(CreateLogger());
		TestChangeListener listener = new();

		sut.AddSubscription(listener);

		// Verify by triggering notification
		TestItemChanged item = new() { Name = "Test", Value = 42 };
		sut.ItemModified(new TrackingId(), item);

		Assert.AreEqual(1, listener.NotificationCount);
		Assert.AreEqual(1, listener.ReceivedItems.Count);
	}

	[TestMethod]
	public void AddSubscriptionWithNullListenerDoesNotThrow()
	{
		InMemoryChangeService sut = new(CreateLogger());

		sut.AddSubscription(null!);

		// Should not throw exception
		Assert.IsNotNull(sut);
	}

	[TestMethod]
	public void AddSubscriptionAllowsMultipleListenersOfSameType()
	{
		InMemoryChangeService sut = new(CreateLogger());
		TestChangeListener listener1 = new();
		TestChangeListener listener2 = new();

		sut.AddSubscription(listener1);
		sut.AddSubscription(listener2);

		TestItemChanged item = new() { Name = "Test", Value = 42 };
		sut.ItemModified(new TrackingId(), item);

		Assert.AreEqual(1, listener1.NotificationCount);
		Assert.AreEqual(1, listener2.NotificationCount);
	}

	[TestMethod]
	public void ItemModifiedNotifiesRegisteredListeners()
	{
		InMemoryChangeService sut = new(CreateLogger());
		TestChangeListener listener = new();
		sut.AddSubscription(listener);

		TestItemChanged item = new() { Name = "TestItem", Value = 100 };
		TrackingId trackingId = new();

		sut.ItemModified(trackingId, item);

		Assert.AreEqual(1, listener.NotificationCount);
		Assert.AreEqual(1, listener.ReceivedItems.Count);
		Assert.AreSame(item, listener.ReceivedItems[0]);
	}

	[TestMethod]
	public void ItemModifiedWithNoListenersDoesNotThrow()
	{
		InMemoryChangeService sut = new(CreateLogger());

		TestItemChanged item = new() { Name = "Test", Value = 42 };
		TrackingId trackingId = new();

		sut.ItemModified(trackingId, item);

		// Should not throw exception
		Assert.IsNotNull(sut);
	}

	[TestMethod]
	public void ItemModifiedHandlesListenerException()
	{
		InMemoryChangeService sut = new(CreateLogger());
		FaultyChangeListener faultyListener = new();
		TestChangeListener goodListener = new();

		sut.AddSubscription(faultyListener);
		sut.AddSubscription(goodListener);

		TestItemChanged item = new() { Name = "Test", Value = 42 };
		TrackingId trackingId = new();

		sut.ItemModified(trackingId, item);

		// Faulty listener should have been called
		Assert.IsTrue(faultyListener.WasCalled);
		// Good listener should still receive notification despite faulty listener
		Assert.AreEqual(1, goodListener.NotificationCount);
	}

	[TestMethod]
	public void RemoveSubscriptionWithValidListenerRemovesIt()
	{
		InMemoryChangeService sut = new(CreateLogger());
		TestChangeListener listener = new();

		sut.AddSubscription(listener);
		sut.RemoveSubscription(listener);

		TestItemChanged item = new() { Name = "Test", Value = 42 };
		sut.ItemModified(new TrackingId(), item);

		// Listener should not be notified after removal
		Assert.AreEqual(0, listener.NotificationCount);
	}

	[TestMethod]
	public void RemoveSubscriptionWithNullListenerDoesNotThrow()
	{
		InMemoryChangeService sut = new(CreateLogger());

		sut.RemoveSubscription(null!);

		// Should not throw exception
		Assert.IsNotNull(sut);
	}

	[TestMethod]
	public void RemoveSubscriptionRemovesOnlySpecifiedListener()
	{
		InMemoryChangeService sut = new(CreateLogger());
		TestChangeListener listener1 = new();
		TestChangeListener listener2 = new();

		sut.AddSubscription(listener1);
		sut.AddSubscription(listener2);
		sut.RemoveSubscription(listener1);

		TestItemChanged item = new() { Name = "Test", Value = 42 };
		sut.ItemModified(new TrackingId(), item);

		Assert.AreEqual(0, listener1.NotificationCount);
		Assert.AreEqual(1, listener2.NotificationCount);
	}

	[TestMethod]
	public void RemoveSubscriptionWithUnregisteredListenerDoesNotThrow()
	{
		InMemoryChangeService sut = new(CreateLogger());
		TestChangeListener listener = new();

		sut.RemoveSubscription(listener);

		// Should not throw exception
		Assert.IsNotNull(sut);
	}

	[TestMethod]
	public void RegisterAddsServiceToCollection()
	{
		IServiceCollection services = new ServiceCollection();

		InMemoryChangeService.Register(services);

		ServiceDescriptor? descriptor = services.FirstOrDefault(
			s => s.ServiceType == typeof(INotifyChangeService));

		Assert.IsNotNull(descriptor);
		Assert.AreEqual(typeof(InMemoryChangeService), descriptor.ImplementationType);
		Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
	}

	[TestMethod]
	public void RegisterWithNullServicesThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => InMemoryChangeService.Register(null!));
	}

	[TestMethod]
	public void RegisteredServiceCanBeResolvedFromContainer()
	{
		IServiceCollection services = new ServiceCollection();
		services.AddLogging();
		InMemoryChangeService.Register(services);
		ServiceProvider sp = services.BuildServiceProvider();

		INotifyChangeService? service = sp.GetService<INotifyChangeService>();

		Assert.IsNotNull(service);
		Assert.IsInstanceOfType<InMemoryChangeService>(service);
	}

	[TestMethod]
	public void MultipleNotificationsIncrementListenerCount()
	{
		InMemoryChangeService sut = new(CreateLogger());
		TestChangeListener listener = new();
		sut.AddSubscription(listener);

		for (int i = 0; i < 5; i++)
		{
			TestItemChanged item = new() { Name = $"Item{i}", Value = i };
			sut.ItemModified(new TrackingId(), item);
		}

		Assert.AreEqual(5, listener.NotificationCount);
		Assert.AreEqual(5, listener.ReceivedItems.Count);
	}
}