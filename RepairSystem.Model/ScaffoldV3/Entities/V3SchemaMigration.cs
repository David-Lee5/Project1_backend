using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class V3SchemaMigration
{
    public string Version { get; set; } = null!;

    public DateTime AppliedAt { get; set; }
}
