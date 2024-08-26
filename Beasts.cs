using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using RareBeasts.Data;
using RareBeasts.ExileCore;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RareBeasts;

public partial class Beasts : BaseSettingsPlugin<BeastsSettings>
{
    private readonly Dictionary<long, Entity> _trackedBeasts = new();

    private Utils.Mouse mouse;
    private static Random random = new Random();
    private SharpDX.Vector2 windowOffset;

    int startTime = 0;

    public override void OnLoad()
    {
        Name = "RareBeast";
        mouse = new Utils.Mouse(Settings);
    }

    public override bool Initialise()
    {

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;
        return base.Initialise();
    }

    public override Job Tick()
    {
        if (Settings.StartStopHotKey.PressedOnce())
        {
            Settings.Work.Value = !Settings.Work.Value;
            startTime = Environment.TickCount;

            mouse.MouseMoveNonLinear(this.GameController.Window.GetWindowRectangle().Center);

        }

        if (Settings.Work.Value)
        {
            if ((Environment.TickCount - startTime) < Settings.DSettings.BeastDelay) 
                return null;

            Work();
            startTime = Environment.TickCount;
            DestroyWindowCheck();

            if (Input.GetKeyState(Settings.StopHotKey.Value))
            {
                Settings.Work.Value = false;
                return null;
            }
        }

        return null;
    }

    private void Work()
    {
        if (!Settings.Work) return;

        var bestiary = GameController.IngameState.IngameUi.GetBestiaryPanel();
        if (bestiary == null || bestiary.IsVisible == false) return;

        var inventory = GameController.IngameState.IngameUi.InventoryPanel;
        if (inventory == null || inventory.IsVisible == false) return;

        var capturedBeastsPanel = bestiary.CapturedBeastsPanel;
        if (capturedBeastsPanel == null || capturedBeastsPanel.IsVisible == false) return;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        var beasts = bestiary.CapturedBeastsPanel.CapturedBeasts;

        if(!beasts.Any() ) 
        {
            Settings.Work.Value = !Settings.Work.Value;
            return;
        }

        var rectCaptureBeast = bestiary.CapturedBeastsPanel.GetChildFromIndices(1).GetClientRectCache;

        var beast = beasts.FirstOrDefault();

        var beastRect = beast.GetClientRect().Center;
        if (!rectCaptureBeast.Contains(beastRect)) return;

        var releaseButton = beast.GetChildAtIndex(3).GetClientRect();
        var rectBeast = beast.GetClientRect();

        var capturedBeast = BeastsDatabase.AllBeasts.Find(b => b.DisplayName == beast.DisplayName);
        if (capturedBeast == null)
        {

            if (GetBestiaryOrb())
            {
                GrabBeast(rectBeast.Center);
                if (!PlaceBeast())
                {
                    LogMessage("Inventory Error");
                    Settings.Work.Value = !Settings.Work.Value;
                    return;
                }
            }
            else
            {
                LogMessage("Error Grab Orb");
                Settings.Work.Value = !Settings.Work.Value;
                return;
            }
        }
        else
        {
            ReleaseBeast(releaseButton.Center);
            Thread.Sleep(Settings.DSettings.ActionDelay);
        }

    }

    public void DestroyWindowCheck()
    {
        var destroyWindow = GameController.IngameState.IngameUi.DestroyConfirmationWindow;
        if (destroyWindow.IsVisible)
        {
            Thread.Sleep(Settings.DSettings.MouseClickDelay * 3);
            Utils.Keyboard.KeyDown(System.Windows.Forms.Keys.Escape);
            Thread.Sleep(Settings.DSettings.MouseClickDelay);
            Utils.Keyboard.KeyUp(System.Windows.Forms.Keys.Escape);
            Thread.Sleep(Settings.DSettings.MouseClickDelay);
        }
    }

    public bool ReleaseBeast(Vector2 pos)
    {

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        mouse.MouseMoveNonLinear(pos + windowOffset);

        Utils.Keyboard.KeyDown(System.Windows.Forms.Keys.LControlKey);

        mouse.LeftDown(Settings.DSettings.MouseClickDelay);
        mouse.LeftUp(Settings.DSettings.MouseClickDelay);

        Utils.Keyboard.KeyUp(System.Windows.Forms.Keys.LControlKey);

        Thread.Sleep(Settings.DSettings.ActionDelay);

        return true;
    }

