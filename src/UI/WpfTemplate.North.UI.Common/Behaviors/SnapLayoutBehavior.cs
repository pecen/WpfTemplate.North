using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace WpfTemplate.North.UI.Common.Behaviors
{
	/// <summary>
	/// Attached Property för att aktivera Windows 11 Snap Layouts på custom titlebar knappar
	/// Använd i XAML: behaviors:SnapLayoutBehavior.IsMaximizeButton="True"
	/// </summary>
	public static class SnapLayoutBehavior
	{
		private const int WM_NCHITTEST = 0x0084;
		private const int WM_NCLBUTTONDOWN = 0x00A1;
		private const int WM_NCLBUTTONUP = 0x00A2;
		private const int HTMAXBUTTON = 9;
		private const int DWM_WINDOW_CORNER_PREFERENCE = 33;
		private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

		[DllImport("user32.dll")]
		private static extern bool GetCursorPos(out POINT lpPoint);

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;
		}

		private static readonly DependencyPropertyKey IsRegisteredPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly(
				"IsRegistered",
				typeof(bool),
				typeof(SnapLayoutBehavior),
				new PropertyMetadata(false));

		public static readonly DependencyProperty IsRegisteredProperty =
			IsRegisteredPropertyKey.DependencyProperty;

		// Attached Property för maximize-knappen
		public static readonly DependencyProperty IsMaximizeButtonProperty =
			DependencyProperty.RegisterAttached(
				"IsMaximizeButton",
				typeof(bool),
				typeof(SnapLayoutBehavior),
				new PropertyMetadata(false, OnIsMaximizeButtonChanged));

		public static bool GetIsMaximizeButton(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsMaximizeButtonProperty);
		}

		public static void SetIsMaximizeButton(DependencyObject obj, bool value)
		{
			obj.SetValue(IsMaximizeButtonProperty, value);
		}

		private static bool GetIsRegistered(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsRegisteredProperty);
		}

		private static void SetIsRegistered(DependencyObject obj, bool value)
		{
			obj.SetValue(IsRegisteredPropertyKey, value);
		}

		private static void OnIsMaximizeButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is Button button) || !(bool)e.NewValue)
				return;

			Debug.WriteLine("SnapLayoutBehavior: IsMaximizeButton attached to button");

			// Vänta tills knappen är loaded
			if (button.IsLoaded)
			{
				RegisterButton(button);
			}
			else
			{
				button.Loaded += OnButtonLoaded;
			}

			// Cleanup när knappen unloadas
			button.Unloaded += OnButtonUnloaded;
		}

		private static void OnButtonLoaded(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				Debug.WriteLine("SnapLayoutBehavior: Button loaded");
				button.Loaded -= OnButtonLoaded;
				RegisterButton(button);
			}
		}

		private static void OnButtonUnloaded(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				Debug.WriteLine("SnapLayoutBehavior: Button unloaded");
				button.Unloaded -= OnButtonUnloaded;
				UnregisterButton(button);
			}
		}

		private static void RegisterButton(Button button)
		{
			if (GetIsRegistered(button))
			{
				Debug.WriteLine("SnapLayoutBehavior: Button already registered");
				return;
			}

			var window = Window.GetWindow(button);
			if (window == null)
			{
				Debug.WriteLine("SnapLayoutBehavior: ERROR - Could not get window from button");
				return;
			}

			Debug.WriteLine($"SnapLayoutBehavior: Window found: {window.Title}");

			var hwnd = new WindowInteropHelper(window).Handle;
			if (hwnd == IntPtr.Zero)
			{
				Debug.WriteLine("SnapLayoutBehavior: Window handle not ready, waiting for SourceInitialized");
				// Om fönstret inte har en handle än, vänta
				window.SourceInitialized += (s, e) => InitializeWindow(window, button);
			}
			else
			{
				InitializeWindow(window, button);
			}

			SetIsRegistered(button, true);
		}

		private static void InitializeWindow(Window window, Button button)
		{
			var hwnd = new WindowInteropHelper(window).Handle;
			if (hwnd == IntPtr.Zero)
			{
				Debug.WriteLine("SnapLayoutBehavior: ERROR - Window handle is still Zero");
				return;
			}

			Debug.WriteLine($"SnapLayoutBehavior: Window handle: {hwnd}");

			// Aktivera Windows 11 rounded corners
			try
			{
				int preference = 2; // DWMWCP_ROUND
				int result = DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(int));
				Debug.WriteLine($"SnapLayoutBehavior: DwmSetWindowAttribute result: {result}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"SnapLayoutBehavior: DwmSetWindowAttribute failed: {ex.Message}");
			}

			// Hook into window messages
			var source = HwndSource.FromHwnd(hwnd);
			if (source != null)
			{
				Debug.WriteLine("SnapLayoutBehavior: HwndSource found, adding message hook");
				var hook = new WindowMessageHook(window, button);
				source.AddHook(hook.WndProc);

				// Spara hooken för cleanup
				window.SetValue(WindowMessageHookProperty, hook);
			}
			else
			{
				Debug.WriteLine("SnapLayoutBehavior: ERROR - Could not get HwndSource");
			}
		}

		private static void UnregisterButton(Button button)
		{
			SetIsRegistered(button, false);

			var window = Window.GetWindow(button);
			if (window != null)
			{
				window.ClearValue(WindowMessageHookProperty);
			}
		}

		// Private property för att spara hook-instansen
		private static readonly DependencyProperty WindowMessageHookProperty =
			DependencyProperty.RegisterAttached(
				"WindowMessageHook",
				typeof(WindowMessageHook),
				typeof(SnapLayoutBehavior),
				new PropertyMetadata(null));

		// Privat klass för att hantera Windows messages
		private class WindowMessageHook
		{
			private readonly Window _window;
			private readonly Button _button;
			private int _hitTestCount = 0;

			public WindowMessageHook(Window window, Button button)
			{
				_window = window;
				_button = button;
				Debug.WriteLine("SnapLayoutBehavior: WindowMessageHook created");
			}

			public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				if (_button == null || _window == null)
					return IntPtr.Zero;

				switch (msg)
				{
					case WM_NCHITTEST:
						_hitTestCount++;
						if (_hitTestCount % 100 == 0) // Log varje 100:e för att inte spamma
						{
							Debug.WriteLine($"SnapLayoutBehavior: WM_NCHITTEST received (count: {_hitTestCount})");
						}
						return HandleHitTest(lParam, ref handled);

					case WM_NCLBUTTONDOWN:
						Debug.WriteLine($"SnapLayoutBehavior: WM_NCLBUTTONDOWN - wParam: {wParam.ToInt32()}");
						if (wParam.ToInt32() == HTMAXBUTTON)
						{
							Debug.WriteLine("SnapLayoutBehavior: HTMAXBUTTON button down detected!");
							handled = true;
						}
						break;

					case WM_NCLBUTTONUP:
						Debug.WriteLine($"SnapLayoutBehavior: WM_NCLBUTTONUP - wParam: {wParam.ToInt32()}");
						if (wParam.ToInt32() == HTMAXBUTTON)
						{
							Debug.WriteLine("SnapLayoutBehavior: HTMAXBUTTON button up detected! Toggling window state.");
							ToggleWindowState();
							handled = true;
						}
						break;
				}

				return IntPtr.Zero;
			}

			private IntPtr HandleHitTest(IntPtr lParam, ref bool handled)
			{
				try
				{
					// Få musens position från lParam (screen coordinates)
					int x = (short)(lParam.ToInt32() & 0xFFFF);
					int y = (short)((lParam.ToInt32() >> 16) & 0xFFFF);
					var mouseScreenPoint = new Point(x, y);

					// Få knappens bounds i screen coordinates
					var buttonScreenBounds = GetElementScreenBounds(_button);

					bool isOverButton = buttonScreenBounds.Contains(mouseScreenPoint);

					if (isOverButton)
					{
						Debug.WriteLine($"SnapLayoutBehavior: Mouse over button! Mouse({x},{y}) ButtonScreen({buttonScreenBounds})");
						handled = true;
						return new IntPtr(HTMAXBUTTON);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"SnapLayoutBehavior: HandleHitTest error: {ex.Message}");
				}

				return IntPtr.Zero;
			}

			private Rect GetElementScreenBounds(FrameworkElement element)
			{
				try
				{
					if (element.ActualWidth == 0 || element.ActualHeight == 0)
					{
						Debug.WriteLine($"SnapLayoutBehavior: WARNING - Button has no size! Width: {element.ActualWidth}, Height: {element.ActualHeight}");
						return Rect.Empty;
					}

					// Få elementets position i screen coordinates
					var topLeft = element.PointToScreen(new Point(0, 0));
					var bottomRight = element.PointToScreen(new Point(element.ActualWidth, element.ActualHeight));

					var rect = new Rect(topLeft, bottomRight);

					return rect;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"SnapLayoutBehavior: GetElementScreenBounds error: {ex.Message}");
					return Rect.Empty;
				}
			}

			private void ToggleWindowState()
			{
				_window.Dispatcher.BeginInvoke(new Action(() =>
				{
					try
					{
						if (_window.WindowState == WindowState.Maximized)
						{
							Debug.WriteLine("SnapLayoutBehavior: Restoring window to normal");
							_window.WindowState = WindowState.Normal;
						}
						else
						{
							Debug.WriteLine("SnapLayoutBehavior: Maximizing window");
							_window.WindowState = WindowState.Maximized;
						}
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"SnapLayoutBehavior: ToggleWindowState error: {ex.Message}");
					}
				}));
			}
		}
	}
}