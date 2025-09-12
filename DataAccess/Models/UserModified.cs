using DataAccess.Enums;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class UserModified : IModifiedEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ProcessedByEnum ToProcess { get; set; }

    public int Key => UserId;
}