using System.ComponentModel.DataAnnotations;

namespace ApplicationModels;

public class Entity
{
    [Key, Required]
    public int Id { get; set; }
}