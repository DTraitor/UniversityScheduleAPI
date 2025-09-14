using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models;

public class PersistentData
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}