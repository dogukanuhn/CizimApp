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
using StackExchange.Redis;

namespace RoomWorker
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
                    if (await _redisHandler.IsCached("Room:Rooms"))
                    {
                        var cachedData = JsonConvert.DeserializeObject<List<Room>>(await _redisHandler.GetFromCache("Room:Rooms"));
                        if (cachedData != null)
                        {
                            var _roomRepository = scope.ServiceProvider.GetService<IRoomRepository>();
                            foreach (var cachedItem in cachedData)
                            {
                                var data = await _roomRepository.FirstOrDefault(x => x.Id == cachedItem.Id);


                                if (data == null)
                                {
                                    await _roomRepository.Add(cachedItem);
                                    _logger.LogInformation("added db loop", DateTimeOffset.Now);
                                }
                                else
                                {
                                    if (cachedItem.roomUserCount != data.roomUserCount)
                                    {
                                        data.roomUserCount = cachedItem.roomUserCount;
                                        await _roomRepository.Update(data);
                                    }
                                    if (data.roomUserCount == 0)
                                    {
                                        await _roomRepository.Remove(data);
                                    }
                                }



                            }
                        }
                    }
                    else
                    {
                        var _roomRepository = scope.ServiceProvider.GetService<IRoomRepository>();
                        var dbData = await _roomRepository.GetAll();
                        if (dbData.Count() > 0)
                        {
                            await _redisHandler.AddToCache("Room:Rooms", TimeSpan.FromMinutes(2), JsonConvert.SerializeObject(dbData));
                            _logger.LogInformation("db eklendi", DateTimeOffset.Now);
                        }


                    }


                }

                _logger.LogInformation("Room Worker: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
