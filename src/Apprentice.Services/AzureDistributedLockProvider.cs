﻿using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DAS.ProvideFeedback.Apprentice.Services
{
    public class AzureDistributedLockProvider : IDistributedLockProvider
    {
        private readonly CloudBlobClient _client;
        private readonly ILogger _logger;
        private readonly List<ControlledLock> _locks = new List<ControlledLock>();
        private readonly AutoResetEvent _mutex = new AutoResetEvent(true);
        private readonly string _containerName;
        private CloudBlobContainer _container;
        private Timer _renewTimer;
        private TimeSpan LockTimeout => TimeSpan.FromMinutes(1);
        private TimeSpan RenewInterval => TimeSpan.FromSeconds(45);

        public AzureDistributedLockProvider(string connectionString, ILoggerFactory logFactory, string containerName = "distributed-locks")
        {
            _logger = logFactory.CreateLogger<AzureDistributedLockProvider>();
            var account = CloudStorageAccount.Parse(connectionString);
            _containerName = containerName;
            _client = account.CreateCloudBlobClient();            
        }

        public async Task<bool> AcquireLock(string Id, CancellationToken cancellationToken)
        {
            var blob = _container.GetBlockBlobReference(Id);

            if (!await blob.ExistsAsync())
            {
                await blob.UploadTextAsync(string.Empty);
            }

            if (_mutex.WaitOne())
            {
                try
                {
                    var leaseId = await blob.AcquireLeaseAsync(LockTimeout);
                    _locks.Add(new ControlledLock(Id, leaseId, blob));
                    return true;
                }
                catch (StorageException ex)
                {
                    _logger.LogDebug($"Failed to acquire lock {Id} - {ex.Message}");
                    return false;
                }
                finally
                {
                    _mutex.Set();
                }
            }
            return false;
        }

        public async Task ReleaseLock(string Id)
        {
            if (_mutex.WaitOne())
            {
                try
                {
                    var entry = _locks.FirstOrDefault(x => x.Id == Id);

                    if (entry != null)
                    {
                        try
                        {
                            await entry.Blob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(entry.LeaseId));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error releasing lock - {ex.Message}");
                        }
                        _locks.Remove(entry);
                    }
                }
                finally
                {
                    _mutex.Set();
                }
            }
        }

        public async Task Start()
        {
            _container = _client.GetContainerReference(_containerName);
            await _container.CreateIfNotExistsAsync();
            _renewTimer = new Timer(RenewLeases, null, RenewInterval, RenewInterval);
        }

        public Task Stop()
        {
            if (_renewTimer != null)
            {
                _renewTimer.Dispose();
                _renewTimer = null;
            }
            
            return Task.CompletedTask;
        }

        private async void RenewLeases(object state)
        {
            //_logger.LogDebug("Renewing active leases");
            if (_mutex.WaitOne())
            {
                try
                {
                    foreach (var entry in _locks)
                        await RenewLock(entry);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error renewing leases - {ex.Message}");
                }
                finally
                {
                    _mutex.Set();
                }
            }
        }

        private async Task RenewLock(ControlledLock entry)
        {
            try
            {
                await entry.Blob.RenewLeaseAsync(AccessCondition.GenerateLeaseCondition(entry.LeaseId));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error renewing lease - {ex.Message}");
            }
        }
    }
}
