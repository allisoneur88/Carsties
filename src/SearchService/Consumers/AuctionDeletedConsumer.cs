using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer(IMapper mapper) : IConsumer<AuctionDeleted>
{
   public async Task Consume(ConsumeContext<AuctionDeleted> context)
   {
      Console.WriteLine("--> Consuming auction deleted: " + context.Message.Id);

      var item = mapper.Map<Item>(context.Message);

      var result = await item.DeleteAsync();

      if (!result.IsAcknowledged)
         throw new MessageException(typeof(AuctionDeleted), "Problem deleting auction from MongoDB");
   }
}
