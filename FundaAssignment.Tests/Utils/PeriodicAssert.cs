namespace FundaAssignment.Tests.Utils
{
    public class PeriodicAssert
    {
        private TimeSpan checkInterval;
        private TimeSpan expirationInterval;

        public static PeriodicAssert Create(TimeSpan? checkInterval = null, TimeSpan? expirationInterval = null)
        {
            return new PeriodicAssert()
            {
                checkInterval = checkInterval ?? TimeSpan.FromMilliseconds(10),
                expirationInterval = expirationInterval ?? TimeSpan.FromSeconds(3)
            };
        }

        public Task ExecuteAsync(Func<bool> assertCheck)
        {
            return ExecuteAsync(() => Task.FromResult(assertCheck()));
        }

        public Task ExecuteAsync(Action assertCheck)
        {
            return ExecuteAsync(() => { assertCheck(); return true; });
        }

        public Task ExecuteAsync(Func<Task> assertCheck)
        {
            return ExecuteAsync(async () => { await assertCheck(); return true; });
        }

        public async Task ExecuteAsync(Func<Task<bool>> assertCheck)
        {
            var startedAt = DateTime.Now;
            bool finished = false;
            Exception? exceptionToRaise = null;
            do
            {
                try
                {
                    finished = await assertCheck();
                }
                catch (Exception exc)
                {
                    exceptionToRaise = exc;
                }

                if (!finished)
                {
                    await Task.Delay((int)checkInterval.TotalMilliseconds);
                }
            } while (DateTime.Now.Subtract(startedAt) < expirationInterval && !finished);

            if (!finished)
                throw exceptionToRaise ??
                    new TimeoutException($"The periodic assert did not complete within the expiration interval of {expirationInterval.TotalMilliseconds} ms.");
        }
    }
}
