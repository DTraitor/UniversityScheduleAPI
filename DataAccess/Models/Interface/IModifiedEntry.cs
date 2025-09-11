using DataAccess.Enums;

namespace DataAccess.Models.Interface;

public interface IModifiedEntry
{
    int Id { get; }
    int Key { get; }
}