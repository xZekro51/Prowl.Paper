// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Veldrid;
using Veldrid.Sdl2;
using Prowl.PaperUI;
using Prowl.Vector;

namespace VeldridSample;
internal class VeldridGUIInput
{
    private readonly Sdl2Window _window;

    public VeldridGUIInput(Sdl2Window window)
    {
        _window = window;
    }

    public void UpdateInput(InputSnapshot snapshot)
    {
        // Handle mouse position and movement
        Vector2 mousePos = snapshot.MousePosition;
        Paper.SetPointerState(PaperMouseBtn.Unknown, (int)mousePos.x, (int)mousePos.y, false, true);

        // Handle mouse buttons
        if (snapshot.IsMouseDown(MouseButton.Left))
            Paper.SetPointerState(PaperMouseBtn.Left, (int)mousePos.x, (int)mousePos.y, true, false);
        else
            Paper.SetPointerState(PaperMouseBtn.Left, (int)mousePos.x, (int)mousePos.y, false, false);

        if (snapshot.IsMouseDown(MouseButton.Right))
            Paper.SetPointerState(PaperMouseBtn.Right, (int)mousePos.x, (int)mousePos.y, true, false);
        else
            Paper.SetPointerState(PaperMouseBtn.Right, (int)mousePos.x, (int)mousePos.y, false, false);

        if (snapshot.IsMouseDown(MouseButton.Middle))
            Paper.SetPointerState(PaperMouseBtn.Middle, (int)mousePos.x, (int)mousePos.y, true, false);
        else
            Paper.SetPointerState(PaperMouseBtn.Middle, (int)mousePos.x, (int)mousePos.y, false, false);

        // Handle mouse wheel
        float wheelDelta = snapshot.WheelDelta;
        if (wheelDelta != 0)
            Paper.SetPointerWheel(wheelDelta);

            // Handle text input
            foreach (char c in snapshot.KeyCharPresses)
            {
                Paper.AddInputCharacter(c.ToString());
            }

            // Handle key states
            foreach (var keyEvent in snapshot.KeyEvents)
            {
                PaperKey paperKey = MapVeldridKeyToPaperKey(keyEvent.Key);
                Paper.SetKeyState(paperKey, keyEvent.Down);
            }
        }

        private static PaperKey MapVeldridKeyToPaperKey(Key key)
        {
            return key switch
            {
                // Letters
                Key.A => PaperKey.A,
                Key.B => PaperKey.B,
                Key.C => PaperKey.C,
                Key.D => PaperKey.D,
                Key.E => PaperKey.E,
                Key.F => PaperKey.F,
                Key.G => PaperKey.G,
                Key.H => PaperKey.H,
                Key.I => PaperKey.I,
                Key.J => PaperKey.J,
                Key.K => PaperKey.K,
                Key.L => PaperKey.L,
                Key.M => PaperKey.M,
                Key.N => PaperKey.N,
                Key.O => PaperKey.O,
                Key.P => PaperKey.P,
                Key.Q => PaperKey.Q,
                Key.R => PaperKey.R,
                Key.S => PaperKey.S,
                Key.T => PaperKey.T,
                Key.U => PaperKey.U,
                Key.V => PaperKey.V,
                Key.W => PaperKey.W,
                Key.X => PaperKey.X,
                Key.Y => PaperKey.Y,
                Key.Z => PaperKey.Z,

                // Numbers
                Key.Number0 => PaperKey.Num0,
                Key.Number1 => PaperKey.Num1,
                Key.Number2 => PaperKey.Num2,
                Key.Number3 => PaperKey.Num3,
                Key.Number4 => PaperKey.Num4,
                Key.Number5 => PaperKey.Num5,
                Key.Number6 => PaperKey.Num6,
                Key.Number7 => PaperKey.Num7,
                Key.Number8 => PaperKey.Num8,
                Key.Number9 => PaperKey.Num9,

                // Function keys
                Key.F1 => PaperKey.F1,
                Key.F2 => PaperKey.F2,
                Key.F3 => PaperKey.F3,
                Key.F4 => PaperKey.F4,
                Key.F5 => PaperKey.F5,
                Key.F6 => PaperKey.F6,
                Key.F7 => PaperKey.F7,
                Key.F8 => PaperKey.F8,
                Key.F9 => PaperKey.F9,
                Key.F10 => PaperKey.F10,
                Key.F11 => PaperKey.F11,
                Key.F12 => PaperKey.F12,

                // Special keys
                Key.Enter => PaperKey.Enter,
                Key.Escape => PaperKey.Escape,
                Key.BackSpace => PaperKey.Backspace,
                Key.Tab => PaperKey.Tab,
                Key.Space => PaperKey.Space,
                Key.Minus => PaperKey.Minus,
                Key.Plus => PaperKey.Equals,
                Key.BracketLeft => PaperKey.LeftBracket,
                Key.BracketRight => PaperKey.RightBracket,
                Key.BackSlash => PaperKey.Backslash,
                Key.Semicolon => PaperKey.Semicolon,
                Key.Quote => PaperKey.Apostrophe,
                Key.Grave => PaperKey.Grave,
                Key.Comma => PaperKey.Comma,
                Key.Period => PaperKey.Period,
                Key.Slash => PaperKey.Slash,
                Key.CapsLock => PaperKey.CapsLock,
                Key.PrintScreen => PaperKey.PrintScreen,
                Key.ScrollLock => PaperKey.ScrollLock,
                Key.Pause => PaperKey.Pause,
                Key.Insert => PaperKey.Insert,
                Key.Home => PaperKey.Home,
                Key.PageUp => PaperKey.PageUp,
                Key.Delete => PaperKey.Delete,
                Key.End => PaperKey.End,
                Key.PageDown => PaperKey.PageDown,
                Key.Right => PaperKey.Right,
                Key.Left => PaperKey.Left,
                Key.Down => PaperKey.Down,
                Key.Up => PaperKey.Up,

                // Keypad
                Key.NumLock => PaperKey.NumLock,
                Key.KeypadDivide => PaperKey.KeypadDivide,
                Key.KeypadMultiply => PaperKey.KeypadMultiply,
                Key.KeypadMinus => PaperKey.KeypadMinus,
                Key.KeypadPlus => PaperKey.KeypadPlus,
                Key.KeypadEnter => PaperKey.KeypadEnter,
                //Key.KeypadEquals => PaperKey.KeypadEquals,
                Key.Keypad1 => PaperKey.Keypad1,
                Key.Keypad2 => PaperKey.Keypad2,
                Key.Keypad3 => PaperKey.Keypad3,
                Key.Keypad4 => PaperKey.Keypad4,
                Key.Keypad5 => PaperKey.Keypad5,
                Key.Keypad6 => PaperKey.Keypad6,
                Key.Keypad7 => PaperKey.Keypad7,
                Key.Keypad8 => PaperKey.Keypad8,
                Key.Keypad9 => PaperKey.Keypad9,
                Key.Keypad0 => PaperKey.Keypad0,
                Key.KeypadDecimal => PaperKey.KeypadDecimal,

                // Modifier keys
                Key.ControlLeft => PaperKey.LeftControl,
                Key.ShiftLeft => PaperKey.LeftShift,
                Key.AltLeft => PaperKey.LeftAlt,

                _ => PaperKey.Unknown
            };
        }
}
