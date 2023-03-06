using WebChatBack.Data;
using Microsoft.EntityFrameworkCore;
using WebChatBack.Services;

var builder = WebApplication.CreateBuilder();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ChatContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionStr")));

builder.Services.AddScoped<ChatService, ChatService>();

builder.Services.AddSignalR();

var app = builder.Build();

app.UseWebSockets();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseCors(builder => builder
   .SetIsOriginAllowed(_ => true)
   .AllowAnyMethod()
   .AllowAnyHeader()
   .AllowCredentials());

app.UseHttpsRedirection();



app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();



app.Run();