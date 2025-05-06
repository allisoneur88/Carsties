using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer(IMapper mapper) : IConsumer<AuctionUpdated>
{
   public async Task Consume(ConsumeContext<AuctionUpdated> context)
   {
      Console.WriteLine("--> Consuming auction updated: " + context.Message.Id);

      var item = mapper.Map<Item>(context.Message);

      var result = await DB.Update<Item>()
         .MatchID(item.ID)
         .ModifyOnly(i => new { i.Make, i.Model, i.Year, i.Color, i.Mileage }, item)
         .ExecuteAsync();

      if (!result.IsAcknowledged)
         throw new MessageException(typeof(AuctionUpdated), "Problem updating MongoDB");
   }
}
