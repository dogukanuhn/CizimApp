using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CizimAppData.Helpers;
using CizimAppData.Repository;
using CizimAppEntity.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ConnectedUserWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        IServiceProvider _serviceProvider;
        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _redisHandler = scope.ServiceProvider.GetService<IRedisHandler>();
                    if (await _redisHandler.IsCached("Userlist:ConnectedUser"))
                    {
                        var cachedData = JsonConvert.DeserializeObject<List<ConnectedUser>>(await _redisHandler.GetFromCache("Userlist:ConnectedUser"));
                        if (cachedData != null)
                        {
                            var _connectedUserRepository = scope.ServiceProvider.GetService<IConnectedUserRepository>();


                            foreach (var cachedItem in cachedData)
                            {
                                var data = await _connectedUserRepository.FirstOrDefault(x => x.Id == cachedItem.Id);

                                if (data == null)
                                {
                                    await _connectedUserRepository.Add(cachedItem);
                                    _logger.LogInformation("ConnectedUser Added To Db Loop", DateTimeOffset.Now);
                                }else
                                {
                                    if (cachedItem.ConnectedRoomName != data.ConnectedRoomName)
                                    {
                                        data.ConnectedRoomName = cachedItem.ConnectedRoomName;
                                        await _connectedUserRepository.Update(data);
                                        _logger.LogInformation("ConnectedUser Updated To Db Roomname", DateTimeOffset.Now);
                                    }

                                    if (cachedItem.ConnectionId != data.ConnectionId)
                                    {
                                        data.ConnectionId = cachedItem.ConnectionId;
                                        await _connectedUserRepository.Update(data);
                                        _logger.LogInformation("ConnectedUser Updated To Db ConnectionId", DateTimeOffset.Now);
                                    }
                                }   

                            }
                        }
                    }
                    else
                    {
                        var _connectedUserRepository = scope.ServiceProvider.GetService<IConnectedUserRepository>();
                        var dbData = await _connectedUserRepository.GetAll();
                        if (dbData.Count() > 0)
                        {
                            await _redisHandler.AddToCache("Userlist:ConnectedUser", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(dbData));
                            _logger.LogInformation("Connected User Added To Cache", DateTimeOffset.Now);
                        }


                    }


                }


                _logger.LogInformation("Connected User Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(150000, stoppingToken);
            }
        }
    }
}
