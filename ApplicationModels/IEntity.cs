using System.ComponentModel.DataAnnotations;

namespace ApplicationModels;

public interface IEntity
{
    [Key, Required]
    public int Id { get; set; }
}