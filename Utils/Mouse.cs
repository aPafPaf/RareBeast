using System;
using System.Runtime.InteropServices;
using System.Threading;
using Point = SharpDX.Point;
using Vector2 = SharpDX.Vector2;
using Vector3 = SharpDX.Vector3;

namespace RareBeasts.Utils
{
    public static class Mouse
    {
        public static bool IsEnabled = true; // Флаг для отключения работы функций

        public static class Constants
        {
            public const int WHILE_DELAY = 100;
            public const int CLICK_DELAY = 50;
            public const int INPUT_DELAY = 15;
            public const int CANT_BE_REACHED = -99999;
        }

        public enum MouseEvents
        {
            Move = 0x00000001,
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            RightDown = 0x00000008,
            RightUp = 0x00000010,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Absolute = 0x00008000
        }

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out SharpDX.Point lpPoint);

        public static Point GetCursorPosition()
        {
            if (!IsEnabled) return new Point(0, 0);

            GetCursorPos(out Point lpPoint);
            return lpPoint;
        }

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public static void MoveMouse(Vector2 pos)
        {
            if (!IsEnabled) return;  // Проверка флага

            SetCursorPos((int)pos.X, (int)pos.Y);
            Thread.Sleep(Constants.CLICK_DELAY);
        }

        public static void LeftDown(int delay)
        {
            if (!IsEnabled) return;  // Проверка флага

            mouse_event((int)MouseEvents.LeftDown, 0, 0, 0, 0);
            Thread.Sleep(Constants.CLICK_DELAY + delay);
        }

        public static void LeftUp(int delay)
        {
            if (!IsEnabled) return;  // Проверка флага

            mouse_event((int)MouseEvents.LeftUp, 0, 0, 0, 0);
            Thread.Sleep(Constants.CLICK_DELAY + delay);
        }

        public static void RightDown(int delay)
        {
            if (!IsEnabled) return;  // Проверка флага

            mouse_event((int)MouseEvents.RightDown, 0, 0, 0, 0);
            Thread.Sleep(Constants.CLICK_DELAY + delay);
        }

        public static void RightUp(int delay)
        {
            if (!IsEnabled) return;  // Проверка флага

            mouse_event((int)MouseEvents.RightUp, 0, 0, 0, 0);
            Thread.Sleep(Constants.CLICK_DELAY + delay);
        }

        public static void MouseMove(SharpDX.Vector2 position)
        {
            if (!IsEnabled) return;  // Проверка флага

            float targetX = position.X;
            float targetY = position.Y;
            Random random = new();

            float currentX = GetCursorPosition().X;
            float currentY = GetCursorPosition().Y;

            float steps = 1;

            for (int i = 0; i <= steps; i++)
            {
                float newX = currentX + ((targetX - currentX) * i / steps);
                float newY = currentY + ((targetY - currentY) * i / steps);

                MoveMouse(new SharpDX.Vector2(newX, newY));
                //Thread.Sleep(random.Next(0 , 1));
            }
        }

        public static void MouseMoveNonLinear(Vector2 endPos)
        {
            if (!IsEnabled) return;  // Проверка флага

            Random random = new();
            float radius = random.Next(50, 80);

            float currentX = GetCursorPosition().X;
            float currentY = GetCursorPosition().Y;
            Vector2 startPos = new(currentX, currentY);

            float centerX = (startPos.X + endPos.X) / 2;
            float centerY = (startPos.Y + endPos.Y) / 2;

            Vector2 controlPoint = new(centerX, centerY + radius);

            int steps = 2;
            float stepSize = 1.0f / steps;

            for (int i = 0; i <= steps; i++)
            {
                float t = i * stepSize;
                float oneMinusT = 1 - t;

                float x = (oneMinusT * ((oneMinusT * startPos.X) + (t * controlPoint.X))) + (t * ((oneMinusT * controlPoint.X) + (t * endPos.X)));
                float y = (oneMinusT * ((oneMinusT * startPos.Y) + (t * controlPoint.Y))) + (t * ((oneMinusT * controlPoint.Y) + (t * endPos.Y)));

                MoveMouse(new Vector2(x, y));
                //Thread.Sleep(random.Next(0, 1));
            }
        }
    }
}
