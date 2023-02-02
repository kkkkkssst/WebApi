using System.Text.RegularExpressions;

List<Item> items = new List<Item>
{
    new(){Id = Guid.NewGuid().ToString(),Name = "Banana", Price = (decimal)2.75},
    new(){Id = Guid.NewGuid().ToString(),Name = "Apple", Price = (decimal)1.58},
    new(){Id = Guid.NewGuid().ToString(),Name = "Bread", Price = 3},
    
};
WebApplicationBuilder builder = WebApplication.CreateBuilder();
WebApplication application = builder.Build();
application.Run(async (context) =>
    {
        var response = context.Response;
        var request = context.Request;
        var path = request.Path;
        string expressionForGuid = @"^/api/items/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
        if (path == "/api/items" && request.Method == "GET")
        {
            await GetAllItems(response);
        }
        else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
        {
            string? id = path.Value.Split("/")[3];
            await GetItem(id, response);
        }
        else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
        {
            string? id = path.Value?.Split("/")[3];
            await DeleteItem(id, response);
        }
        else if (path == "/api/items" && request.Method == "PUT")
        {
            await UpdateItem(response, request);
        }
        else if (path == "/api/items" && request.Method == "POST")
        {
            await CreateItem(response, request);
        }
        else
        {
            response.ContentType = "text/html; charset=utf-8";
            await response.SendFileAsync("html/index.html");
        }
    }
);
application.Run();
// get all items
async Task GetAllItems(HttpResponse response)
{
    await response.WriteAsJsonAsync(items);
}

async Task GetItem(string id, HttpResponse response)
{
    //get item by id 
    Item? item = items.FirstOrDefault((u) => u.Id == id);
    if (item != null)
    {
        await response.WriteAsJsonAsync(item);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Not found" });
    }
}

async Task DeleteItem(string id, HttpResponse response)
{
    //get item by id 
    Item? item = items.FirstOrDefault((i) => i.Id == id);
    if (item != null)
    {
        items.Remove(item);
        await response.WriteAsJsonAsync(item);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Not found" });
    }
}

async Task UpdateItem(HttpResponse response, HttpRequest request)
{
    try
    {
        Item? itemData = await request.ReadFromJsonAsync<Item>();
        if (itemData != null)
        {
            var item = items.FirstOrDefault((i) => i.Id == itemData.Id);
            if (item != null)
            {
                item.Price = itemData.Price;
                item.Name = itemData.Name;
                await response.WriteAsJsonAsync(item);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Not found" });
            }
        }
        else
        {
            throw new Exception("Invalid data");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Invalid data" });
    }
}
async Task CreateItem(HttpResponse response, HttpRequest request)
{
    try
    {
        var item = await request.ReadFromJsonAsync<Item>();
        if (item != null)
        {
            item.Id = Guid.NewGuid().ToString();
            items.Add(item);
            await response.WriteAsJsonAsync(item);
        }
    }
    catch (Exception)
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Invalid data" });
    }
   
}
public class Item
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}