using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace MeaMod.Utilities
{
    /// <summary>
	/// async ServiceController.WaitForStatus class
    /// <para><see href="https://stackoverflow.com/a/38236239"/></para>
    /// <para>Licence: CC BY-SA 3.0</para>
	/// </summary>
	/// 
    public static class ServiceControllerExtensions
    {
        /// <summary>
        /// Async method to wait for Windows Server status change
        /// </summary>
        /// <param name="controller">SeviceController for Windows Service to wait</param>
        /// <param name="desiredStatus">Windows Service status to wait for</param>
        /// <param name="timeout">TimeSpan to wait for</param>
        /// <returns>Task of WaitForStatusAsync</returns>
        public async static Task WaitForStatusAsync(this ServiceController controller, ServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            var utcNow = DateTime.UtcNow;
            controller.Refresh();
            while (controller.Status != desiredStatus)
            {
                if (DateTime.UtcNow - utcNow > timeout)
                {
                    throw new System.ServiceProcess.TimeoutException($"Failed to wait for '{controller.ServiceName}' to change status to '{desiredStatus}'.");
                }

                await Task.Delay(250).ConfigureAwait(false);
                controller.Refresh();
            }
        }
        /// <summary>
        /// Async method to wait for Windows Server status change
        /// </summary>
        /// <param name="controller">SeviceController for Windows Service to wait</param>
        /// <param name="desiredStatus">Windows Service status to wait for</param>
        /// <param name="timeout">TimeSpan to wait for</param>
        /// <param name="cancellationToken">Async cancellation token</param>
        /// <returns>Task of WaitForStatusAsync</returns>
        public static async Task WaitForStatusAsync(this ServiceController controller, ServiceControllerStatus desiredStatus, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var utcNow = DateTime.UtcNow;
            controller.Refresh();
            while (controller.Status != desiredStatus)
            {
                if (DateTime.UtcNow - utcNow > timeout)
                {
                    throw new System.ServiceProcess.TimeoutException($"Failed to wait for '{controller.ServiceName}' to change status to '{desiredStatus}'.");
                }
                await Task.Delay(250, cancellationToken)
                    .ConfigureAwait(false);
                controller.Refresh();
            }
        }

    }

}
