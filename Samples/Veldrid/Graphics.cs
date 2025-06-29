// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeldridSample;
internal static class Graphics
{
    [global::System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "GetDpiForWindow", ExactSpelling = true)]
    public static extern int GetDpiForWindow(nint hwnd);
}
