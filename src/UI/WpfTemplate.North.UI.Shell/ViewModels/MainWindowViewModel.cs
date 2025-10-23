using Prism.Commands;
using Prism.Regions;
using System.Windows;
using WpfTemplate.North.UI.Common.Enums;
using WpfTemplate.North.UI.Common.MVVM.Prism;

namespace WpfTemplate.North.UI.Shell.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private readonly IRegionManager _regionManager;

		#region Commands

		public DelegateCommand MoveWindowCommand { get; set; }
		public DelegateCommand MinimizeWindowCommand { get; set; }
		public DelegateCommand MaximizeWindowCommand { get; set; }
		public DelegateCommand ShutdownWindowCommand { get; set; }

		#endregion

		#region Properties

		public string LeftMenuBarRegion => RegionNames.LeftMenuBarRegion.ToString();
		public string ContentRegion => RegionNames.ContentRegion.ToString();

		private string _maximizeBtnTooltip = "Maximize Window";
		public string MaximizeBtnTooltip
		{
			get => _maximizeBtnTooltip;
			set => SetProperty(ref _maximizeBtnTooltip, value);
		}

		#endregion

		public MainWindowViewModel(IRegionManager regionManager)
		{
			Title = "Wpf Template - North Theme";
			_regionManager = regionManager;

			//_regionManager.RegisterViewWithRegion(RegionNames.ContentRegion.ToString(), typeof(StartView));

			ShutdownWindowCommand = new DelegateCommand(Application.Current.MainWindow.Close);
			MoveWindowCommand = new DelegateCommand(Application.Current.MainWindow.DragMove);
			MinimizeWindowCommand = new DelegateCommand(() => Application.Current.MainWindow.WindowState = WindowState.Minimized);

			MaximizeWindowCommand = new DelegateCommand(() =>
			{
				if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
				{
					Application.Current.MainWindow.WindowState = WindowState.Normal;
					MaximizeBtnTooltip = "Maximize Window";
				}
				else
				{
					Application.Current.MainWindow.WindowState = WindowState.Maximized;
					MaximizeBtnTooltip = "Restore to Normal State";
				}
			});
		}
	}
}