    public bool PlaceBeast()
    {
        Vector2 freeSlot = SearchFreeSpace();

        if (freeSlot.IsZero) return false;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        mouse.MouseMoveNonLinear(freeSlot + windowOffset);
        Thread.Sleep(Settings.DSettings.ActionDelay);

        mouse.LeftDown(Settings.DSettings.MouseClickDelay);
        mouse.LeftUp(Settings.DSettings.MouseClickDelay);

        Thread.Sleep(Settings.DSettings.ActionDelay);

        return true;
    }

    private SharpDX.Vector2 SearchFreeSpace()
    {

        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;
        // quick sanity check
        if (inventoryItems.Count > 60)
        {
            return new Vector2(0, 0);
        }

        // track each inventory slot
        InventoryPlayer[,] inventorySlot = new InventoryPlayer[12, 5];

        //get pos inventory slots
        var defRect = inventoryItems.Where(item => item.PosX == 0 && item.PosY == 0).FirstOrDefault();

        var firtItem = inventoryItems.Where(item => item.PosX == 0 && item.PosY == 0).FirstOrDefault();

        float offsetX = firtItem.GetClientRect().TopLeft.X;
        float offsetY = firtItem.GetClientRect().TopLeft.Y;

        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                RectangleF rectSlot = new RectangleF(offsetX, offsetY, defRect.GetClientRect().Width, defRect.GetClientRect().Height);

                inventorySlot[x, y] = new InventoryPlayer(false, rectSlot);

                offsetY += defRect.GetClientRect().Width;
            }
            offsetY = firtItem.GetClientRect().TopLeft.Y;
            offsetX += defRect.GetClientRect().Height;
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
                    inventorySlot[row, col] = new InventoryPlayer(true, inventoryItem.GetClientRect());
                }
            }
        }

        // check for any empty slots
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (inventorySlot[x, y].full == false)
                {
                    return inventorySlot[x, y].rect.Center;
                }
            }
        }


        // no empty slots, so inventory is full
        return new Vector2(0, 0);
    }

    public bool GrabBeast(Vector2 beastPos)
    {
        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        mouse.MouseMoveNonLinear(beastPos + windowOffset);
        Thread.Sleep(Settings.DSettings.ActionDelay);

        mouse.LeftDown(Settings.DSettings.MouseClickDelay);
        mouse.LeftUp(Settings.DSettings.MouseClickDelay);

        Thread.Sleep(Settings.DSettings.ActionDelay);

        return true;
    }

    public bool GetBestiaryOrb()
    {

        string bsOrb = "Metadata/Items/Currency/CurrencyItemiseCapturedMonster";

        var playerInventory =  GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;

        var bestiartOrbs = playerInventory.Where(item => item.Item.Metadata == bsOrb).ToList();

        if (!bestiartOrbs.Any()) return false;

        this.windowOffset = this.GameController.Window.GetWindowRectangle().TopLeft;

        var firstItem = bestiartOrbs.FirstOrDefault();
        if (firstItem == null)
        {
            LogMessage("Could not find item.");
            return false;
        }

        Vector2 itemPos = firstItem.GetClientRect().Center;

        mouse.MouseMoveNonLinear(itemPos + windowOffset);
        Thread.Sleep(Settings.DSettings.ActionDelay);

        var sss = GameController.IngameState.UIHover?.Entity?.Metadata;

        if (Settings.OrbCheck)
        {
            Thread.Sleep(Settings.DSettings.CheckDelay);

            if (GameController.IngameState.UIHover?.Entity?.Metadata == bsOrb)
            {
                mouse.RightDown(Settings.DSettings.MouseClickDelay);
                mouse.RightUp(Settings.DSettings.MouseClickDelay);

                Thread.Sleep(Settings.DSettings.ActionDelay);

                return true;
            }
            else
            {
                LogMessage("Item on cursor");
                return false;
            }

        }
        mouse.RightDown(Settings.DSettings.MouseClickDelay);
        mouse.RightUp(Settings.DSettings.MouseClickDelay);

        Thread.Sleep(Settings.DSettings.ActionDelay);

        return true;
    }

    public override void DrawSettings()
    {
        base.DrawSettings();


    }
}