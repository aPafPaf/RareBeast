using System.Drawing;

namespace RareBeasts.Data;

public class Beast
{
    public string Path;
    public string DisplayName;
    public string[] Crafts;
}

struct InventoryPlayer
{
    public bool full { get; set; }
    public SharpDX.RectangleF rect { get; set; }

    public InventoryPlayer(bool full, SharpDX.RectangleF rect)
    {
        this.full = full;
        this.rect = rect;
    }
}