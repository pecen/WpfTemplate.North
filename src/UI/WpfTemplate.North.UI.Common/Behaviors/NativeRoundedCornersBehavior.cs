using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WpfTemplate.North.UI.Common.Behaviors
{
	/// <summary>
	/// Attached Property för att aktivera Windows 11 native rounded corners
	/// Använd i XAML: behaviors:NativeRoundedCornersBehavior.Enable="True"
	/// </summary>
	public static class NativeRoundedCornersBehavior
	{
		// DWM Attributes
		private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
		private const int DWMWA_BORDER_COLOR = 34;
		private const int DWMWA_CAPTION_COLOR = 35;

		// Corner preferences
		public enum DWM_WINDOW_CORNER_PREFERENCE
		{
			DWMWCP_DEFAULT = 0,      // Let system decide
			DWMWCP_DONOTROUND = 1,   // Never round
			DWMWCP_ROUND = 2,        // Round if appropriate
			DWMWCP_ROUNDSMALL = 3    // Small rounded corners
		}

		[DllImport("dwmapi.dll", PreserveSig = true)]
		private static extern int DwmSetWindowAttribute(
			IntPtr hwnd,
			int attr,
			ref int attrValue,
			int attrSize);

		public static readonly DependencyProperty EnableProperty =
			DependencyProperty.RegisterAttached(
				"Enable",
				typeof(bool),
				typeof(NativeRoundedCornersBehavior),
				new PropertyMetadata(false, OnEnableChanged));

		public static readonly DependencyProperty CornerPreferenceProperty =
			DependencyProperty.RegisterAttached(
				"CornerPreference",
				typeof(DWM_WINDOW_CORNER_PREFERENCE),
				typeof(NativeRoundedCornersBehavior),
				new PropertyMetadata(DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND));

		public static bool GetEnable(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnableProperty);
		}

		public static void SetEnable(DependencyObject obj, bool value)
		{
			obj.SetValue(EnableProperty, value);
		}

		public static DWM_WINDOW_CORNER_PREFERENCE GetCornerPreference(DependencyObject obj)
		{
			return (DWM_WINDOW_CORNER_PREFERENCE)obj.GetValue(CornerPreferenceProperty);
		}

		public static void SetCornerPreference(DependencyObject obj, DWM_WINDOW_CORNER_PREFERENCE value)
		{
			obj.SetValue(CornerPreferenceProperty, value);
		}

		private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is Window window) || !(bool)e.NewValue)
				return;

			Debug.WriteLine("NativeRoundedCornersBehavior: Enabling native rounded corners");

			if (window.IsLoaded)
			{
				ApplyRoundedCorners(window);
			}
			else
			{
				window.SourceInitialized += (s, args) => ApplyRoundedCorners(window);
			}
		}

		private static void ApplyRoundedCorners(Window window)
		{
			try
			{
				var hwnd = new WindowInteropHelper(window).Handle;
				if (hwnd == IntPtr.Zero)
				{
					Debug.WriteLine("NativeRoundedCornersBehavior: ERROR - Window handle is Zero");
					return;
				}

				Debug.WriteLine($"NativeRoundedCornersBehavior: Window handle: {hwnd}");

				// Hämta corner preference
				var preference = GetCornerPreference(window);
				int cornerPref = (int)preference;

				// Sätt rundade hörn
				int result = DwmSetWindowAttribute(
					hwnd,
					DWMWA_WINDOW_CORNER_PREFERENCE,
					ref cornerPref,
					sizeof(int));

				if (result == 0)
				{
					Debug.WriteLine($"NativeRoundedCornersBehavior: Successfully applied corner preference: {preference}");
				}
				else
				{
					Debug.WriteLine($"NativeRoundedCornersBehavior: DwmSetWindowAttribute failed with error: {result}");
				}

				// Verifiera att vi är på Windows 11
				var version = Environment.OSVersion.Version;
				if (version.Major < 10 || (version.Major == 10 && version.Build < 22000))
				{
					Debug.WriteLine($"NativeRoundedCornersBehavior: WARNING - Native rounded corners require Windows 11 (Build 22000+). Current: {version}");
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"NativeRoundedCornersBehavior: Exception: {ex.Message}");
			}
		}

		/// <summary>
		/// Optional: Sätt custom border color (Windows 11 only)
		/// </summary>
		public static void SetBorderColor(Window window, System.Windows.Media.Color color)
		{
			try
			{
				var hwnd = new WindowInteropHelper(window).Handle;
				if (hwnd == IntPtr.Zero)
					return;

				// Convert Color to COLORREF (0x00BBGGRR format)
				int colorRef = color.R | (color.G << 8) | (color.B << 16);

				DwmSetWindowAttribute(
					hwnd,
					DWMWA_BORDER_COLOR,
					ref colorRef,
					sizeof(int));

				Debug.WriteLine($"NativeRoundedCornersBehavior: Border color set to #{color.R:X2}{color.G:X2}{color.B:X2}");
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"NativeRoundedCornersBehavior: SetBorderColor failed: {ex.Message}");
			}
		}
	}
}