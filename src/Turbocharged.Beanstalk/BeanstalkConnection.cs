﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Turbocharged.Beanstalk
{
    /// <summary>
    /// A connection to a Beanstalk server.
    /// </summary>
    public sealed class BeanstalkConnection : IProducer, IConsumer, IDisposable
    {
        string _hostname;
        int _port;
        PhysicalConnection _connection;

        // Private, so we can make the object creation async
        private BeanstalkConnection(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        private Task ConnectAsync()
        {
            _connection = new PhysicalConnection(_hostname, _port);
            return _connection.ConnectAsync();
        }

        /// <summary>
        /// Creates a new connection to a Beanstalk server.
        /// </summary>
        public static async Task<BeanstalkConnection> ConnectAsync(string hostname, int port)
        {
            var connection = new BeanstalkConnection(hostname, port);
            await connection.ConnectAsync();
            return connection;
        }

        public void Close()
        {
            var c = _connection;
            _connection = null;
            if (c != null)
            {
                try
                {
                    c.Close();
                }
                catch
                {
                }
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        public IProducer GetProducer()
        {
            return this;
        }

        public IConsumer GetConsumer()
        {
            return this;
        }

        #region Producer

        Task<string> IProducer.UseAsync(string tube)
        {
            var request = new UseRequest(tube);
            return SendAndGetResult(request);
        }

        Task<string> IProducer.UsingAsync()
        {
            var request = new UsingRequest();
            return SendAndGetResult(request);
        }

        Task<int> IProducer.PutAsync(byte[] job, int priority, TimeSpan delay, TimeSpan timeToRun)
        {
            var request = new PutRequest(job, priority, delay, timeToRun);
            return SendAndGetResult(request);
        }

        Task<Job> IProducer.PeekAsync()
        {
            return ((IProducer)this).PeekAsync(JobState.Ready);
        }

        Task<Job> IProducer.PeekAsync(JobState state)
        {
            var request = new PeekRequest(state);
            return SendAndGetResult(request);
        }

        Task<Job> IProducer.PeekAsync(int id)
        {
            var request = new PeekRequest(id);
            return SendAndGetResult(request);
        }

        Task<TubeStatistics> IProducer.TubeStatisticsAsync(string tube)
        {
            var request = new TubeStatisticsRequest(tube);
            return SendAndGetResult(request);
        }

        #endregion

        #region Consumer

        Task<int> IConsumer.WatchAsync(string tube)
        {
            var request = new WatchRequest(tube);
            return SendAndGetResult(request);
        }

        Task<int> IConsumer.IgnoreAsync(string tube)
        {
            var request = new IgnoreRequest(tube);
            return SendAndGetResult(request);
        }

        Task<List<string>> IConsumer.WatchedAsync()
        {
            var request = new WatchedRequest();
            return SendAndGetResult(request);
        }

        Task<Job> IConsumer.ReserveAsync(TimeSpan timeout)
        {
            var request = new ReserveRequest(timeout);
            return SendAndGetResult(request);
        }

        Task<Job> IConsumer.ReserveAsync()
        {
            var request = new ReserveRequest();
            return SendAndGetResult(request);
        }

        Task<Job> IConsumer.PeekAsync(int id)
        {
            return ((IProducer)this).PeekAsync(id);
        }

        Task<bool> IConsumer.DeleteAsync(int id)
        {
            var request = new DeleteRequest(id);
            return SendAndGetResult(request);
        }

        Task<bool> IConsumer.ReleaseAsync(int id, int priority, TimeSpan delay)
        {
            var request = new ReleaseRequest(id, priority, (int)delay.TotalSeconds);
            return SendAndGetResult(request);
        }

        Task<bool> IConsumer.BuryAsync(int id, int priority)
        {
            var request = new BuryRequest(id, priority);
            return SendAndGetResult(request);
        }

        Task<bool> IConsumer.TouchAsync(int id)
        {
            var request = new TouchRequest(id);
            return SendAndGetResult(request);
        }

        Task<JobStatistics> IConsumer.JobStatisticsAsync(int id)
        {
            var request = new JobStatisticsRequest(id);
            return SendAndGetResult(request);
        }

        Task<TubeStatistics> IConsumer.TubeStatisticsAsync(string tube)
        {
            return ((IProducer)this).TubeStatisticsAsync(tube);
        }

        #endregion

        async Task<T> SendAndGetResult<T>(Request<T> request)
        {
            await _connection.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return await request.Task.ConfigureAwait(false);
        }
    }
}
