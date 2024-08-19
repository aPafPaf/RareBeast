using ExileCore.PoEMemory.Components;
using ExileCore.Shared.Enums;
using ImGuiNET;
using RareBeasts.Data;
using RareBeasts.ExileCore;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace RareBeasts;

public partial class Beasts
{
    public override void Render()
    {
        DrawBestiaryPanel();

        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        // track each inventory slot
        InventoryPlayer[,] inventorySlot = new InventoryPlayer[12, 5];

        //get pos inventory slots
        var defRect = inventoryItems.Where(item => item.PosX == 0 && item.PosY == 0).FirstOrDefault();

        var firtItem = inventoryItems.Where(item => item.PosX == 0 && item.PosY == 0).FirstOrDefault();

        float offsetX = firtItem.GetClientRect().TopLeft.X;
        float offsetY = firtItem.GetClientRect().TopLeft.Y;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                RectangleF rectSlot = new RectangleF(offsetX, offsetY, defRect.GetClientRect().Width, defRect.GetClientRect().Height);

                Graphics.DrawFrame(rectSlot, Color.White, 2);

                offsetY += defRect.GetClientRect().Width;
            }
            offsetY = firtItem.GetClientRect().TopLeft.Y;
            offsetX += defRect.GetClientRect().Height;
        }

    }

    private void DrawBestiaryPanel()
    {
        var bestiary = GameController.IngameState.IngameUi.GetBestiaryPanel();
        if (bestiary == null || bestiary.IsVisible == false) return;

        var capturedBeastsPanel = bestiary.CapturedBeastsPanel;
        if (capturedBeastsPanel == null || capturedBeastsPanel.IsVisible == false) return;

        var beasts = bestiary.CapturedBeastsPanel.CapturedBeasts;

        var rectCaptureBeast = bestiary.CapturedBeastsPanel.GetChildFromIndices(1).GetClientRectCache;

        var beast = beasts.FirstOrDefault();

        //foreach (var beast in beasts)
        //{
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
        //}
    }

}