namespace DataAccess.Models;

public class User
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public int? GroupId { get; set; }
    public List<int> ElectedLessonIds { get; set; } = [];
}