using BusinessLogic.Enums;

namespace BusinessLogic.DTO;

public class UserDtoOutput
{
    public int Id { get; set; }
    public int? GroupId { get; set; }
    public ElectiveStatusEnum ElectiveStatus { get; set; }
}