using System.ServiceProcess;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class ServiceControllerExtensionsTests
    {
        // The Windows Event Log service is present and running on every supported Windows version.
        private const string AlwaysRunningService = "EventLog";

        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        public async Task WaitForTheCurrentStatusCompletesImmediately()
        {
            using var controller = new ServiceController(AlwaysRunningService);

#pragma warning disable MSTEST0049 // the overload without a cancellation token is what this test covers
            await controller.WaitForStatusAsync(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
#pragma warning restore MSTEST0049
            await controller.WaitForStatusAsync(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5), TestContext.CancellationToken);
        }

        [TestMethod]
        public async Task WaitForAStatusThatNeverArrivesTimesOut()
        {
            using var controller = new ServiceController(AlwaysRunningService);

#pragma warning disable MSTEST0049 // the overload without a cancellation token is what this test covers
            await Assert.ThrowsExactlyAsync<System.ServiceProcess.TimeoutException>(
                () => controller.WaitForStatusAsync(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(600)));
#pragma warning restore MSTEST0049
            await Assert.ThrowsExactlyAsync<System.ServiceProcess.TimeoutException>(
                () => controller.WaitForStatusAsync(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(600), TestContext.CancellationToken));
        }

        [TestMethod]
        public async Task CancellationStopsTheWait()
        {
            using var controller = new ServiceController(AlwaysRunningService);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsExactlyAsync<TaskCanceledException>(
                () => controller.WaitForStatusAsync(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(1), cancellation.Token));
        }
    }
}
