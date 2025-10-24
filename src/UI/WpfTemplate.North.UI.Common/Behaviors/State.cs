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
	sealed class State : IDisposable
	{
		private readonly Window _window;
		private readonly FrameworkElement _maxButton;
		private HwndSource? _source;
		private HwndSourceHook? _hook;
		private Rect _buttonRectClientPx = Rect.Empty;

		private const int WM_NCHITTEST = 0x0084;
		private const int HTMAXBUTTON = 9;

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

		private void OnSourceInitialized(object? sender, EventArgs e)
		{
			_window.SourceInitialized -= OnSourceInitialized;
			_source = (HwndSource?)PresentationSource.FromVisual(_window);
			if (_source != null)
				AddHook(_source);
		}

		private void AddHook(HwndSource source)
		{
			_hook = new HwndSourceHook(Hook);
			source.AddHook(_hook);
		}

		private void OnLayoutUpdated(object? sender, EventArgs e) => UpdateButtonRect();

		private void UpdateButtonRect()
		{
			if (!_maxButton.IsVisible || _maxButton.ActualWidth <= 0 || _maxButton.ActualHeight <= 0)
			{
				_buttonRectClientPx = Rect.Empty;
				return;
			}

			// MaxButton -> window-klient (DIPs)
			var topLeft = _maxButton.TransformToAncestor(_window).Transform(new Point(0, 0));
			var dipsRect = new Rect(topLeft, new Size(_maxButton.ActualWidth, _maxButton.ActualHeight));

			// DIPs -> device px
			var dpi = VisualTreeHelper.GetDpi(_window);
			_buttonRectClientPx = new Rect(
				dipsRect.X * dpi.DpiScaleX,
				dipsRect.Y * dpi.DpiScaleY,
				dipsRect.Width * dpi.DpiScaleX,
				dipsRect.Height * dpi.DpiScaleY);
		}

		private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == WM_NCHITTEST && !_buttonRectClientPx.IsEmpty)
			{
				// lParam = skärm-koordinater (x i LOWORD, y i HIWORD)
				int xScreen = LOWORD(lParam);
				int yScreen = HIWORD(lParam);
				var pt = new POINT { X = xScreen, Y = yScreen };

				// Skärm -> klient
				ScreenToClient(hwnd, ref pt);

				if (_buttonRectClientPx.Contains(pt.X, pt.Y))
				{
					handled = true;
					return (IntPtr)HTMAXBUTTON; // <- triggar Win11 snap-layouts-flyout
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

