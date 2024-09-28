using RareBeasts.Data;
using RareBeasts.ExileCore;
using SharpDX;
using System.Linq;

namespace RareBeasts;

public partial class Beasts
{
    public override void Render()
    {
        DrawBestiaryPanel();

    }

    private void DrawBestiaryPanel()
    {
        var bestiary = GameController.IngameState.IngameUi.GetBestiaryPanel();
        if (bestiary == null || bestiary.IsVisible == false) return;

        var capturedBeastsPanel = bestiary.CapturedBeastsPanel;
        if (capturedBeastsPanel == null || capturedBeastsPanel.IsVisible == false) return;

        var beasts = bestiary.CapturedBeastsPanel.CapturedBeasts;

        var rectCaptureBeast = bestiary.CapturedBeastsPanel.GetChildFromIndices(1).GetClientRectCache;

        if (!beasts.Any()) return;

        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        // track each inventory slot
        InventoryPlayer[,] inventorySlot = new InventoryPlayer[12, 5];

        //get pos inventory slots
        var inventoryRect = GameController.IngameState.IngameUi.GetChildFromIndices(37, 3, 27).GetClientRectCache;
        var invSlotW = inventoryRect.Width / 12;
        var invSlotH = inventoryRect.Height / 5;

        float offsetX = inventoryRect.X;
        float offsetY = inventoryRect.Y;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                RectangleF rectSlot = new RectangleF(offsetX, offsetY, invSlotW, invSlotH);

                Graphics.DrawFrame(rectSlot, Color.White, 2);

                offsetY += invSlotW;
            }
            offsetY = inventoryRect.Y;
            offsetX += invSlotH;
        }

        var beast = beasts.FirstOrDefault();


        var beastRect = beast.GetClientRect().Center;
        if (!rectCaptureBeast.Contains(beastRect)) return;

        var capturedBeast = BeastsDatabase.AllBeasts.Find(b => b.DisplayName == beast.DisplayName);
        if (capturedBeast == null)
        {
            Graphics.DrawFrame(beast.GetClientRect(), Color.White, 2);
            Graphics.DrawFrame(beast.GetChildAtIndex(3).GetClientRect(), Color.White, 2);

        }
        else
        {
            Graphics.DrawFrame(beast.GetClientRect(), Color.Red, 2);
            Graphics.DrawFrame(beast.GetChildAtIndex(3).GetClientRect(), Color.Red, 2);
        }

    }

}