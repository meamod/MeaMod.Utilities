using System.ServiceProcess;

namespace MeaMod.Utilities.Tests
{
    [TestClass]
    public class ServiceControllerExtensionsTests
    {
        // The Windows Event Log service is present and running on every supported Windows version.
        private const string AlwaysRunningService = "EventLog";

        [TestMethod]
        public async Task WaitForTheCurrentStatusCompletesImmediately()
        {
            using var controller = new ServiceController(AlwaysRunningService);

            await controller.WaitForStatusAsync(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
            await controller.WaitForStatusAsync(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5), CancellationToken.None);
        }

        [TestMethod]
        public async Task WaitForAStatusThatNeverArrivesTimesOut()
        {
            using var controller = new ServiceController(AlwaysRunningService);

            await Assert.ThrowsExactlyAsync<System.ServiceProcess.TimeoutException>(
                () => controller.WaitForStatusAsync(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(600)));
            await Assert.ThrowsExactlyAsync<System.ServiceProcess.TimeoutException>(
                () => controller.WaitForStatusAsync(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(600), CancellationToken.None));
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
