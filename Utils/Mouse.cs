using SharpDX;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using RareBeasts;

namespace RareBeasts.Utils;

public class Mouse
{
    private readonly BeastsSettings _settings;

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
        SetCursorPos((int)pos.X, (int)pos.Y);
        Thread.Sleep(_settings.DSettings.MouseMoveDelay);
    }

    public void LeftDown(int delay)
    {
        mouse_event((int)MouseEvents.LeftDown, 0, 0, 0, 0);
        Thread.Sleep(_settings.DSettings.MouseClickDelay.Value + delay);
    }

    public void LeftUp(int delay)
    {
        mouse_event((int)MouseEvents.LeftUp, 0, 0, 0, 0);
        Thread.Sleep(_settings.DSettings.MouseClickDelay.Value + delay);
    }

    public void RightDown(int delay)
    {
        mouse_event((int)MouseEvents.RightDown, 0, 0, 0, 0);
        Thread.Sleep(_settings.DSettings.MouseClickDelay.Value + delay);
    }

    public void RightUp(int delay)
    {
        mouse_event((int)MouseEvents.RightUp, 0, 0, 0, 0);
        Thread.Sleep(_settings.DSettings.MouseClickDelay.Value + delay);
    }

    public void MouseMove(SharpDX.Vector2 position)
    {
        float targetX = position.X;
        float targetY = position.Y;
        Random random = new Random();

        // Получаем текущие координаты мыши
        float currentX = GetCursorPosition().X;
        float currentY = GetCursorPosition().Y;

        float steps = _settings.DSettings.MouseStep.Value;

        for (int i = 0; i <= steps; i++)
        {
            float newX = currentX + (targetX - currentX) * i / steps;
            float newY = currentY + (targetY - currentY) * i / steps;

            MoveMouse(new SharpDX.Vector2(newX, newY));

            Thread.Sleep(random.Next(_settings.DSettings.MouseStepDelayMin.Value, _settings.DSettings.MouseStepDelayMin.Value + _settings.DSettings.MouseStepDelayMax.Value));
        }
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

            Thread.Sleep(random.Next(_settings.DSettings.MouseStepDelayMin.Value, _settings.DSettings.MouseStepDelayMin.Value + _settings.DSettings.MouseStepDelayMax.Value));
        }
        SetCursorPos((int)endPos.X, (int)endPos.Y);
    }
}