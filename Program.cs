using MesssageBroker.Data;
using MesssageBroker.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt=> opt.UseSqlite("Data Source=MessageBroker.db"));


var app = builder.Build();



app.UseHttpsRedirection();


// Create Topic
app.MapPost("api/topics", async(AppDbContext context, Topic topic) =>{
    await context.Topics.AddAsync(topic);
    await context.SaveChangesAsync();
    return Results.Created($"api/topics/{topic.Id}", topic);
});


// ReturnallTopics
app.MapGet("api/topics", async(AppDbContext context) =>{
    var topics= await context.Topics.ToListAsync();
    return Results.Ok(topics) ;
});

//Publish Message
app.MapPost("api/topics/{id}/messages", async(AppDbContext context, int id, Message message) =>{
    bool topics = await context.Topics.AnyAsync(t => t.Id ==id);
    if(!topics)
    return Results.NotFound("TopicNotFound");
   
    var subs = context.Subscriptions.Where(s =>s.TpoicId == id);
    
    if (subs.Count()==0)
    return Results.NotFound("There are no subscriptons for this topic");
   
    foreach( var sub in subs)
    {
        Message msg = new Message{
            TopicMessage = message.TopicMessage,
            SubscriptionId =sub.Id,
            ExpiresAfter = message.ExpiresAfter,
            MessageStatus = message.MessageStatus
        };
        await context.Messages.AddAsync(msg);
        
    }
    await context.SaveChangesAsync();
    return Results.Ok("Message has been published");
});


// Create Subscriptions
app.MapPost("api/topics/{id}/subscriptions", async(AppDbContext context, int id, Subscription sub) =>{
    bool topics = await context.Topics.AnyAsync(t =>t.Id == id);
    if(!topics)
    return Results.NotFound("Topics not found");

    sub.TpoicId =id;

    await context.Subscriptions.AddAsync(sub);
    await context.SaveChangesAsync();

    return Results.Created($"api/topics/{id}/subscriptions/{sub.Id}", sub);
});


/// GEt Subcriber messages
app.MapGet("api/subscriptions/{id}/messages", async (AppDbContext context, int id) =>{
    bool subs = await context.Subscriptions.AnyAsync( s => s.Id == id);
    if(!subs)
    return Results.NotFound("Subscriptions not found");
    var messages =  context.Messages.Where(m =>m.SubscriptionId == id && m.MessageStatus !="SENT");
    if (messages.Count() == 0)
    return Results.NotFound("No new messages");
    foreach( var msg in messages)
    {

    }
});


/// Ackmessage for subscriber
app.MapPost("api/subscriptions/{id}/message", async(AppDbContext context,int id, int[] confs) =>{
    bool subs =await context.Subscriptions.AnyAsync( s => s.Id == id);
    
    if(!subs)
    return Results.NotFound("Subscription notfound");

    if(confs.Length <=0) return Results.BadRequest();
    int count =0;
    foreach (int item in confs)
    {
        var msg = context.Messages.FirstOrDefault(m => m.Id == item);

        if (msg != null) 
        {
            msg.MessageStatus = "SENT";
            await context.SaveChangesAsync();
            count++;
        }
    }
    return Results.Ok($"Acknoledged {count}/{confs.Length} messages");
{

}
});
app.Run();


