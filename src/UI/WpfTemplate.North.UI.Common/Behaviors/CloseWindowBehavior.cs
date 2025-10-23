using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Xaml.Behaviors;
using System.Windows;

namespace WpfTemplate.North.UI.Common.Behaviors
{
	public class CloseWindowBehavior : Behavior<Window>
	{
		private bool _shutdown;

		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.Closing += AssociatedObjectClosing;
		}

		public void AssociatedObjectClosing(object? sender, System.ComponentModel.CancelEventArgs e)
		{
			if (e.Cancel)
			{
				return;
			}

			if (!_shutdown)
			{
				e.Cancel = true;

				if (sender is MetroWindow window)
				{
					// We have to delay the execution through BeginInvoke to prevent potential re-entrancy
					window.Dispatcher.BeginInvoke(new Action(async () => await ConfirmShutdown(window)));
				}
			}
		}

		private async Task ConfirmShutdown(MetroWindow window)
		{
			var mySettings = new MetroDialogSettings
			{
				AffirmativeButtonText = "Quit",
				NegativeButtonText = "Cancel",
				AnimateShow = true,
				AnimateHide = true
			};

			var result = await window.ShowMessageAsync("You are about to Quit the application!",
													 "Sure you want to quit the application?",
													 MessageDialogStyle.AffirmativeAndNegative,
													 mySettings);

			_shutdown = result == MessageDialogResult.Affirmative;

			if (_shutdown)
			{
				Application.Current.Shutdown();
			}
		}

		protected override void OnDetaching()
		{
			AssociatedObject.Closing -= AssociatedObjectClosing;
			base.OnDetaching();
		}
	}
}
