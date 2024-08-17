using SharpDX;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using RareBeasts;

namespace RareBeasts.Utils;

public class Mouse
{
    private readonly BeastsSettings _settings;
    public bool mouseIsBusy = false;

    public Mouse(BeastsSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public enum MouseEvents
    {
        LeftDown = 0x00000002,
        LeftUp = 0x00000004,
        MiddleDown = 0x00000020,
        MiddleUp = 0x00000040,
        Move = 0x00000001,
        Absolute = 0x00008000,
        RightDown = 0x00000008,
        RightUp = 0x00000010
    }

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out SharpDX.Point lpPoint);

    public SharpDX.Point GetCursorPosition()
    {
        SharpDX.Point lpPoint;
        GetCursorPos(out lpPoint);

        return lpPoint;
    }

    [DllImport("user32.dll")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

    public void MoveMouse(Vector2 pos)
    {
        mouseIsBusy = true;
        SetCursorPos((int)pos.X, (int)pos.Y);
        Thread.Sleep(_settings.MouseSettings.MouseMoveDelay);
        mouseIsBusy = false;
    }

    public void LeftDown(int delay)
    {
        mouseIsBusy = true;
        mouse_event((int)MouseEvents.LeftDown, 0, 0, 0, 0);
        Thread.Sleep(_settings.MouseSettings.MouseClickDelay.Value + delay);
        mouseIsBusy = false;
    }

    public void LeftUp(int delay)
    {
        mouseIsBusy = true;
        mouse_event((int)MouseEvents.LeftUp, 0, 0, 0, 0);
        Thread.Sleep(_settings.MouseSettings.MouseClickDelay.Value + delay);
        mouseIsBusy = false;
    }

    public void RightDown(int delay)
    {
        mouseIsBusy = true;
        mouse_event((int)MouseEvents.RightDown, 0, 0, 0, 0);
        Thread.Sleep(_settings.MouseSettings.MouseClickDelay.Value + delay);
        mouseIsBusy = false;
    }

    public void RightUp(int delay)
    {
        mouseIsBusy = true;
        mouse_event((int)MouseEvents.RightUp, 0, 0, 0, 0);
        Thread.Sleep(_settings.MouseSettings.MouseClickDelay.Value + delay);
        mouseIsBusy = false;
    }

    public void MouseMove(SharpDX.Vector2 position)
    {
        mouseIsBusy = true;
        float targetX = position.X;
        float targetY = position.Y;
        Random random = new Random();

        // Получаем текущие координаты мыши
        float currentX = GetCursorPosition().X;
        float currentY = GetCursorPosition().Y;

        float steps = _settings.MouseSettings.MouseStep.Value;

        for (int i = 0; i <= steps; i++)
        {
            float newX = currentX + (targetX - currentX) * i / steps;
            float newY = currentY + (targetY - currentY) * i / steps;

            MoveMouse(new SharpDX.Vector2(newX, newY));

            Thread.Sleep(random.Next(_settings.MouseSettings.MouseStepDelayMin.Value, _settings.MouseSettings.MouseStepDelayMin.Value + _settings.MouseSettings.MouseStepDelayMax.Value));
        }
        mouseIsBusy = false;
    }

    public void MouseMoveNonLinear(Vector2 endPos)
    {
        Random random = new Random();
        float radius = random.Next(50,80);

        float currentX = GetCursorPosition().X;
        float currentY = GetCursorPosition().Y;
        Vector2 startPos = new Vector2(currentX, currentY);

        float centerX = (startPos.X + endPos.X) / 2;
        float centerY = (startPos.Y + endPos.Y) / 2;



        Vector2 controlPoint = new Vector2(centerX, centerY + radius);

        int steps = 10;
        float stepSize = 1.0f / steps;

        for (int i = 0; i <= steps; i++)
        {
            float t = i * stepSize;
            float oneMinusT = 1 - t;

            float x = oneMinusT * (oneMinusT * startPos.X + t * controlPoint.X) + t * (oneMinusT * controlPoint.X + t * endPos.X);
            float y = oneMinusT * (oneMinusT * startPos.Y + t * controlPoint.Y) + t * (oneMinusT * controlPoint.Y + t * endPos.Y);

            MoveMouse(new Vector2(x, y));

            Thread.Sleep(random.Next(_settings.MouseSettings.MouseStepDelayMin.Value, _settings.MouseSettings.MouseStepDelayMin.Value + _settings.MouseSettings.MouseStepDelayMax.Value));
        }
    }
}