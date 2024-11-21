using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Services
{
    public class ResponseHandlerService
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _responseHandlers;
        private readonly ConcurrentDictionary<string, byte[]> _earlyResponses;
        public ResponseHandlerService()
        {
            _responseHandlers = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();
            _earlyResponses = new ConcurrentDictionary<string, byte[]>();
        }

        /// <summary>
        /// Registers a new request and waits for the response with timeout.
        /// </summary>
        /// <param name="requestId">Unique identifier for the request.</param>
        /// <param name="timeout">The maximum duration to wait for the response.</param>
        /// <param name="token">Cancellation token for the operation.</param>
        /// <returns>The response data or throws an exception on timeout.</returns>
        public async Task<byte[]> WaitForResponseAsync(string requestId, TimeSpan timeout, CancellationToken token)
        {
            // Check if the response was received early
            if (_earlyResponses.TryRemove(requestId, out var earlyResponse))
            {
                return earlyResponse;
            }

            var completionSource = new TaskCompletionSource<byte[]>();
            if (!_responseHandlers.TryAdd(requestId, completionSource))
                throw new InvalidOperationException($"Request with ID {requestId} is already registered.");

            try
            {
                // Create a task that completes when the token is canceled
                var cancellationTask = new TaskCompletionSource<bool>();
                using (token.Register(() => cancellationTask.TrySetResult(true)))
                {
                    // Wait for the response, timeout, or cancellation
                    var timeoutTask = Task.Delay(timeout);
                    var completedTask = await Task.WhenAny(completionSource.Task, timeoutTask, cancellationTask.Task);

                    if (completedTask == completionSource.Task)
                    {
                        // Response received
                        return await completionSource.Task;
                    }
                    else if (completedTask == timeoutTask)
                    {
                        // Timeout occurred
                        throw new TimeoutException($"Request {requestId} timed out.");
                    }
                    else
                    {
                        // Cancellation requested
                        throw new OperationCanceledException(token);
                    }
                }
            }
            finally
            {
                // Ensure the handler is always cleaned up
                _responseHandlers.TryRemove(requestId, out _);
            }
        }

        /// <summary>
        /// Completes the response for a given request ID.
        /// </summary>
        /// <param name="requestId">Unique identifier for the request.</param>
        /// <param name="response">The response data.</param>
        /// <returns>True if the request was successfully completed; otherwise, false.</returns>
        public bool CompleteRequest(string requestId, byte[] response)
        {
            // Check if the handler is already registered
            if (_responseHandlers.TryRemove(requestId, out var completionSource))
            {
                return completionSource.TrySetResult(response);
            }

            // Store the response for later if the handler is not yet registered
            _earlyResponses[requestId] = response;
            return true;
        }

        /// <summary>
        /// Cancels the request with a timeout exception.
        /// </summary>
        /// <param name="requestId">Unique identifier for the request.</param>
        public void TimeoutRequest(string requestId)
        {
            if (_responseHandlers.TryRemove(requestId, out var completionSource))
            {
                completionSource.TrySetException(new TimeoutException($"Request {requestId} timed out."));
            }
        }
    }

}
