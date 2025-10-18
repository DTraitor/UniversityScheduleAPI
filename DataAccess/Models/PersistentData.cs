using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Models.Interface;

namespace DataAccess.Models;

public class PersistentData : IEntityId
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}