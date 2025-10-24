using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace WpfTemplate.North.UI.Common.Behaviors
{
	public static class SnapLayoutsBehavior
	{
		public static readonly DependencyProperty MaximizeElementProperty =
			DependencyProperty.RegisterAttached(
				"MaximizeElement",
				typeof(FrameworkElement),
				typeof(SnapLayoutsBehavior),
				new PropertyMetadata(null, OnMaximizeElementChanged));

		private static readonly DependencyProperty StateProperty =
			DependencyProperty.RegisterAttached(
				"State",
				typeof(State),
				typeof(SnapLayoutsBehavior),
				new PropertyMetadata(null));

		public static void SetMaximizeElement(DependencyObject obj, FrameworkElement value) =>
			obj.SetValue(MaximizeElementProperty, value);

		public static FrameworkElement GetMaximizeElement(DependencyObject obj) =>
			(FrameworkElement)obj.GetValue(MaximizeElementProperty);

		private static void OnMaximizeElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is Window window)
			{
				// Släpp ev. tidigare state
				(window.GetValue(StateProperty) as State)?.Dispose();
				window.ClearValue(StateProperty);

				if (e.NewValue is FrameworkElement fe)
				{
					var state = new State(window, fe);
					window.SetValue(StateProperty, state);
				}
			}
		}

		private sealed class State : IDisposable
		{
			private readonly Window _window;
			private readonly FrameworkElement _maxButton;
			private HwndSource? _source;
			private HwndSourceHook? _hook;
			private Rect _buttonRectClientPx = Rect.Empty;

			private const int WM_NCHITTEST = 0x0084;
			private const int HTMAXBUTTON = 9;
			private const int VK_LBUTTON = 0x01;

			private const int GWL_STYLE = -16;
			private const uint WS_MAXIMIZEBOX = 0x00010000;
			private const uint WS_MINIMIZEBOX = 0x00020000;
			private const uint WS_THICKFRAME = 0x00040000;
			private const uint WS_SYSMENU = 0x00080000;

			public State(Window window, FrameworkElement maxButton)
			{
				_window = window;
				_maxButton = maxButton;

				if (_window.IsLoaded)
					Attach();
				else
					_window.Loaded += OnLoaded;
			}

			private void OnLoaded(object? sender, RoutedEventArgs e)
			{
				_window.Loaded -= OnLoaded;
				Attach();
			}

			private void Attach()
			{
				_source = (HwndSource?)PresentationSource.FromVisual(_window);
				if (_source is null)
				{
					_window.SourceInitialized += OnSourceInitialized;
				}
				else
				{
					AddHook(_source);
				}

				_maxButton.LayoutUpdated += OnLayoutUpdated;
				_window.SizeChanged += OnLayoutUpdated;
				_window.LocationChanged += OnLayoutUpdated;
				UpdateButtonRect();
			}

			[DllImport("user32.dll", SetLastError = true)]
			private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

			[DllImport("user32.dll", SetLastError = true)]
			private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

			private static void EnsureSnapRequiredStyles(Window window)
			{
				var hwnd = new WindowInteropHelper(window).Handle;
				if (hwnd == IntPtr.Zero) return;

				var style = (ulong)(long)GetWindowLongPtr(hwnd, GWL_STYLE);

				style |= WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_SYSMENU | WS_THICKFRAME;

				SetWindowLongPtr(hwnd, GWL_STYLE, (IntPtr)(long)style);
			}

			private void OnSourceInitialized(object? sender, EventArgs e)
			{
				_window.SourceInitialized -= OnSourceInitialized;
				_source = (HwndSource?)PresentationSource.FromVisual(_window);
				if (_source != null)
				{
					EnsureSnapRequiredStyles(_window);
					AddHook(_source);
				}
			}

			private void AddHook(HwndSource source)
			{
				EnsureSnapRequiredStyles(_window);
				_hook = new HwndSourceHook(Hook);
				source.AddHook(_hook);
			}

			private void OnLayoutUpdated(object? sender, EventArgs e) => UpdateButtonRect();

			private Rect _buttonRectScreenPx = Rect.Empty;

			private void UpdateButtonRect()
			{
				if (!_maxButton.IsVisible || _maxButton.ActualWidth <= 0 || _maxButton.ActualHeight <= 0)
				{
					_buttonRectClientPx = Rect.Empty;
					return;
				}

				// WPF ger skärmpx direkt:
				var tl = _maxButton.PointToScreen(new Point(0, 0));
				var br = _maxButton.PointToScreen(new Point(_maxButton.ActualWidth, _maxButton.ActualHeight));
				_buttonRectScreenPx = new Rect(tl, br);

				//// MaxButton -> window-klient (DIPs)
				//var topLeft = _maxButton.TransformToAncestor(_window).Transform(new Point(0, 0));
				//var dipsRect = new Rect(topLeft, new Size(_maxButton.ActualWidth, _maxButton.ActualHeight));

				//// DIPs -> device px
				//var dpi = VisualTreeHelper.GetDpi(_window);
				//_buttonRectClientPx = new Rect(
				//	dipsRect.X * dpi.DpiScaleX,
				//	dipsRect.Y * dpi.DpiScaleY,
				//	dipsRect.Width * dpi.DpiScaleX,
				//	dipsRect.Height * dpi.DpiScaleY);
			}

			[DllImport("user32.dll", SetLastError = false)]
			private static extern short GetKeyState(int nVirtKey);

			private static bool IsLeftMouseDown() => (GetKeyState(VK_LBUTTON) & 0x8000) != 0;

			private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				//if (msg == WM_NCHITTEST && !_buttonRectClientPx.IsEmpty)
				//{
				//	// Om vänster knapp är nedtryckt: låt WPF få klicket
				//	if (IsLeftMouseDown())
				//		return IntPtr.Zero;

				//	// Annars (hover / flytt): rapportera HTMAXBUTTON för snap-flyout
				//	int xScreen = LOWORD(lParam);
				//	int yScreen = HIWORD(lParam);
				//	var pt = new POINT { X = xScreen, Y = yScreen };
				//	ScreenToClient(hwnd, ref pt);

				//	if (_buttonRectClientPx.Contains(pt.X, pt.Y))
				//	{
				//		handled = true;
				//		return (IntPtr)HTMAXBUTTON;
				//	}
				//}

				//return IntPtr.Zero;

				if (msg == WM_NCHITTEST && !_buttonRectScreenPx.IsEmpty)
				{
					if (IsLeftMouseDown())
						return IntPtr.Zero; // låt WPF få klicket

					// lParam är SKÄRMKOORDINATER (x=LOWORD, y=HIWORD)
					int xScreen = unchecked((short)((long)lParam & 0xffff));
					int yScreen = unchecked((short)(((long)lParam >> 16) & 0xffff));

					if (_buttonRectScreenPx.Contains(xScreen, yScreen))
					{
						handled = true;
						return (IntPtr)9; // HTMAXBUTTON
					}
				}

				return IntPtr.Zero;
			}

			public void Dispose()
			{
				if (_source != null && _hook != null)
				{
					_source.RemoveHook(_hook);
				}

				_maxButton.LayoutUpdated -= OnLayoutUpdated;
				_window.SizeChanged -= OnLayoutUpdated;
				_window.LocationChanged -= OnLayoutUpdated;
			}

			private static int LOWORD(IntPtr ptr) => unchecked((short)((long)ptr & 0xffff));
			private static int HIWORD(IntPtr ptr) => unchecked((short)(((long)ptr >> 16) & 0xffff));

			[StructLayout(LayoutKind.Sequential)]
			private struct POINT { public int X; public int Y; }

			[DllImport("user32.dll", SetLastError = true)]
			private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
		}
	}
}
