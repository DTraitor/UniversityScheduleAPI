namespace BusinessLogic.DTO;

public class UserDtoInput
{
    public long TelegramId { get; set; }
    public string GroupName { get; set; }
    public int SubGroup { get; set; } = -1;
}