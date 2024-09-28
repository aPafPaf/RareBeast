using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RectangleF = SharpDX.RectangleF;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;

namespace RareBeasts;

public partial class Beasts
{
    public bool OpenStash()
    {
        if (!StashIsOpen())
        {
            var stash = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Stash];
            if (stash.Count > 0)
            {
                var stashEntity = stash[0];

                if (WorldToValidScreenPositionBool(stashEntity.Pos))
                {
                    if (TargetEntity(stashEntity, "Stash"))
                    {
                        Utils.Mouse.LeftDown(1);
                        Utils.Mouse.LeftUp(1);
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            return true;
        }
        return false;
    }

    public bool TargetEntity(Entity entity, string TextNoTags)
    {
        if (GameController.IngameState.IngameUi.HighlightedElement != null && GameController.IngameState.IngameUi.HighlightedElement.TextNoTags == TextNoTags)
        {
            return true;
        }

        if (entity.TryGetComponent(out Render entityRender))
        {
            Utils.Mouse.MoveMouse(WorldToValidScreenPosition(entityRender.InteractCenter));
        }

        return false;
    }

    public bool StashIsOpen()
    {
        return GameController.IngameState.IngameUi.StashElement.IsVisible;
    }

    public bool InventoryIsOpen()
    {
        return GameController.IngameState.IngameUi.InventoryPanel.IsVisible;
    }

    private bool WorldToValidScreenPositionBool(Vector3 worldPos)
    {
        var windowRect = GameController.Window.GetWindowRectangle();
        var screenPos = GameController.IngameState.Camera.WorldToScreen(worldPos);
        var result = screenPos + windowRect.Location;

        var edgeBounds = 50;
        if (!windowRect.Intersects(new SharpDX.RectangleF(result.X, result.Y, edgeBounds, edgeBounds)))
        {
            return false;
        }
        return true;
    }

    private bool WorldToValidScreenPositionBool(Vector2 worldPosV2)
    {
        SharpDX.Vector3 worldPos = new Vector3(worldPosV2.X, worldPosV2.Y, 0);
        var windowRect = GameController.Window.GetWindowRectangle();
        var screenPos = GameController.IngameState.Camera.WorldToScreen(worldPos);
        var result = screenPos + windowRect.Location;

        var edgeBounds = 50;
        if (!windowRect.Intersects(new SharpDX.RectangleF(result.X, result.Y, edgeBounds, edgeBounds)))
        {
            return false;
        }
        return true;
    }

    private Vector2 WorldToValidScreenPosition(Vector3 worldPos)
    {
        var windowRect = GameController.Window.GetWindowRectangle();
        var screenPos = GameController.IngameState.Camera.WorldToScreen(worldPos);
        var result = screenPos + windowRect.Location;

        var edgeBounds = 50;
        if (!windowRect.Intersects(new SharpDX.RectangleF(result.X, result.Y, edgeBounds, edgeBounds)))
        {
            //Adjust for offscreen entity. Need to clamp the screen position using the game window info. 
            if (result.X < windowRect.TopLeft.X) result.X = windowRect.TopLeft.X + edgeBounds;
            if (result.Y < windowRect.TopLeft.Y) result.Y = windowRect.TopLeft.Y + edgeBounds;
            if (result.X > windowRect.BottomRight.X) result.X = windowRect.BottomRight.X - edgeBounds;
            if (result.Y > windowRect.BottomRight.Y) result.Y = windowRect.BottomRight.Y - edgeBounds;
        }
        return result;
    }

    public bool OpenTab(string tabName)
    {
        var inventoryTab = GetInventoryByTabName(tabName);

        var uiStash = GameController.IngameState.IngameUi.StashElement;
        var tabElement = uiStash.FindChildRecursive(tabName);

        if (!uiStash.IsVisible) return false;

        var windowOffset = GameController.Window.GetWindowRectangle().Location;
        Utils.Mouse.MoveMouse(tabElement.GetClientRectCache.Center + windowOffset);

        Utils.Mouse.LeftDown(1);
        Utils.Mouse.LeftUp(1);


        return true;
    }
    public Inventory GetInventoryByTabName(string tabName)
    {
        var visibleIndex = GameController.IngameState.ServerData.PlayerStashTabs.First(x => x.Name == tabName).VisibleIndex;
        var inventoryStashTab = GameController.IngameState.IngameUi.StashElement.GetStashInventoryByIndex(visibleIndex);

        return inventoryStashTab;
    }

    public bool AllItemsToStash()
    {
        if (!StashIsOpen())
        {
            OpenStash();
        }

        if (!InventoryIsOpen())
        {
            Utils.Keyboard.KeyPress(Keys.I);
        }

        if (!OpenTab(Settings.BeastTabName.Value)) return false;

        var windowOffset = GameController.Window.GetWindowRectangle().Location;

        Utils.Keyboard.KeyDown(Keys.ControlKey);

        foreach (var item in playerInventory)
        {
            if (item.Full)
            {
                Utils.Mouse.MoveMouse(item.Rect.Center + windowOffset);
                Utils.Mouse.LeftDown(1);
                Utils.Mouse.LeftUp(1);
            }
            if (Input.GetKeyState(Settings.StopHotKey.Value))
            {
                Settings.WorkGrabber.Value = false;
                Settings.WorkStasher.Value = false;

                Utils.Keyboard.KeyUp(Keys.ControlKey);
                break;
            }
        }
        Utils.Keyboard.KeyUp(Keys.ControlKey);

        return true;
    }

    public void UpdateInventoryPlayer()
    {
        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        // track each inventory slot
        SlotInventory[,] inventorySlots = new SlotInventory[12, 5];

        //get pos inventory slots
        var inventoryRect = GameController.IngameState.IngameUi.GetChildFromIndices(37, 3, 25).GetClientRectCache;
        var invSlotW = inventoryRect.Width / 12;
        var invSlotH = inventoryRect.Height / 5;

        float offsetX = inventoryRect.X;
        float offsetY = inventoryRect.Y;

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                RectangleF rectSlot = new(offsetX, offsetY, invSlotW, invSlotH);

                inventorySlots[x, y] = new SlotInventory(false, rectSlot, "");

                offsetY += invSlotW;
            }
            offsetY = inventoryRect.Y;
            offsetX += invSlotH;
        }

        // iterate through each item in the inventory and mark used slots
        foreach (var inventoryItem in inventoryItems)
        {
            int x = inventoryItem.PosX;
            int y = inventoryItem.PosY;
            int height = inventoryItem.SizeY;
            int width = inventoryItem.SizeX;
            for (int row = x; row < x + width; row++)
            {
                for (int col = y; col < y + height; col++)
                {
                    if (inventoryItem.Item.TryGetComponent(out Base itemBase))
                    {
                        inventorySlots[row, col] = new SlotInventory(true, inventoryItem.GetClientRect(), itemBase.Name);
                    }
                    else
                    {
                        inventorySlots[row, col] = new SlotInventory(true, inventoryItem.GetClientRect(), "");
                    }
                }
            }
        }

        playerInventory = inventorySlots;
    }

