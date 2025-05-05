using System;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config)
{
   public async Task<List<Item>> GetItemsForSearchDb()
   {
      var lastUpdated = await DB.Find<Item, string>()
         .Sort(x => x.Descending(a => a.UpdatedAt))
         .Project(x => x.UpdatedAt.ToString())
         .ExecuteFirstAsync();

      return await httpClient.GetFromJsonAsync<List<Item>>(config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated);
   }
}
