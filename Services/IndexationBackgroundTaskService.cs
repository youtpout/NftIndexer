using System;
using Nethereum.JsonRpc.Client;
using NftIndexer.Interfaces;

namespace NftIndexer.Services
{
    public class IndexationBackgroundTaskService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<IndexationBackgroundTaskService> _logger;
        private readonly IConfiguration _configuration;
        private Timer _timer;
        private IServiceScopeFactory _serviceScopeFactory;
        private static bool workFinished = true;

        public IndexationBackgroundTaskService(ILogger<IndexationBackgroundTaskService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }



        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("IndexationBackgroundTaskService running.");

            int timeout = int.Parse(_configuration["Timeout"]);

            _timer = new Timer(async (e) => { await DoWork(e); }, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(timeout));

            return Task.CompletedTask;
        }


        private async Task DoWork(object state)
        {
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation(
                "IndexationBackgroundTaskService is working. Count: {Count}", count);

            if (count < 2)
            {
                // don't start before 2 minutes
                return;
            }

            if (!workFinished)
            {
                _logger.LogInformation("Work not finished");

                return;
            }


            try
            {
                workFinished = false;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    IIndexationService dataService = scope.ServiceProvider.GetRequiredService<IIndexationService>();

                    //Do your stuff
                    await dataService.IndexData();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexationBackgroundTaskService on IndexationService IndexData method.");
            }
            finally
            {
                workFinished = true;
            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("IndexationBackgroundTaskService is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