    public bool TakeConsumablesFromTab(string tabName)
    {
        string bsOrb = "Bestiary Orb";

        UpdateInventoryTab(tabName);

        var inventoryTab = GetInventoryByTabName(tabName);

        if (inventoryTab.VisibleInventoryItems.Count == 0) return false;

        if (!StashIsOpen())
            OpenStash();

        if (!OpenTab(tabName)) return false;

        List<SlotInventory> consublesInTab = [];

        stashTabs.TryGetValue(tabName, out List<SlotInventory> stashTab);

        consublesInTab = stashTab.Where(x => x.Name == bsOrb).ToList();

        int iterationMax = 6;
        int i = 0;

        foreach (var item in consublesInTab)
        {
            UpdateInventoryTab(tabName);

            MoveItem(item);
            i++;

            if (i >= iterationMax) break;

        }

        return true;
    }

    public bool UpdateInventoryTab(string tabName)
    {
        var inventoryStashTab = GetInventoryByTabName(tabName);

        if (inventoryStashTab == null)
        {
            LogMessage("Need Open Tab");
            OpenStash();
            var visibleIndex = GameController.IngameState.ServerData.PlayerStashTabs.First(x => x.Name == tabName).VisibleIndex;
            var uiStash = GameController.IngameState.IngameUi.StashElement;
            var tabElement = uiStash.FindChildRecursive(tabName);

            if (!uiStash.IsVisible) return false;

            var windowOffset = GameController.Window.GetWindowRectangle().Location;
            Utils.Mouse.MoveMouse(tabElement.GetClientRectCache.Center + windowOffset);

            Utils.Mouse.LeftDown(1);
            Utils.Mouse.LeftUp(1);

            return false;
        }

        List<SlotInventory> tab = [];

        foreach (var item in inventoryStashTab.VisibleInventoryItems)
        {
            tab.Add(new()
            {
                Full = true,
                Rect = item.GetClientRectCache,
                Name = item.Item.GetComponent<Base>().Name
            });
        }

        stashTabs[tabName] = tab;

        return true;
    }

    void MoveItem(SlotInventory item)
    {
        var windowOffset = GameController.Window.GetWindowRectangle().Location;

        Utils.Keyboard.KeyDown(Keys.ControlKey);

        Utils.Mouse.MoveMouse(item.Rect.Center + windowOffset);
        Utils.Mouse.LeftDown(1);
        Utils.Mouse.LeftUp(1);

        Utils.Keyboard.KeyUp(Keys.ControlKey);
    }

    public Element FindChildRecursiveLocal(Element elem, string text, bool contains = false)
    {
        if (elem.Text == text || (contains && (elem.Text?.Contains(text) ?? false)))
        {
            return elem;
        }

        foreach (var child in elem.Children)
        {
            Element result = FindChildRecursiveLocal(child, text, contains);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}

struct SlotInventory
{
    public bool Full { get; set; }
    public SharpDX.RectangleF Rect { get; set; }
    public string Name { get; set; }

    public SlotInventory(bool full, SharpDX.RectangleF rect, string name)
    {
        this.Full = full;
        this.Rect = rect;
        this.Name = name;
    }
}
