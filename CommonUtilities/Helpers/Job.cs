using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommonUtilities.Helpers
{
    public abstract class Job
    {
        public enum StatusType
        {
            Fresh,
            Queued,
            Processing,
            Canceled,
            Completed
        }

        public StatusType Status = StatusType.Fresh;

        public int WorkIterations = 0;

        public abstract bool Run(TimeSpan timeout);

        public void Cancel()
        {
            Status = StatusType.Canceled;
        }
    }

    public class Job<T> : Job
    {
        public T Result { get; private set; }

        private readonly IEnumerator<T> _yieldJob;

        private Action<T> _completionCallback;

        public Job(IEnumerable<T> yieldJob)
        {
            _yieldJob = yieldJob.GetEnumerator();
        }

        public Job(IEnumerator<T> yieldJob)
        {
            _yieldJob = yieldJob;
        }

        /// <summary>
        /// Sets the callback called when the job is complete.
        /// If the job is already complete, the callback will be invoked immediately.
        /// </summary>
        /// <param name="completionCallback">The callback action used when the job is complete</param>
        public void SetCompletionCallback(Action<T> completionCallback)
        {
            _completionCallback = completionCallback;
            if (Result != null)
            {
                _completionCallback.Invoke(Result);
            }
        }

        public override bool Run(TimeSpan timeout)
        {
            if (Status == StatusType.Completed || Status == StatusType.Canceled)
            {
                return true;
            }

            Status = StatusType.Processing;

            var end = DateTime.Now + timeout;
            while (DateTime.Now < end)
            {
                WorkIterations += 1;
                _yieldJob.MoveNext();
                var result = _yieldJob.Current;
                if (result != null)
                {
                    Complete(result);
                    return true;
                }
            }

            return false;
        }

        private void Complete(T completionResult)
        {
            Result = completionResult;
            Status = StatusType.Completed;
            _completionCallback?.Invoke(Result);
        }
    }
}
