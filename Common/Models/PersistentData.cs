using Common.Models.Interface;

namespace Common.Models;

public class PersistentData : IEntityId
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}