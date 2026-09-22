namespace RepairSystem.Model;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Module { get; set; } = null!; // e.g., "Users", "Inventory", "Repair"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
